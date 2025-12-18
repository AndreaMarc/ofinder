# Analisi Ottimizzazione JsonApiManualService
**Data**: 2025-12-14
**Commit analizzato**: `8e05a03` - "Forzate modifiche per funzionamento api ma da risistemare"

---

## Problema Generale

Durante la migrazione da .NET 8 a .NET 10, sono stati fatti fix rapidi per far funzionare l'API con MySQL. Questi fix hanno risolto gli errori immediati ma hanno creato **problemi di performance** (N+1 query problem, materializzazione eccessiva).

### Errori comuni .NET 10 + MySQL:
1. **Translation error**: `roles.Select(x => x.Id).Contains()` non traducibile in SQL
2. **Open DataReader**: Foreach su IQueryable con query interne
3. **N+1 problem**: Loop con query separate invece di JOIN/bulk query

---

## Metodi da Ottimizzare

### 1. GetAllRoleClaimsInRoles (linee 702-716)

#### Codice Originale (pre-.NET 10)
```csharp
public List<RoleClaim> GetAllRoleClaimsInRoles(List<Role> roles)
{
    return [.. _context.RoleClaims.Where(x => roles.Select(x => x.Id).Contains(x.RoleId))];
}
```

#### Errore .NET 10
```
System.InvalidOperationException: The LINQ expression 'DbSet<RoleClaim>
    .Where(r => roles.Select(r => r.Id).Contains(r.RoleId))' could not be translated.
```

**Causa**: `roles.Select(x => x.Id)` all'interno della query viene valutato lato client. EF Core 10 è più rigido e non riesce a tradurre questa espressione in SQL.

#### Fix Attuale (PROBLEMATICO - N+1)
```csharp
public List<RoleClaim> GetAllRoleClaimsInRoles(List<Role> roles)
{
    // Fix MySQL: Materializza prima gli ID per evitare type mapping error
    List<string> roleIds = roles.Select(x => x.Id).ToList();

    // Query separata per ogni role - MySQL gestisce facilmente query semplici con indici
    List<RoleClaim> allClaims = new List<RoleClaim>();
    foreach (string roleId in roleIds)
    {
        List<RoleClaim> claims = _context.RoleClaims
            .Where(x => x.RoleId == roleId)
            .ToList();
        allClaims.AddRange(claims);
    }
    return allClaims;
}
```

**Problema**: Se hai 100 role, fai **100 query separate** invece di 1!

#### Soluzione Ottimale
```csharp
public async Task<List<RoleClaim>> GetAllRoleClaimsInRoles(List<Role> roles)
{
    // 1. Materializza SOLO gli ID (pochi byte), non le entity intere
    List<string> roleIds = roles.Select(x => x.Id).ToList();

    // 2. SINGOLA query sul DB con lista materializzata
    return await _context.RoleClaims
        .Where(x => roleIds.Contains(x.RoleId))
        .ToListAsync();
}
```

**Performance**:
- Prima: N query (una per role)
- Dopo: **1 query** con IN clause: `WHERE RoleId IN ('id1', 'id2', ...)`

---

### 2. CheckIsSuperAdmin / CheckIsOwner (linee 1008-1044)

#### Codice Originale
```csharp
public async Task<bool> CheckIsSuperAdmin(string id)
{
    User user = await GetUserById(id);
    IEnumerable<UserRole> userRoles = (await GetAllUserRolesByEmail(user.Email))
        .Where(x => x.TenantId == 1);

    IQueryable<RoleClaim> allClaims = _context.RoleClaims
        .Where(x => userRoles.Select(x => x.RoleId).Contains(x.RoleId));

    return userRoles.Any(role =>
        allClaims.Any(x => x.RoleId == role.RoleId && x.ClaimValue == "isSuperAdmin"));
}
```

#### Errore .NET 10
```
System.InvalidOperationException: There is already an open DataReader associated
with this Connection which must be closed first.
```

**Causa**:
1. `userRoles` è IEnumerable non materializzato
2. Doppio `.Any()` crea query nidificate
3. MySQL/.NET 10 non gestisce DataReader multipli aperti

#### Fix Attuale (PROBLEMATICO - N query)
```csharp
public async Task<bool> CheckIsSuperAdmin(string id)
{
    User user = await GetUserById(id);

    List<UserRole> userRoles = (await GetAllUserRolesByEmail(user.Email))
        .Where(x => x.TenantId == 1)
        .ToList();

    if (!userRoles.Any()) return false;

    // Query separata per ogni role - MySQL gestisce facilmente query semplici
    foreach (var userRole in userRoles)
    {
        bool hasClaim = await _context.RoleClaims
            .AnyAsync(x => x.RoleId == userRole.RoleId && x.ClaimValue == "isSuperAdmin");

        if (hasClaim) return true;
    }

    return false;
}
```

**Problema**: Se utente ha 10 ruoli, fai **10 query**!

#### Soluzione Ottimale
```csharp
public async Task<bool> CheckIsSuperAdmin(string id)
{
    User user = await GetUserById(id);

    // 1. Materializza userRoles (OK, sono pochi record)
    List<UserRole> userRoles = (await GetAllUserRolesByEmail(user.Email))
        .Where(x => x.TenantId == 1)
        .ToList();

    if (!userRoles.Any()) return false;

    // 2. Estrai solo gli ID (pochi byte)
    List<string> roleIds = userRoles.Select(x => x.RoleId).ToList();

    // 3. SINGOLA query con AnyAsync
    return await _context.RoleClaims
        .AnyAsync(x => roleIds.Contains(x.RoleId) && x.ClaimValue == "isSuperAdmin");
}
```

**Performance**:
- Prima: N query (una per UserRole)
- Dopo: **1 query** con `WHERE RoleId IN (...) AND ClaimValue = 'isSuperAdmin'`

**Stesso fix per CheckIsOwner** (cambia solo "isSuperAdmin" → "isOwner").

---

### 3. GetTemplateByKeyLC (linee 1059-1076)

#### Codice Originale
```csharp
public async Task<Template> GetTemplateByKeyLC(string code, string language)
{
    Template res = null;

    IQueryable<Template> templates = _context.Templates
        .Where(x => x.Code == code && x.Language == language);

    foreach (Template template in templates)
    {
        if (res == null || template.Version > res.Version)
        {
            res = template;
        }
    }

    return res;
}
```

#### Errore .NET 10
```
System.InvalidOperationException: There is already an open DataReader...
```

**Causa**: Il `foreach` su `IQueryable` tiene aperto un DataReader mentre itera.

#### Fix Attuale (PROBLEMATICO - materializza tutto)
```csharp
public async Task<Template> GetTemplateByKeyLC(string code, string language)
{
    Template res = null;

    List<Template> templates = _context.Templates
        .Where(x => x.Code == code && x.Language == language)
        .ToList(); // Carica TUTTE le versioni!

    foreach (Template template in templates)
    {
        if (res == null || template.Version > res.Version)
        {
            res = template;
        }
    }

    return res;
}
```

**Problema**: Se hai 100 versioni del template con stesso code/language, le carica **TUTTE in memoria** per trovare la più recente!

#### Soluzione Ottimale
```csharp
public async Task<Template> GetTemplateByKeyLC(string code, string language)
{
    // Lascia che il DB faccia il lavoro! Ha indici ottimizzati per ordinare.
    return await _context.Templates
        .Where(x => x.Code == code && x.Language == language)
        .OrderByDescending(x => x.Version)
        .FirstOrDefaultAsync();
}
```

**Performance**:
- Prima: Carica N template in memoria, loop per trovare max
- Dopo: **1 query** con `ORDER BY Version DESC LIMIT 1`

---

### 4. GetClaimsByUsername (linee 1691-1711)

#### Codice Originale
```csharp
public Task<bool> GetClaimsByUsername(string username, string tenantId)
{
    User user = _context.Users.FirstOrDefault(x => x.UserName == username);
    if (user == null) { return Task.FromResult(false); }

    IQueryable<UserRole> userRoles = tenantId == ""
        ? _context.UserRoles.Where(x => x.UserId == user.Id)
        : _context.UserRoles.Where(x => x.UserId == user.Id &&
            (x.TenantId.ToString() == tenantId || x.TenantId == 1));

    List<string> claimNames = [];
    IQueryable<RoleClaim> allRoleClaims = _context.RoleClaims
        .Where(x => userRoles.Select(x => x.RoleId).Contains(x.RoleId));

    foreach (UserRole userRole in userRoles)
    {
        Role role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
        if (role == null) { continue; }
        IQueryable<RoleClaim> roleClaims = allRoleClaims.Where(x => x.RoleId == role.Id);

        foreach (RoleClaim claim in roleClaims)
        {
            if (!claimNames.Contains(claim.ClaimValue))
            {
                claimNames.Add(claim.ClaimValue);
            }
        }
    }
    // ...
}
```

#### Errori .NET 10
1. `userRoles.Select().Contains()` → Translation error
2. `foreach` su `IQueryable` + query interne → Open DataReader
3. **N+1 problem già presente nel codice originale!**

#### Fix Attuale (PEGGIORE - N*2 query)
```csharp
// Fix MySQL: Materializza userRoles per evitare DataReader error
List<UserRole> userRoles = (tenantId == ""
    ? _context.UserRoles.Where(x => x.UserId == user.Id)
    : _context.UserRoles.Where(x => x.UserId == user.Id &&
        (x.TenantId.ToString() == tenantId || x.TenantId == 1)))
    .ToList();

List<string> claimNames = [];

// Fix MySQL: Query separata per ogni role invece di usare Select().Contains()
foreach (UserRole userRole in userRoles)
{
    Role role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId); // Query 1
    if (role == null) { continue; }

    // Query separata materializzata - MySQL gestisce facilmente query semplici con indici
    List<RoleClaim> roleClaims = _context.RoleClaims
        .Where(x => x.RoleId == role.Id)
        .ToList(); // Query 2

    foreach (RoleClaim claim in roleClaims)
    {
        if (!claimNames.Contains(claim.ClaimValue))
        {
            claimNames.Add(claim.ClaimValue);
        }
    }
}
```

**Problema**: Se utente ha 10 UserRole: **10 query per Role + 10 query per RoleClaims = 20 query totali!**

#### Soluzione Ottimale
```csharp
public async Task<string> GetClaimsByUsername(string username, string tenantId)
{
    User user = _context.Users.FirstOrDefault(x => x.UserName == username);
    if (user == null) { return Task.FromResult(""); }

    // 1. Materializza userRoles (OK, pochi record)
    List<UserRole> userRoles = (tenantId == ""
        ? _context.UserRoles.Where(x => x.UserId == user.Id)
        : _context.UserRoles.Where(x => x.UserId == user.Id &&
            (x.TenantId.ToString() == tenantId || x.TenantId == 1)))
        .ToList();

    if (!userRoles.Any()) return "";

    // 2. Carica TUTTI i role necessari in UNA query
    List<string> roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
    Dictionary<string, Role> roles = await _context.Roles
        .Where(x => roleIds.Contains(x.Id))
        .ToDictionaryAsync(x => x.Id, x => x);

    // 3. Carica TUTTI i roleClaims in UNA query
    List<RoleClaim> allRoleClaims = await _context.RoleClaims
        .Where(x => roleIds.Contains(x.RoleId))
        .ToListAsync();

    // 4. Processa in memoria (efficiente - i dati sono già caricati)
    HashSet<string> claimNames = new HashSet<string>(); // Usa HashSet per Contains() O(1)

    foreach (UserRole userRole in userRoles)
    {
        if (!roles.TryGetValue(userRole.RoleId, out Role role)) continue;

        // Filter in memoria (efficiente - lista già caricata)
        foreach (RoleClaim claim in allRoleClaims.Where(x => x.RoleId == role.Id))
        {
            claimNames.Add(claim.ClaimValue); // HashSet ignora automaticamente duplicati
        }
    }

    return string.Join(",", claimNames);
}
```

**Performance**:
- Prima: 1 + N + N = **2N + 1 query** (se 10 UserRole: 21 query)
- Dopo: 1 + 1 + 1 = **3 query totali** (fisso, indipendentemente da quanti UserRole)

**Ottimizzazioni bonus**:
- Usa `Dictionary` per lookup O(1) invece di `FirstOrDefault` O(n)
- Usa `HashSet` invece di `List.Contains()` per performance O(1) vs O(n)

---

## Pattern di Ottimizzazione

### Errore: `Select().Contains()` non traducibile
**Fix Sbagliato**:
```csharp
foreach (var item in items) {
    var result = _context.Entity.Where(x => x.Id == item.Id).ToList();
}
```

**Fix Corretto**:
```csharp
List<string> ids = items.Select(x => x.Id).ToList(); // Materializza SOLO gli ID
var result = await _context.Entity
    .Where(x => ids.Contains(x.Id))
    .ToListAsync();
```

### Errore: Open DataReader
**Fix Sbagliato**:
```csharp
List<Entity> all = _context.Entity.Where(...).ToList(); // Carica tutto
```

**Fix Corretto**:
```csharp
// Materializza SOLO ciò che serve per il filtro
List<int> ids = _context.RelatedEntity.Where(...).Select(x => x.Id).ToList();
var result = await _context.Entity.Where(x => ids.Contains(x.Id)).ToListAsync();
```

### Errore: N+1 Problem
**Fix Sbagliato**:
```csharp
foreach (var item in items) {
    var related = await _context.Related.FirstOrDefaultAsync(x => x.Id == item.RelatedId);
}
```

**Fix Corretto**:
```csharp
// Carica TUTTO in 1-2 query, poi processa in memoria
List<int> relatedIds = items.Select(x => x.RelatedId).Distinct().ToList();
Dictionary<int, Related> relatedDict = await _context.Related
    .Where(x => relatedIds.Contains(x.Id))
    .ToDictionaryAsync(x => x.Id, x => x);

foreach (var item in items) {
    if (relatedDict.TryGetValue(item.RelatedId, out var related)) {
        // Usa related...
    }
}
```

---

## Riepilogo Modifiche Necessarie

| Metodo | Linee | Query Attuali | Query Ottimali | Riduzione |
|--------|-------|---------------|----------------|-----------|
| GetAllRoleClaimsInRoles | 702-716 | N query | 1 query | **-99%** (se N=100) |
| CheckIsSuperAdmin | 1008-1023 | N query | 1 query | **-90%** (se N=10) |
| CheckIsOwner | 1025-1044 | N query | 1 query | **-90%** (se N=10) |
| GetTemplateByKeyLC | 1059-1076 | 1 query + loop | 1 query ottimizzata | **-50%** memoria |
| GetClaimsByUsername | 1691-1711 | 2N+1 query | 3 query | **-85%** (se N=10) |

---

## Metodi Già Ottimizzati ✅

Questi metodi sono stati ottimizzati CORRETTAMENTE con JOIN:

### GetTenantsByUsername (linee 1625-1636)
```csharp
// ✅ Usa JOIN - ottimo!
List<Tenant> tenants = _context.UserTenants
    .Where(x => x.UserId == user.Id && (x.State == "accepted" || x.State == "selfCreated" || x.State == "ownerCreated"))
    .Select(x => x.TenantId)
    .Distinct()
    .Join(_context.Tenants,
          tenantId => tenantId,
          tenant => tenant.Id,
          (tenantId, tenant) => tenant)
    .ToList();
```

### GetNonBlockedTenantsByUserId (linee 1639-1658)
```csharp
// ✅ Usa JOIN - ottimo!
List<Tenant> tenants = _context.UserTenants
    .Where(x => x.UserId == userId && (x.State == "accepted" || x.State == "selfCreated" || x.State == "ownerCreated"))
    .Where(x => !bannedTenantsId.Contains(x.TenantId))
    .Select(x => x.TenantId)
    .Distinct()
    .Join(_context.Tenants,
          tenantId => tenantId,
          tenant => tenant.Id,
          (tenantId, tenant) => tenant)
    .ToList();
```

**Nota**: Questi metodi dimostrano il pattern corretto da seguire!

---

## Altri Metodi da Analizzare (Non Critici)

Questi metodi usano `.ToList()` ma in contesti dove è probabilmente necessario (operazioni di DELETE massive):

- **DeleteTenantReferiments** (linea 1294+): Materializza prima di DELETE per evitare DataReader issues
- **MoveMediaCategory** (linea 1116+): Materializza prima di UPDATE/DELETE
- **CreateTenantReferiments** (linea 739+): Materializza prima di CREATE in loop

**Nota**: Per operazioni DELETE/UPDATE in loop, materializzare può essere necessario. Ma si potrebbe considerare `ExecuteDeleteAsync()` / `ExecuteUpdateAsync()` (EF Core 7+) per operazioni bulk.

---

## Prossimi Passi

1. **Testare le ottimizzazioni**:
   - Creare branch `feature/optimize-jsonapimanualservice`
   - Applicare fix uno per uno
   - Testare ogni metodo con dati reali

2. **Misurare performance**:
   - Aggiungere logging per contare query SQL
   - Confrontare tempo esecuzione prima/dopo
   - Usare SQL Profiler o EF Core logging

3. **Considerare refactoring più ampio**:
   - Spostare logica complessa in Stored Procedures (se MySQL lo supporta bene)
   - Usare Redis per cache di claims/roles (cambiano raramente)
   - Implementare `IMemoryCache` per GetClaimsByUsername (chiamato spesso)

---

## Testing Consigliato

```csharp
// Abilita logging SQL in appsettings.json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

Poi osserva i log per vedere:
- Prima: 20+ query per GetClaimsByUsername
- Dopo: 3 query totali

---

## Note Tecniche .NET 8 → .NET 10

### Cambiamenti EF Core che hanno causato errori:

1. **Translation più rigida**: EF Core 10 non traduce espressioni client-side valutate nella query
2. **DataReader più strict**: MySQL connector non permette DataReader multipli aperti
3. **Type inference migliore ma più strict**: `Select().Contains()` richiede tipi esatti

### Soluzioni generali:

- ✅ Materializza collezioni PRIMA di usarle in query: `.Select(x => x.Id).ToList()`
- ✅ Usa `AsNoTracking()` per query read-only (performance)
- ✅ Preferisci query bulk invece di loop con query
- ✅ Usa `Dictionary` e `HashSet` per lookup in memoria O(1)
- ✅ Chiudi sempre DataReader con `.ToList()` / `.ToListAsync()` quando necessario

---

**Fine Analisi**
Pronto per l'implementazione quando necessario.
