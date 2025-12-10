# MIT Framework - Entity Framework Migrations Guide

## Sommario

- [1. Concetti Fondamentali](#1-concetti-fondamentali)
  - [1.1 Entity Framework Migrations](#11-entity-framework-migrations)
  - [1.2 Architettura Multi-DbContext](#12-architettura-multi-dbcontext)
  - [1.3 Sistema di Auto-Discovery](#13-sistema-di-auto-discovery)
  - [1.4 Tabelle Migrations Separate](#14-tabelle-migrations-separate)
  - [1.5 Pattern di Ordinamento](#15-pattern-di-ordinamento)
- [2. Modalità Operative](#2-modalità-operative)
  - [2.1 Auto-Migrations (EnableAutoMigrations)](#21-auto-migrations-enableautomigrations)
  - [2.2 Script PowerShell (Add-Migration.ps1)](#22-script-powershell-add-migrationps1)
  - [2.3 EF Core CLI (Operazioni Avanzate)](#23-ef-core-cli-operazioni-avanzate)
- [3. Script PowerShell - Guida Dettagliata](#3-script-powershell---guida-dettagliata)
  - [3.1 Parametri e Sintassi](#31-parametri-e-sintassi)
  - [3.2 Operazioni Disponibili](#32-operazioni-disponibili)
  - [3.3 Esempi Pratici](#33-esempi-pratici)
- [4. Configurazione](#4-configurazione)
  - [4.1 customsettings.json](#41-customsettingsjson)
  - [4.2 dbconnections.json](#42-dbconnectionsjson)
  - [4.3 Validazione Configurazione](#43-validazione-configurazione)
- [5. Workflow Sviluppatore](#5-workflow-sviluppatore)
  - [5.1 Creare Nuovo DbContext Custom](#51-creare-nuovo-dbcontext-custom)
  - [5.2 Generare Prima Migration](#52-generare-prima-migration)
  - [5.3 Modificare Entità e Creare Migration Incrementale](#53-modificare-entità-e-creare-migration-incrementale)
  - [5.4 Applicare Migrations](#54-applicare-migrations)
  - [5.5 Rimuovere Migration Errata](#55-rimuovere-migration-errata)
- [6. Scenari Avanzati](#6-scenari-avanzati)
  - [6.1 Rollback a Migration Specifica](#61-rollback-a-migration-specifica)
  - [6.2 DbContext Esterni (Senza Migrations)](#62-dbcontext-esterni-senza-migrations)
  - [6.3 Gestione Migration History Tables](#63-gestione-migration-history-tables)
  - [6.4 Data Seeding in Migrations](#64-data-seeding-in-migrations)
- [7. Riferimenti Rapidi](#7-riferimenti-rapidi)
  - [7.1 Comandi PowerShell Quick Reference](#71-comandi-powershell-quick-reference)
  - [7.2 Pattern Interfacce](#72-pattern-interfacce)
  - [7.3 File e Cartelle](#73-file-e-cartelle)

---

## 1. Concetti Fondamentali

### 1.1 Entity Framework Migrations

Le **migrations** di Entity Framework Core sono un sistema di versioning per lo schema del database. Consentono di:

- **Evolvere lo schema**: Aggiungere, modificare o rimuovere tabelle e colonne in modo incrementale
- **Tracciare cambiamenti**: Ogni migration è un file C# che descrive le modifiche allo schema
- **Sincronizzare modelli**: Mantenere allineati i modelli C# (entità) con la struttura del database
- **Rollback sicuro**: Tornare a una versione precedente dello schema tramite metodi `Up()` e `Down()`
- **Automazione**: Generare automaticamente il codice di migrazione confrontando il modello attuale con lo snapshot precedente

**Anatomia di una Migration**:

```csharp
public partial class AddProducts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Modifiche da applicare (forward)
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Products", x => x.Id); }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Modifiche per rollback (backward)
        migrationBuilder.DropTable(name: "Products");
    }
}
```

**ModelSnapshot**: File generato automaticamente che rappresenta lo stato corrente del modello. EF lo usa per rilevare cambiamenti tra una migration e la successiva.

---

### 1.2 Architettura Multi-DbContext

MIT Framework supporta **più DbContext contemporaneamente**, ognuno connesso a un database diverso:

```
JsonApiDbContext (database principale)
├── AspNetUsers, AspNetRoles (Identity)
├── Entities (entity standard del framework)
└── __EFMigrationsHistory_JsonApiDbContext

OtherDbContext (database custom #1)
├── Products, Categories (entità custom)
└── __EFMigrationsHistory_OtherDbContext

MyCompanyDbContext (database custom #2)
├── Orders, Customers (entità custom)
└── __EFMigrationsHistory_MyCompanyDbContext

NorthwindDbContext (database esterno, read-only)
├── Employees, Orders (tabelle esistenti)
└── Nessuna migration table (non gestito da framework)
```

**Vantaggi**:
- **Separazione domini**: Database separati per moduli indipendenti
- **Scaling selettivo**: Distribuire database su server diversi
- **Isolamento dati**: Multi-tenancy o segregazione per compliance
- **Integrazione legacy**: Connettersi a database esistenti (read-only)

**Gestione Migrations**:
- Ogni DbContext ha le **proprie migrations** nella cartella `Migrations/` del progetto
- Ogni DbContext ha una **migration history table separata** nel database
- Le migrations vengono applicate in **ordine specificato** (vedi DatabaseMigrationOrder)

---

### 1.3 Sistema di Auto-Discovery

Il framework usa un sistema di **auto-discovery basato su interfacce marker** per identificare quali DbContext devono avere le migrations gestite.

#### Interfaccia `IMigrationDbContext`

```csharp
// Src/MIT.Fwk.Core/Data/IMigrationDbContext.cs
public interface IMigrationDbContext
{
    // Marker interface - no methods required
}
```

**Quando implementare `IMigrationDbContext`**:
- ✅ DbContext che gestisci tu (framework o moduli custom)
- ✅ Database dove vuoi che il framework applichi migrations automaticamente
- ✅ Tabelle che possono essere modificate dalla tua applicazione

**Quando NON implementare `IMigrationDbContext`**:
- ❌ DbContext per database esterni/legacy (read-only)
- ❌ Database gestiti da altre applicazioni
- ❌ Connessioni a database di terze parti dove non hai controllo dello schema

#### Pattern di Implementazione

**Scenario A - DbContext Gestito (con migrations)**:

```csharp
// Src/MIT.Fwk.Examples/Data/OtherDbContext.cs
public class OtherDbContext : DbContext, IMigrationDbContext  // ← Include IMigrationDbContext
{
    public OtherDbContext(DbContextOptions<OtherDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
}
```

**Risultato**:
- Framework trova `OtherDbContext` tramite `IMigrationDbContext`
- Applica migrations automaticamente (se `EnableAutoMigrations: true`)
- Script PowerShell include `OtherDbContext` nella lista di discovery

**Scenario B - DbContext Esterno (senza migrations)**:

```csharp
// Src/MIT.Fwk.Examples/Data/NorthwindDbContext.cs
public class NorthwindDbContext : DbContext  // ← NO IMigrationDbContext
{
    public NorthwindDbContext(DbContextOptions<NorthwindDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Order> Orders => Set<Order>();
}
```

**Risultato**:
- Framework ignora `NorthwindDbContext` per le auto-migrations
- Script PowerShell NON lo include nella discovery
- Nessuna migration history table creata
- Usabile per query read-only o database gestiti esternamente

#### Processo di Auto-Discovery

**Al startup dell'applicazione** (`DatabaseMigrationService.ApplyMigrationsAsync()`):

1. **Scansione assemblies**:
   ```csharp
   var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
       .Where(a => a.FullName.StartsWith("MIT."))
       .SelectMany(a => a.GetTypes())
       .Where(t => typeof(IMigrationDbContext).IsAssignableFrom(t) &&
                   t.IsClass && !t.IsAbstract)
       .ToList();
   ```

2. **Ordinamento** (vedi sezione 1.5):
   ```csharp
   var orderedContexts = OrderDbContextsByConfiguration(dbContextTypes);
   ```

3. **Applicazione migrations**:
   ```csharp
   foreach (var contextType in orderedContexts)
   {
       var context = serviceProvider.GetService(contextType) as DbContext;
       await context.Database.MigrateAsync();
   }
   ```

**Nello script PowerShell** (`Add-Migration.ps1`):

```powershell
# Cerca file .cs che contengono "IMigrationDbContext"
$files = Get-ChildItem -Path "Src" -Recurse -Filter "*.cs" |
    Select-String -Pattern ":\s*DbContext.*IMigrationDbContext|IMigrationDbContext.*DbContext"

# Estrae nome classe DbContext
if ($content -match "class\s+(\w+).*?IMigrationDbContext") {
    $contextName = $Matches[1]
}
```

---

### 1.4 Tabelle Migrations Separate

Per evitare conflitti tra migrations di DbContext diversi, ogni contesto ha la **propria migration history table**.

#### Migration History Table Naming

**Convenzione standard EF Core**: `__EFMigrationsHistory` (una sola tabella per tutta l'applicazione)

**MIT Framework**: `__EFMigrationsHistory_{ContextName}` (una tabella per ogni DbContext)

**Esempio**:
```sql
-- Database principale
CREATE TABLE __EFMigrationsHistory_JsonApiDbContext (
    MigrationId nvarchar(150) NOT NULL,
    ProductVersion nvarchar(32) NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory_JsonApiDbContext PRIMARY KEY (MigrationId)
);

-- Database custom
CREATE TABLE __EFMigrationsHistory_OtherDbContext (
    MigrationId nvarchar(150) NOT NULL,
    ProductVersion nvarchar(32) NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory_OtherDbContext PRIMARY KEY (MigrationId)
);
```

#### Configurazione nel DbContext

**Pattern richiesto in `OnConfiguring()`**:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseSqlServer(
            GetConfiguration().GetConnectionString(nameof(OtherDbContext)),
            options => options.MigrationsHistoryTable($"__EFMigrationsHistory_{nameof(OtherDbContext)}")
        );
    }
}
```

**Riferimento**: `Src/MIT.Fwk.Examples/Data/OtherDbContext.cs:41-47`

#### Struttura Cartelle Migrations

Ogni DbContext ha una **sottocartella dedicata** per le proprie migrations:

```
Src/MIT.Fwk.Infrastructure/
└── Migrations/
    └── (migrations di JsonApiDbContext - cartella root)
        ├── 20250101_InitialCreate.cs
        ├── 20250115_AddIdentityTables.cs
        └── JsonApiDbContextModelSnapshot.cs

Src/MIT.Fwk.Examples/
└── Migrations/
    └── (migrations di OtherDbContext - cartella root)
        ├── 20250122_InitialCreate.cs
        ├── 20250125_AddProducts.cs
        └── OtherDbContextModelSnapshot.cs
```

**Nota**: Le migrations sono nella cartella `Migrations/` **alla radice del progetto**, non in sottocartelle nominate per contesto. Questo è il comportamento standard di EF Core quando si usa `--output-dir Migrations`.

#### Vantaggi

✅ **Indipendenza**: Ogni DbContext evolve indipendentemente
✅ **Nessun conflitto**: Nomi migration possono essere uguali tra contesti diversi
✅ **Rollback selettivo**: Rollback di un contesto non influenza gli altri
✅ **Debug facilitato**: Chiaro quale migration appartiene a quale contesto
✅ **Deploy granulare**: Applicare migrations solo a specifici database

---

### 1.5 Pattern di Ordinamento

Le migrations di DbContext diversi possono avere **dipendenze** tra loro. Il framework richiede un ordinamento esplicito tramite configurazione.

#### Perché serve un ordine?

**Scenario comune**: `JsonApiDbContext` contiene tabelle ASP.NET Identity (Users, Roles) che altri DbContext possono referenziare tramite foreign key.

```sql
-- JsonApiDbContext (deve essere creato PRIMA)
CREATE TABLE AspNetUsers (
    Id nvarchar(450) PRIMARY KEY,
    UserName nvarchar(256),
    Email nvarchar(256)
);

-- OtherDbContext (dipende da AspNetUsers)
CREATE TABLE Products (
    Id int PRIMARY KEY IDENTITY,
    Name nvarchar(200),
    CreatedByUserId nvarchar(450),  -- ← Foreign key to AspNetUsers
    CONSTRAINT FK_Products_Users FOREIGN KEY (CreatedByUserId)
        REFERENCES AspNetUsers(Id)
);
```

Se applichi migrations di `OtherDbContext` **prima** di `JsonApiDbContext`:
```
❌ Error: Invalid object name 'AspNetUsers'
❌ Error: FOREIGN KEY constraint references non-existent table
```

#### Configurazione `DatabaseMigrationOrder`

**File**: `C:\MaeFWK\Runtime\Bin\customsettings.json` (o `Src/MIT.Fwk.WebApi/customsettings.json`)

```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",    // ← Sempre per primo (contiene Identity)
    "OtherDbContext",      // ← Secondo
    "MyCompanyDbContext"   // ← Terzo
  ]
}
```

**Regole**:
1. `JsonApiDbContext` **deve essere sempre per primo** (tabelle Identity)
2. Lista **tutti i DbContext** che implementano `IMigrationDbContext`
3. L'ordine determina la sequenza di applicazione migrations
4. Se un DbContext è presente nel codice ma **non** in `DatabaseMigrationOrder`, il framework genera **errore** al startup

#### Processo di Validazione

**Al startup** (`DatabaseMigrationService.OrderDbContextsByConfiguration()`):

```csharp
// Legge configurazione
List<string> migrationOrder = _configuration
    .GetSection("DatabaseMigrationOrder")
    .Get<List<string>>();

// 1. Verifica configurazione esistente
if (migrationOrder == null || !migrationOrder.Any())
{
    throw new Exception("DatabaseMigrationOrder parameter not found!");
}

// 2. Verifica tutti i DbContext discovered sono in configurazione
var discovered = dbContextTypes.Select(t => t.Name).ToList();
var configured = migrationOrder;
var missingInConfig = discovered.Except(configured).ToList();

if (missingInConfig.Any())
{
    throw new Exception(
        $"DbContexts discovered but NOT in configuration: {string.Join(", ", missingInConfig)}"
    );
}

// 3. Ordina i DbContext secondo configurazione
return dbContextTypes
    .OrderBy(t => migrationOrder.IndexOf(t.Name))
    .ToList();
```

**Riferimento**: `Src/MIT.Fwk.WebApi/Services/DatabaseMigrationService.cs:110-210`

#### Messaggio di Errore Completo

Se la configurazione è errata, il framework mostra un messaggio dettagliato:

```
=================================================
ERROR: Missing DatabaseMigrationOrder configuration!
=================================================
Please add 'DatabaseMigrationOrder' to customsettings.json
Example:
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext"
  ]
=================================================

Discovered DbContexts: JsonApiDbContext, OtherDbContext, MyCompanyDbContext
NOT in configuration: MyCompanyDbContext

Please add them to 'DatabaseMigrationOrder' in customsettings.json
```

**Riferimento**: `Src/MIT.Fwk.WebApi/Services/DatabaseMigrationService.cs:120-167`

---

## 2. Modalità Operative

### 2.1 Auto-Migrations (EnableAutoMigrations)

Le **auto-migrations** consentono al framework di applicare automaticamente tutte le migrations pendenti al **startup dell'applicazione**.

#### Quando Usarle

✅ **Ambiente di sviluppo**: Aggiornamenti frequenti allo schema, nessun rischio dati
✅ **Testing automatizzato**: I test creano database fresh con schema sempre aggiornato
✅ **Staging/Pre-produzione**: Deploy automatico senza intervento manuale
❓ **Produzione**: Dipende dal livello di rischio e dalla strategia di deploy

#### Configurazione

**File**: `customsettings.json`

```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext"
  ]
}
```

**Parametri obbligatori**:
- `EnableAutoMigrations`: `true` per abilitare, `false` o assente per disabilitare
- `DatabaseMigrationOrder`: **Obbligatorio** quando `EnableAutoMigrations: true`

#### Comportamento

**Al startup dell'applicazione** (`Program.cs`):

```csharp
// 1. Leggi configurazione
bool enableAutoMigrations = configuration.GetValue<bool>("EnableAutoMigrations", false);

// 2. Se abilitato, applica migrations PRIMA di avviare l'API
if (enableAutoMigrations)
{
    ApplyMigrationsSync(services, dbContextTypes, configuration);
}

// 3. Avvia l'applicazione
app.Run();
```

**Riferimento**: `Src/MIT.Fwk.WebApi/Program.cs:175-178`

**Sequenza di esecuzione**:

1. **Discovery**: Trova tutti i DbContext con `IMigrationDbContext`
2. **Ordinamento**: Ordina secondo `DatabaseMigrationOrder`
3. **Applicazione**: Per ogni DbContext, esegue `context.Database.MigrateAsync()`

```
[Auto-Migration] Discovering DbContexts...
[Auto-Migration] Found 2 DbContext(s):
  - JsonApiDbContext
  - OtherDbContext

[Auto-Migration] Applying migrations to JsonApiDbContext...
[Auto-Migration]   - 20250101_InitialCreate
[Auto-Migration]   - 20250115_AddIdentityTables
[Auto-Migration] ✓ JsonApiDbContext migrations applied successfully

[Auto-Migration] Applying migrations to OtherDbContext...
[Auto-Migration]   - 20250122_InitialCreate
[Auto-Migration] ✓ OtherDbContext migrations applied successfully

[Auto-Migration] All migrations applied successfully!
```

**Riferimento**: `Src/MIT.Fwk.WebApi/Services/DatabaseMigrationService.cs:46-103`

#### Fallback Behavior

Se `EnableAutoMigrations: false` (o parametro assente):
- Il framework non applica migrations al startup
- Lo sviluppatore deve applicarle manualmente (PowerShell o EF CLI)
- L'applicazione si avvia normalmente, ma potrebbero esserci errori runtime se lo schema è obsoleto

```
[Startup] Auto-migrations disabled (EnableAutoMigrations: false)
[Startup] Database schema will not be updated automatically
[Startup] Use Add-Migration.ps1 -Update to apply pending migrations
```

---

### 2.2 Script PowerShell (Add-Migration.ps1)

Lo script **`Add-Migration.ps1`** è il tool principale per gestire migrations manualmente durante lo sviluppo.

#### Vantaggi rispetto a EF CLI diretto

✅ **Auto-discovery**: Trova automaticamente tutti i DbContext con `IMigrationDbContext`
✅ **Batch operations**: Applica operazioni su tutti i contesti con un comando
✅ **Validazione**: Verifica che i progetti esistano e dotnet-ef sia installato
✅ **Feedback visivo**: Output colorato e riepilogo operazioni
✅ **Gestione errori**: Continua se un contesto fallisce, mostra summary finale

#### Operazioni Disponibili

| Operazione | Flag | Descrizione |
|------------|------|-------------|
| **Creare migration** | `-Name` | Genera nuova migration confrontando modello con snapshot |
| **Applicare migrations** | `-Update` | Applica tutte le migrations pendenti al database |
| **Rimuovere ultima migration** | `-Remove` | Rimuove l'ultima migration **non applicata** |
| **Info DbContext** | `-List` | Mostra info su DbContext scoperti e migrations esistenti |

**Riferimento**: `Add-Migration.ps1:1-361`

#### Targeting

**Tutti i DbContext** (default):
```powershell
.\Add-Migration.ps1 -Name "AddProducts"
```

**Singolo DbContext** (parametro `-Context`):
```powershell
.\Add-Migration.ps1 -Name "AddProducts" -Context "OtherDbContext"
```

---

### 2.3 EF Core CLI (Operazioni Avanzate)

Per operazioni non coperte dallo script PowerShell, puoi usare **direttamente EF Core CLI**.

#### Installazione

```bash
# Installa dotnet-ef global tool
dotnet tool install --global dotnet-ef

# Verifica installazione
dotnet ef --version
```

#### Operazioni Comuni

**Creare migration**:
```bash
cd Src/MIT.Fwk.Examples
dotnet ef migrations add AddProducts \
  --context OtherDbContext \
  --startup-project ../MIT.Fwk.WebApi \
  --output-dir Migrations
```

**Applicare migrations**:
```bash
dotnet ef database update \
  --context OtherDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

**Rollback a migration specifica**:
```bash
dotnet ef database update InitialCreate \
  --context OtherDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

**Generare script SQL** (per deploy manuale in produzione):
```bash
dotnet ef migrations script \
  --context OtherDbContext \
  --startup-project ../MIT.Fwk.WebApi \
  --output migration.sql
```

**Rimuovere ultima migration**:
```bash
dotnet ef migrations remove \
  --context OtherDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

**Nota**: Assicurati sempre di specificare `--startup-project` correttamente, altrimenti EF non troverà la configurazione (connection strings, appsettings.json).

---

## 3. Script PowerShell - Guida Dettagliata

### 3.1 Parametri e Sintassi

**Sintassi generale**:
```powershell
.\Add-Migration.ps1 [-Name <string>] [-Context <string>] [-Remove] [-Update] [-List]
```

#### Parametri

| Parametro | Tipo | Obbligatorio | Descrizione |
|-----------|------|--------------|-------------|
| `-Name` | `string` | ❌ | Nome della migration (es: "AddProducts"). **Obbligatorio** se non usi `-Remove`, `-Update` o `-List` |
| `-Context` | `string` | ❌ | Nome del DbContext target (es: "OtherDbContext"). Se omesso, opera su **tutti** i DbContext |
| `-Remove` | `switch` | ❌ | Rimuove l'ultima migration **non applicata** |
| `-Update` | `switch` | ❌ | Applica tutte le migrations pendenti al database |
| `-List` | `switch` | ❌ | Mostra informazioni sui DbContext e migrations esistenti |

**Riferimento**: `Add-Migration.ps1:24-39`

---

### 3.2 Operazioni Disponibili

#### Operazione: `-List` (Info DbContext)

**Scopo**: Mostrare quali DbContext sono scoperti dal framework e quali migrations esistono per ciascuno.

**Sintassi**:
```powershell
# Info su tutti i DbContext
.\Add-Migration.ps1 -List

# Info su DbContext specifico
.\Add-Migration.ps1 -List -Context "OtherDbContext"
```

**Output esempio**:
```
============================================================================
 OtherDbContext Information
============================================================================

Context: OtherDbContext
Project: Src\MIT.Fwk.Examples
Namespace: MIT.Fwk.Examples.Data
Migration Table: __EFMigrationsHistory_OtherDbContext

Existing migrations: 3

Latest migrations:
  - 20250122_InitialCreate
  - 20250125_AddProducts
  - 20250128_AddCategories

============================================================================
```

**Riferimento**: `Add-Migration.ps1:133-170` (funzione `Show-MigrationInfo`)

---

#### Operazione: Creare Migration

**Scopo**: Generare una nuova migration confrontando il modello corrente con lo snapshot.

**Sintassi**:
```powershell
# Crea migration per tutti i DbContext
.\Add-Migration.ps1 -Name "AddProducts"

# Crea migration solo per OtherDbContext
.\Add-Migration.ps1 -Name "AddProducts" -Context "OtherDbContext"
```

**Comportamento**:
1. EF Core confronta il modello attuale delle entità con `{Context}ModelSnapshot.cs`
2. Genera due metodi: `Up()` (applicare modifiche) e `Down()` (rollback)
3. Aggiorna il `ModelSnapshot` con il nuovo stato
4. Salva i file nella cartella `Migrations/`

**File generati**:
```
Src/MIT.Fwk.Examples/Migrations/
├── 20250128120000_AddProducts.cs          (migration)
├── 20250128120000_AddProducts.Designer.cs (metadata)
└── OtherDbContextModelSnapshot.cs         (aggiornato)
```

**Riferimento**: `Add-Migration.ps1:315-330`

---

#### Operazione: `-Update` (Applicare Migrations)

**Scopo**: Applicare tutte le migrations pendenti al database.

**Sintassi**:
```powershell
# Applica migrations per tutti i DbContext
.\Add-Migration.ps1 -Update

# Applica migrations solo per OtherDbContext
.\Add-Migration.ps1 -Update -Context "OtherDbContext"
```

**Comportamento**:
1. Legge la migration history table nel database (es: `__EFMigrationsHistory_OtherDbContext`)
2. Confronta con le migration files presenti in `Migrations/`
3. Esegue `Up()` di tutte le migrations non ancora applicate
4. Aggiorna la history table con i nuovi record

**Output esempio**:
```
========================================
 Processing: OtherDbContext
========================================
Applying pending migrations to OtherDbContext...
Build started...
Build succeeded.
Applying migration '20250128_AddProducts'.
Done.
✓ Database updated successfully for OtherDbContext
```

**Riferimento**: `Add-Migration.ps1:301-313`

---

#### Operazione: `-Update -Name` (Rollback/Forward a Migration Specifica)

**Scopo**: Portare il database a uno stato specifico (rollback o forward).

**Sintassi**:
```powershell
# Rollback a migration "InitialCreate"
.\Add-Migration.ps1 -Update -Name "InitialCreate" -Context "OtherDbContext"

# Forward a migration "AddCategories"
.\Add-Migration.ps1 -Update -Name "AddCategories" -Context "OtherDbContext"
```

**Comportamento**:
- **Rollback**: Se la migration target è **precedente** allo stato attuale, EF esegue `Down()` delle migrations successive
- **Forward**: Se la migration target è **successiva** allo stato attuale, EF esegue `Up()` delle migrations mancanti

**Esempio rollback**:
```
Stato attuale:     InitialCreate → AddProducts → AddCategories
Target:            InitialCreate
Azioni:            Esegue Down() di AddCategories, poi Down() di AddProducts
Stato finale:      InitialCreate
```

**Riferimento**: `Add-Migration.ps1:287-299`

---

#### Operazione: `-Remove` (Rimuovere Ultima Migration)

**Scopo**: Rimuovere l'**ultima migration non applicata**.

**Sintassi**:
```powershell
# Rimuovi ultima migration da tutti i DbContext
.\Add-Migration.ps1 -Remove

# Rimuovi ultima migration solo da OtherDbContext
.\Add-Migration.ps1 -Remove -Context "OtherDbContext"
```

**Comportamento**:
1. Verifica che l'ultima migration **non sia stata applicata** al database
2. Rimuove i file della migration
3. Ripristina il `ModelSnapshot` allo stato precedente

**Limitazioni**:
- ❌ **Non puoi rimuovere** una migration già applicata al database
- ❌ **Non puoi rimuovere** una migration intermedia (solo l'ultima)

**Se l'ultima migration è già applicata**:
```
Error: The migration '20250128_AddProducts' has been applied to the database.
To revert, use: .\Add-Migration.ps1 -Update -Name "PreviousMigration" -Context "OtherDbContext"
Then remove the migration: .\Add-Migration.ps1 -Remove -Context "OtherDbContext"
```

**Riferimento**: `Add-Migration.ps1:273-286`

---

### 3.3 Esempi Pratici

#### Esempio 1: Workflow completo per nuovo DbContext

```powershell
# 1. Verifica che il DbContext sia scoperto
.\Add-Migration.ps1 -List -Context "MyCompanyDbContext"

# Output:
# Context: MyCompanyDbContext
# Project: Src\MIT.Fwk.MyCompany
# Migrations folder not found: ...
# No migrations found yet

# 2. Crea prima migration
.\Add-Migration.ps1 -Name "InitialCreate" -Context "MyCompanyDbContext"

# Output:
# Creating migration 'InitialCreate' for MyCompanyDbContext...
# ✓ Migration created successfully for MyCompanyDbContext
#   Location: Src\MIT.Fwk.MyCompany\Migrations\

# 3. Applica migration al database
.\Add-Migration.ps1 -Update -Context "MyCompanyDbContext"

# Output:
# Applying pending migrations to MyCompanyDbContext...
# ✓ Database updated successfully for MyCompanyDbContext
```

---

#### Esempio 2: Modificare entità e creare migration incrementale

```powershell
# 1. Modifica l'entità Product aggiungendo campo "Description"
# (modifica manualmente Product.cs)

# 2. Crea migration per tracciare il cambiamento
.\Add-Migration.ps1 -Name "AddProductDescription" -Context "OtherDbContext"

# Output:
# Creating migration 'AddProductDescription' for OtherDbContext...
# ✓ Migration created successfully for OtherDbContext

# 3. Verifica il file generato
# (apri Src\MIT.Fwk.Examples\Migrations\*_AddProductDescription.cs)

# 4. Applica la migration
.\Add-Migration.ps1 -Update -Context "OtherDbContext"
```

---

#### Esempio 3: Correggere una migration errata (non ancora applicata)

```powershell
# 1. Hai creato una migration con errori
.\Add-Migration.ps1 -Name "AddOrders" -Context "OtherDbContext"

# 2. Ti accorgi dell'errore PRIMA di applicarla al database
# Rimuovi la migration errata
.\Add-Migration.ps1 -Remove -Context "OtherDbContext"

# Output:
# Removing last migration from OtherDbContext...
# ✓ Migration removed successfully from OtherDbContext

# 3. Correggi il codice delle entità

# 4. Ricrea la migration corretta
.\Add-Migration.ps1 -Name "AddOrders" -Context "OtherDbContext"
```

---

#### Esempio 4: Applicare migrations a tutti i DbContext

```powershell
# Applica tutte le migrations pendenti a tutti i contesti
.\Add-Migration.ps1 -Update

# Output:
# ========================================
#  Processing: JsonApiDbContext
# ========================================
# Applying pending migrations to JsonApiDbContext...
# ✓ Database updated successfully for JsonApiDbContext
#
# ========================================
#  Processing: OtherDbContext
# ========================================
# Applying pending migrations to OtherDbContext...
# ✓ Database updated successfully for OtherDbContext
#
# ============================================================================
#  ALL OPERATIONS COMPLETED SUCCESSFULLY
# ============================================================================
```

---

#### Esempio 5: Info rapida su tutti i DbContext

```powershell
# Mostra info su tutti i DbContext scoperti
.\Add-Migration.ps1 -List

# Output:
# Discovering DbContexts that implement IJsonApiDbContext...
#   Found: JsonApiDbContext in Src\MIT.Fwk.Infrastructure
#   Found: OtherDbContext in Src\MIT.Fwk.Examples
#
# ============================================================================
#  JsonApiDbContext Information
# ============================================================================
# Context: JsonApiDbContext
# Project: Src\MIT.Fwk.Infrastructure
# Existing migrations: 5
# Latest migrations:
#   - 20250101_InitialCreate
#   - 20250115_AddIdentityTables
#   ...
#
# ============================================================================
#  OtherDbContext Information
# ============================================================================
# Context: OtherDbContext
# Project: Src\MIT.Fwk.Examples
# Existing migrations: 3
# Latest migrations:
#   - 20250122_InitialCreate
#   - 20250125_AddProducts
#   - 20250128_AddCategories
```

---

## 4. Configurazione

### 4.1 customsettings.json

**Percorso**: `C:\MaeFWK\Runtime\Bin\customsettings.json` (runtime) o `Src\MIT.Fwk.WebApi\customsettings.json` (sviluppo)

#### Parametri Migrations

```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext",
    "MyCompanyDbContext"
  ]
}
```

#### Parametro: `EnableAutoMigrations`

**Tipo**: `boolean`
**Default**: `false`
**Descrizione**: Se `true`, il framework applica automaticamente tutte le migrations pendenti al startup dell'applicazione.

**Valori accettati**:
- `true` → Abilita auto-migrations
- `false` → Disabilita (default)
- Assente → Equivalente a `false`

**Quando usare `true`**:
- Ambiente di sviluppo locale
- Pipeline CI/CD per testing/staging
- Ambienti dove non è richiesto controllo manuale

**Quando usare `false`**:
- Produzione (se preferisci apply manuale controllato)
- Ambienti multi-istanza (per evitare race conditions)

---

#### Parametro: `DatabaseMigrationOrder`

**Tipo**: `string[]` (array di stringi)
**Obbligatorio**: **Sì**, quando `EnableAutoMigrations: true`
**Descrizione**: Definisce l'ordine di applicazione delle migrations tra DbContext diversi.

**Regole**:
1. ✅ `JsonApiDbContext` deve essere **sempre per primo**
2. ✅ Deve contenere **tutti** i DbContext che implementano `IMigrationDbContext`
3. ✅ L'ordine riflette le dipendenze (es: FK tra tabelle di contesti diversi)
4. ❌ Se un DbContext è scoperto ma **non** in lista → **Errore fatale** al startup

**Esempio completo**:
```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",      // Framework DB (Identity, core entities)
    "OtherDbContext",        // Custom module #1
    "MyCompanyDbContext",    // Custom module #2
    "AnalyticsDbContext"     // Custom module #3
  ],

  "OtherDbContext": "Sql",
  "MyCompanyDbContext": "MySql",
  "AnalyticsDbContext": "Sql"
}
```

---

### 4.2 dbconnections.json

**Percorso**: `C:\MaeFWK\Runtime\Bin\dbconnections.json` (runtime) o `Src\MIT.Fwk.WebApi\dbconnections.json` (sviluppo)

#### Struttura

```json
{
  "ConnectionStrings": {
    "JsonApiConnection": "Server=localhost;Database=MainDB;User Id=sa;Password=***;TrustServerCertificate=True",
    "OtherDbContext": "Server=localhost;Database=CustomDB;User Id=sa;Password=***;TrustServerCertificate=True",
    "MyCompanyDbContext": "Server=mysql-server;Database=CompanyDB;User=root;Password=***",
    "NoSQLConnection": "mongodb://localhost:27017/FWK"
  }
}
```

#### Convenzioni Naming

**Regola critica**: La **key della connection string** deve matchare **esattamente il nome della classe DbContext**.

✅ **Corretto**:
```json
"ConnectionStrings": {
  "OtherDbContext": "Server=...;Database=CustomDB;..."
}
```

```csharp
public class OtherDbContext : DbContext, IMigrationDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            GetConfiguration().GetConnectionString(nameof(OtherDbContext))  // ← "OtherDbContext"
        );
    }
}
```

❌ **Errato**:
```json
"ConnectionStrings": {
  "CustomDb": "Server=...;Database=CustomDB;..."  // ← Nome diverso!
}
```
```csharp
public class OtherDbContext : DbContext  // ← Mismatch!
{
    // GetConnectionString("OtherDbContext") → Returns NULL
}
```

**Riferimento**: `Src/MIT.Fwk.Examples/Data/OtherDbContext.cs:40`

---

#### Provider Database

Il provider da usare (SQL Server vs MySQL) è configurato in `customsettings.json`:

```json
{
  "OtherDbContext": "Sql",          // SQL Server
  "MyCompanyDbContext": "MySql"     // MySQL
}
```

**Riferimento**: `CLAUDE.md:172-173`

---

### 4.3 Validazione Configurazione

#### Errore: `DatabaseMigrationOrder` mancante

**Quando si verifica**: `EnableAutoMigrations: true` ma `DatabaseMigrationOrder` è assente o vuoto.

**Messaggio**:
```
=================================================
ERROR: Missing DatabaseMigrationOrder configuration!
=================================================
Please add 'DatabaseMigrationOrder' to customsettings.json
Example:
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext"
  ]
=================================================
```

**Soluzione**: Aggiungi il parametro `DatabaseMigrationOrder` con tutti i DbContext.

**Riferimento**: `Src/MIT.Fwk.WebApi/Services/DatabaseMigrationService.cs:118-139`

---

#### Errore: DbContext scoperto non in configurazione

**Quando si verifica**: Un DbContext che implementa `IMigrationDbContext` è presente nel codice, ma **non** è listato in `DatabaseMigrationOrder`.

**Messaggio**:
```
=================================================
ERROR: DbContext(s) missing from configuration!
=================================================
Discovered DbContexts NOT in configuration: MyCompanyDbContext
Please add them to 'DatabaseMigrationOrder' in customsettings.json
=================================================
```

**Scenario**:
- Hai aggiunto un nuovo modulo con `MyCompanyDbContext`
- Il DbContext implementa `IMigrationDbContext`
- Hai dimenticato di aggiornare `customsettings.json`

**Soluzione**:
```json
{
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext",
    "MyCompanyDbContext"  // ← Aggiungi qui
  ]
}
```

**Riferimento**: `Src/MIT.Fwk.WebApi/Services/DatabaseMigrationService.cs:156-167`

---

#### Errore: Connection string mancante

**Quando si verifica**: La key della connection string in `dbconnections.json` non matcha il nome del DbContext.

**Messaggio**:
```
System.ArgumentNullException: Connection string 'OtherDbContext' not found
```

**Soluzione**: Verifica che la key in `dbconnections.json` sia esattamente uguale al nome della classe DbContext.

---

## 5. Workflow Sviluppatore

### 5.1 Creare Nuovo DbContext Custom

#### Step 1: Creare progetto custom

```powershell
# Crea nuovo progetto di libreria .NET
cd Src
dotnet new classlib -n MIT.Fwk.MyCompany -f net8.0
```

#### Step 2: Aggiungere riferimento al progetto WebApi

**File**: `Src/MIT.Fwk.WebApi/MIT.Fwk.WebApi.csproj`

```xml
<ItemGroup>
  <ProjectReference Include="..\MIT.Fwk.MyCompany\MIT.Fwk.MyCompany.csproj" />
</ItemGroup>
```

#### Step 3: Creare DbContext

**File**: `Src/MIT.Fwk.MyCompany/Data/MyCompanyDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Data;

namespace MIT.Fwk.MyCompany.Data
{
    public class MyCompanyDbContext : DbContext, IMigrationDbContext
    {
        public MyCompanyDbContext(DbContextOptions<MyCompanyDbContext> options)
            : base(options) { }

        // Parameterless constructor per design-time tools (EF CLI)
        public MyCompanyDbContext()
            : base(GetOptions()) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    GetConfiguration().GetConnectionString(nameof(MyCompanyDbContext)),
                    options => options.MigrationsHistoryTable(
                        $"__EFMigrationsHistory_{nameof(MyCompanyDbContext)}"
                    )
                );
            }
        }

        // Entities
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();

        private static DbContextOptions<MyCompanyDbContext> GetOptions()
        {
            return new DbContextOptionsBuilder<MyCompanyDbContext>()
                .UseSqlServer(GetConfiguration().GetConnectionString(nameof(MyCompanyDbContext)))
                .Options;
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("dbconnections.json")
                .Build();
        }
    }
}
```

**Pattern chiave**:
- ✅ Implementa `IMigrationDbContext` (per auto-migrations)
- ✅ Usa `nameof(MyCompanyDbContext)` per connection string e history table
- ✅ Configura `MigrationsHistoryTable` con naming convention `__EFMigrationsHistory_{ContextName}`

**Riferimento**: `Src/MIT.Fwk.Examples/Data/OtherDbContext.cs`

#### Step 4: Creare entità

**File**: `Src/MIT.Fwk.MyCompany/Entities/Customer.cs`

```csharp
using JsonApiDotNetCore.Resources.Annotations;

namespace MIT.Fwk.MyCompany.Entities
{
    [Resource]
    [Table("customers")]
    public class Customer : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Email { get; set; }

        [HasMany]
        public virtual ICollection<Order> Orders { get; set; }
    }
}
```

#### Step 5: Configurare connection string

**File**: `dbconnections.json`

```json
{
  "ConnectionStrings": {
    "MyCompanyDbContext": "Server=localhost;Database=MyCompanyDB;User Id=sa;Password=***;TrustServerCertificate=True"
  }
}
```

#### Step 6: Configurare auto-migrations

**File**: `customsettings.json`

```json
{
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext",
    "MyCompanyDbContext"  // ← Aggiungi il nuovo contesto
  ],
  "MyCompanyDbContext": "Sql"  // Provider
}
```

---

### 5.2 Generare Prima Migration

```powershell
# Verifica che il DbContext sia scoperto
.\Add-Migration.ps1 -List -Context "MyCompanyDbContext"

# Output:
# Found: MyCompanyDbContext in Src\MIT.Fwk.MyCompany
# Migrations folder not found
# No migrations found yet

# Genera la prima migration
.\Add-Migration.ps1 -Name "InitialCreate" -Context "MyCompanyDbContext"

# Output:
# Creating migration 'InitialCreate' for MyCompanyDbContext...
# Build started...
# Build succeeded.
# ✓ Migration created successfully for MyCompanyDbContext
#   Location: Src\MIT.Fwk.MyCompany\Migrations\
```

**File generati**:
```
Src/MIT.Fwk.MyCompany/Migrations/
├── 20250128120000_InitialCreate.cs
├── 20250128120000_InitialCreate.Designer.cs
└── MyCompanyDbContextModelSnapshot.cs
```

**Contenuto migration** (`*_InitialCreate.cs`):
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: false)
            },
            constraints: table => {
                table.PrimaryKey("PK_customers", x => x.Id);
            }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customers");
    }
}
```

---

### 5.3 Modificare Entità e Creare Migration Incrementale

#### Step 1: Modificare l'entità

**File**: `Src/MIT.Fwk.MyCompany/Entities/Customer.cs`

```csharp
[Resource]
[Table("customers")]
public class Customer : Identifiable<int>
{
    [Attr]
    public string Name { get; set; }

    [Attr]
    public string Email { get; set; }

    [Attr]
    public string PhoneNumber { get; set; }  // ← Campo aggiunto

    [Attr]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ← Campo aggiunto

    [HasMany]
    public virtual ICollection<Order> Orders { get; set; }
}
```

#### Step 2: Generare migration incrementale

```powershell
.\Add-Migration.ps1 -Name "AddCustomerContactInfo" -Context "MyCompanyDbContext"
```

**Migration generata**:
```csharp
public partial class AddCustomerContactInfo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PhoneNumber",
            table: "customers",
            nullable: true
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "customers",
            nullable: false,
            defaultValue: new DateTime(2025, 1, 28, 12, 0, 0)
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PhoneNumber", table: "customers");
        migrationBuilder.DropColumn(name: "CreatedAt", table: "customers");
    }
}
```

**Nota**: EF Core ha rilevato automaticamente i cambiamenti confrontando il modello corrente con `MyCompanyDbContextModelSnapshot.cs`.

---

### 5.4 Applicare Migrations

#### Opzione A: Auto-Migrations (al startup)

Se `EnableAutoMigrations: true`, le migrations vengono applicate automaticamente al prossimo avvio dell'API:

```powershell
cd C:\MaeFWK\Runtime\Bin
.\startupWebApi.bat
```

**Output console**:
```
[Auto-Migration] Starting automatic database migrations...
[Auto-Migration] Found 3 DbContext(s)
[Auto-Migration] Applying migrations to MyCompanyDbContext...
[Auto-Migration]   - 20250128_InitialCreate
[Auto-Migration]   - 20250128_AddCustomerContactInfo
[Auto-Migration] ✓ MyCompanyDbContext migrations applied successfully
```

#### Opzione B: Manuale via PowerShell

```powershell
.\Add-Migration.ps1 -Update -Context "MyCompanyDbContext"

# Output:
# Applying pending migrations to MyCompanyDbContext...
# Applying migration '20250128_InitialCreate'.
# Applying migration '20250128_AddCustomerContactInfo'.
# ✓ Database updated successfully for MyCompanyDbContext
```

#### Opzione C: Manuale via EF CLI

```bash
cd Src/MIT.Fwk.MyCompany
dotnet ef database update \
  --context MyCompanyDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

#### Verifica nel database

**Tabella `__EFMigrationsHistory_MyCompanyDbContext`**:
```sql
SELECT * FROM __EFMigrationsHistory_MyCompanyDbContext;

-- Output:
-- MigrationId                           | ProductVersion
-- 20250128120000_InitialCreate          | 8.0.0
-- 20250128130000_AddCustomerContactInfo | 8.0.0
```

**Tabella `customers`**:
```sql
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'customers';

-- Output:
-- COLUMN_NAME   | DATA_TYPE
-- Id            | int
-- Name          | nvarchar
-- Email         | nvarchar
-- PhoneNumber   | nvarchar  (← campo aggiunto)
-- CreatedAt     | datetime2 (← campo aggiunto)
```

---

### 5.5 Rimuovere Migration Errata

#### Scenario: Migration non ancora applicata

```powershell
# 1. Hai creato una migration con errori
.\Add-Migration.ps1 -Name "AddOrders" -Context "MyCompanyDbContext"

# 2. Ti accorgi dell'errore PRIMA di fare -Update

# 3. Rimuovi la migration
.\Add-Migration.ps1 -Remove -Context "MyCompanyDbContext"

# Output:
# Removing last migration from MyCompanyDbContext...
# ✓ Migration removed successfully from MyCompanyDbContext

# 4. Correggi il codice e ricrea
.\Add-Migration.ps1 -Name "AddOrders" -Context "MyCompanyDbContext"
```

#### Scenario: Migration già applicata (richiede rollback)

```powershell
# 1. Migration già applicata al database
.\Add-Migration.ps1 -Update -Context "MyCompanyDbContext"

# 2. Ti accorgi dell'errore DOPO l'apply

# 3. Rollback al database (reverte al penultima migration)
.\Add-Migration.ps1 -Update -Name "AddCustomerContactInfo" -Context "MyCompanyDbContext"

# Output:
# Reverting migration '20250128_AddOrders'...
# ✓ Database updated to migration 'AddCustomerContactInfo'

# 4. Ora puoi rimuovere la migration dal codice
.\Add-Migration.ps1 -Remove -Context "MyCompanyDbContext"

# 5. Correggi e ricrea
.\Add-Migration.ps1 -Name "AddOrders" -Context "MyCompanyDbContext"
```

---

## 6. Scenari Avanzati

### 6.1 Rollback a Migration Specifica

#### Uso: Tornare a uno stato precedente del database

**Sintassi**:
```powershell
.\Add-Migration.ps1 -Update -Name "<MigrationName>" -Context "<ContextName>"
```

**Esempio**:
```powershell
# Stato attuale del database:
# - InitialCreate
# - AddCustomerContactInfo
# - AddOrders
# - AddOrderItems

# Vuoi tornare a "AddCustomerContactInfo"
.\Add-Migration.ps1 -Update -Name "AddCustomerContactInfo" -Context "MyCompanyDbContext"
```

**Comportamento**:
1. EF legge la migration history table
2. Identifica quali migrations sono successive a "AddCustomerContactInfo"
3. Esegue `Down()` di ogni migration in ordine inverso:
   - `AddOrderItems.Down()`
   - `AddOrders.Down()`
4. Aggiorna la history table rimuovendo i record

**Output**:
```
Reverting migration '20250130_AddOrderItems'...
Reverting migration '20250129_AddOrders'...
✓ Database updated to migration 'AddCustomerContactInfo'
```

**Verifica**:
```sql
SELECT * FROM __EFMigrationsHistory_MyCompanyDbContext;

-- Output:
-- MigrationId                           | ProductVersion
-- 20250128120000_InitialCreate          | 8.0.0
-- 20250128130000_AddCustomerContactInfo | 8.0.0
-- (le successive sono state rimosse)
```

**Nota**: Le migration files nel codice rimangono. Solo lo stato del database cambia.

---

#### Rollback completo (rimuovere tutto)

**Sintassi speciale**:
```powershell
# Via EF CLI (Add-Migration.ps1 non supporta migration "0")
cd Src/MIT.Fwk.MyCompany
dotnet ef database update 0 \
  --context MyCompanyDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

**Risultato**: Esegue `Down()` di tutte le migrations, riportando il database a stato vuoto (nessuna tabella creata da questo DbContext).

---

### 6.2 DbContext Esterni (Senza Migrations)

#### Scenario: Connessione a database legacy/esterno

Vuoi connetterti a un database **esistente** (es: Northwind, database di terze parti) per fare query read-only o integrazioni.

**Pattern**: Crea un DbContext che **NON** implementa `IMigrationDbContext`.

#### Esempio: NorthwindDbContext

**File**: `Src/MIT.Fwk.Integrations/Data/NorthwindDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;

namespace MIT.Fwk.Integrations.Data
{
    // ❌ NO IMigrationDbContext
    public class NorthwindDbContext : DbContext
    {
        public NorthwindDbContext(DbContextOptions<NorthwindDbContext> options)
            : base(options) { }

        // Mappa tabelle esistenti
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura mapping su schema esistente
            modelBuilder.Entity<Employee>().ToTable("Employees", schema: "dbo");
            modelBuilder.Entity<Order>().ToTable("Orders", schema: "dbo");
        }
    }
}
```

**Entità con mapping esplicito**:
```csharp
[Table("Employees")]
public class Employee
{
    [Key]
    [Column("EmployeeID")]
    public int Id { get; set; }

    [Column("FirstName")]
    public string FirstName { get; set; }

    [Column("LastName")]
    public string LastName { get; set; }
}
```

**Connection string** (`dbconnections.json`):
```json
{
  "ConnectionStrings": {
    "NorthwindDbContext": "Server=external-server;Database=Northwind;User Id=readonly;Password=***"
  }
}
```

**Comportamento**:
- ✅ Framework ignora `NorthwindDbContext` per auto-migrations
- ✅ Script PowerShell **non** lo scopre (no `IMigrationDbContext`)
- ✅ Nessuna migration history table creata
- ✅ Usabile per query normalmente: `_context.Employees.ToListAsync()`

**Nota**: Se tenti di creare una migration per questo contesto, otterrai errore:

```powershell
.\Add-Migration.ps1 -Name "Test" -Context "NorthwindDbContext"

# Output:
# ERROR: DbContext 'NorthwindDbContext' not found!
# Available contexts:
#   - JsonApiDbContext
#   - OtherDbContext
#   - MyCompanyDbContext
```

---

### 6.3 Gestione Migration History Tables

#### Query sullo stato delle migrations

**SQL**:
```sql
-- Verifica quali migrations sono applicate
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory_MyCompanyDbContext
ORDER BY MigrationId;

-- Count migrations applicate
SELECT COUNT(*) AS AppliedMigrations
FROM __EFMigrationsHistory_MyCompanyDbContext;
```

**PowerShell** (via EF CLI):
```bash
cd Src/MIT.Fwk.MyCompany
dotnet ef migrations list \
  --context MyCompanyDbContext \
  --startup-project ../MIT.Fwk.WebApi

# Output:
# 20250128120000_InitialCreate (Applied)
# 20250128130000_AddCustomerContactInfo (Applied)
# 20250129120000_AddOrders (Pending)
```

---

#### Migrare migration history da progetto diverso

**Scenario**: Hai spostato un DbContext da un progetto a un altro. Le migrations esistenti sono nel database, ma i file migration sono in percorso diverso.

**Soluzione**: Crea uno "stub" migration vuota per allineare lo snapshot.

```powershell
# 1. Genera migration vuota nello snapshot corrente
.\Add-Migration.ps1 -Name "MigrateFromOldProject" -Context "MyCompanyDbContext"

# 2. Modifica la migration per farla vuota
# (apri il file e lascia Up() e Down() vuoti)

public partial class MigrateFromOldProject : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Vuoto - nessuna modifica schema
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Vuoto - nessuna modifica schema
    }
}

# 3. Applica la migration vuota (segna solo lo snapshot)
.\Add-Migration.ps1 -Update -Context "MyCompanyDbContext"
```

**Risultato**: Lo snapshot è allineato con lo schema esistente, future migrations funzioneranno correttamente.

---

### 6.4 Data Seeding in Migrations

#### Seeding statico (dati di setup)

**Scenario**: Vuoi popolare tabelle di lookup o dati iniziali durante una migration.

**Pattern**: Usa `migrationBuilder.InsertData()` in una migration.

**Esempio**:
```csharp
public partial class SeedRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "AspNetRoles",
            columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
            values: new object[,]
            {
                { "1", "Admin", "ADMIN", Guid.NewGuid().ToString() },
                { "2", "User", "USER", Guid.NewGuid().ToString() },
                { "3", "Guest", "GUEST", Guid.NewGuid().ToString() }
            }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValues: new object[] { "1", "2", "3" }
        );
    }
}
```

**Generare migration**:
```powershell
.\Add-Migration.ps1 -Name "SeedRoles" -Context "JsonApiDbContext"
```

---

#### Seeding dinamico (OnModelCreating)

**Scenario**: Vuoi che EF generi automaticamente INSERT nelle migrations per dati di seed.

**Pattern**: Usa `modelBuilder.Entity<T>().HasData()` in `OnModelCreating()`.

**Esempio**:
```csharp
public class MyCompanyDbContext : DbContext, IMigrationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Furniture" },
            new Category { Id = 3, Name = "Clothing" }
        );
    }
}
```

**Generare migration**:
```powershell
.\Add-Migration.ps1 -Name "SeedCategories" -Context "MyCompanyDbContext"
```

**Migration generata automaticamente**:
```csharp
public partial class SeedCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 1, "Electronics" },
                { 2, "Furniture" },
                { 3, "Clothing" }
            }
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Categories",
            keyColumn: "Id",
            keyValues: new object[] { 1, 2, 3 }
        );
    }
}
```

**Nota**: `HasData()` richiede che gli ID siano specificati esplicitamente (non auto-generati).

---

#### Seeding con SQL raw

**Scenario**: Hai query SQL complesse o vuoi massima flessibilità.

**Pattern**: Usa `migrationBuilder.Sql()`.

**Esempio**:
```csharp
public partial class SeedProducts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO Products (Name, CategoryId, Price, Stock)
            VALUES
                ('Laptop', 1, 999.99, 50),
                ('Mouse', 1, 29.99, 200),
                ('Desk', 2, 349.99, 30)
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM Products WHERE Name IN ('Laptop', 'Mouse', 'Desk')
        ");
    }
}
```

---

## 7. Riferimenti Rapidi

### 7.1 Comandi PowerShell Quick Reference

| Operazione | Comando |
|------------|---------|
| **Info su tutti i DbContext** | `.\Add-Migration.ps1 -List` |
| **Info su DbContext specifico** | `.\Add-Migration.ps1 -List -Context "OtherDbContext"` |
| **Creare migration (tutti)** | `.\Add-Migration.ps1 -Name "AddProducts"` |
| **Creare migration (singolo)** | `.\Add-Migration.ps1 -Name "AddProducts" -Context "OtherDbContext"` |
| **Applicare migrations (tutti)** | `.\Add-Migration.ps1 -Update` |
| **Applicare migrations (singolo)** | `.\Add-Migration.ps1 -Update -Context "OtherDbContext"` |
| **Rollback a migration** | `.\Add-Migration.ps1 -Update -Name "InitialCreate" -Context "OtherDbContext"` |
| **Rimuovere ultima migration** | `.\Add-Migration.ps1 -Remove -Context "OtherDbContext"` |

---

### 7.2 Pattern Interfacce

| Interfaccia | Scopo | Quando Implementare |
|-------------|-------|---------------------|
| **`IMigrationDbContext`** | Marca DbContext che devono avere migrations gestite dal framework | ✅ Database gestiti dall'applicazione<br>✅ Schema modificabile<br>✅ Vuoi auto-migrations |
| **`IJsonApiDbContext`** | Marca DbContext per JsonAPI auto-discovery (controller CRUD auto-generati) | ✅ Entità con attributo `[Resource]`<br>✅ Vuoi endpoint REST auto-generati |

**Pattern comuni**:

**Scenario A - DbContext gestito con JsonAPI**:
```csharp
public class OtherDbContext : DbContext, IMigrationDbContext, IJsonApiDbContext
```
→ Migrations automatiche + endpoint JsonAPI auto-generati

**Scenario B - DbContext gestito senza JsonAPI**:
```csharp
public class AnalyticsDbContext : DbContext, IMigrationDbContext
```
→ Migrations automatiche + controller custom manuali

**Scenario C - DbContext esterno per integrazione**:
```csharp
public class NorthwindDbContext : DbContext
```
→ Nessuna migration + controller custom read-only

---

### 7.3 File e Cartelle

#### Cartella Migrations

```
Src/<ProjectName>/Migrations/
├── 20250128120000_InitialCreate.cs
├── 20250128120000_InitialCreate.Designer.cs
├── 20250128130000_AddProducts.cs
├── 20250128130000_AddProducts.Designer.cs
└── <ContextName>ModelSnapshot.cs
```

**File migration** (`*_MigrationName.cs`):
- Contiene metodi `Up()` e `Down()`
- Generato automaticamente da EF Core
- Può essere modificato manualmente (con cautela)

**File designer** (`*_MigrationName.Designer.cs`):
- Metadata per EF Core
- **NON modificare manualmente**

**ModelSnapshot** (`*ModelSnapshot.cs`):
- Rappresenta lo stato corrente del modello
- Usato da EF per rilevare cambiamenti
- Aggiornato automaticamente ad ogni migration
- **NON modificare manualmente**

---

#### Migration History Table (Database)

**Tabella**: `__EFMigrationsHistory_{ContextName}`

**Schema**:
```sql
CREATE TABLE __EFMigrationsHistory_OtherDbContext (
    MigrationId nvarchar(150) NOT NULL PRIMARY KEY,
    ProductVersion nvarchar(32) NOT NULL
);
```

**Esempio dati**:
```sql
SELECT * FROM __EFMigrationsHistory_OtherDbContext;

-- MigrationId                           | ProductVersion
-- 20250128120000_InitialCreate          | 8.0.0
-- 20250128130000_AddProducts            | 8.0.0
```

---

#### File di Configurazione

| File | Percorso Runtime | Percorso Sviluppo | Scopo |
|------|------------------|-------------------|-------|
| **customsettings.json** | `C:\MaeFWK\Runtime\Bin\` | `Src\MIT.Fwk.WebApi\` | Configurazione framework (EnableAutoMigrations, DatabaseMigrationOrder) |
| **dbconnections.json** | `C:\MaeFWK\Runtime\Bin\` | `Src\MIT.Fwk.WebApi\` | Connection strings per ogni DbContext |
| **appsettings.json** | `C:\MaeFWK\Runtime\Bin\` | `Src\MIT.Fwk.WebApi\` | Configurazione ASP.NET Core standard |

---

#### Script e Tool

| File/Tool | Percorso | Scopo |
|-----------|----------|-------|
| **Add-Migration.ps1** | Root repository | Script PowerShell per gestione migrations |
| **dotnet-ef** | Global tool | EF Core CLI per operazioni avanzate |
| **DatabaseMigrationService.cs** | `Src\MIT.Fwk.WebApi\Services\` | Service che applica auto-migrations al startup |

---

## Conclusione

Questa guida ha coperto tutti gli aspetti delle migrations in MIT Framework:

✅ **Concetti**: Entity Framework migrations, multi-DbContext, auto-discovery, migration history
✅ **Modalità**: Auto-migrations, script PowerShell, EF CLI
✅ **Configurazione**: customsettings.json, dbconnections.json, validazione
✅ **Workflow**: Creare DbContext, generare migrations, applicarle, rollback
✅ **Scenari avanzati**: DbContext esterni, seeding, gestione history tables

**Prossimi passi suggeriti**:
1. Prova a creare un DbContext custom seguendo la sezione 5.1
2. Genera alcune migrations e applicale con auto-migrations
3. Sperimenta con rollback e rimozione migrations
4. Esplora data seeding per popolare tabelle di lookup

**Riferimenti esterni**:
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [JsonApiDotNetCore Documentation](https://www.jsonapi.net/)
- CLAUDE.md (documentazione framework MIT.FWK)
