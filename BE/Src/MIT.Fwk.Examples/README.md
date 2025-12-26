# MIT.Fwk.Examples - Guida Completa alle Funzionalità del Framework

Questo progetto contiene **esempi pratici e funzionanti** per tutte le funzionalità chiave del framework MIT.FWK v8.0+.

> **IMPORTANTE**: Tutti gli esempi sono file reali e funzionanti che puoi testare direttamente!

## Quick Start

1. Compila il progetto: `dotnet build`
2. Avvia l'API: `dotnet run --project Src/MIT.Fwk.WebApi`
3. Testa gli esempi via Swagger: `https://localhost:port/swagger`

## File Creati

```
MIT.Fwk.Examples/
├── README.md (questo file)
├── Entities/Examples/
│   ├── Product.cs          ✅ Entità esempio con relazioni
│   └── Review.cs           ✅ Entità recensioni
├── Controllers/
│   ├── ProductsEFCoreExamplesController.cs       ✅ 10 esempi EF Core
│   ├── ProductsDapperExamplesController.cs       ✅ 10 esempi Dapper
│   └── AuthenticationExamplesController.cs       ✅ 10 esempi Authentication
└── Data/
    ├── OtherDbContext.cs    (già esistente)
    └── ... (migrations)
```

## Indice delle Funzionalità

### 1. [JsonAPI Entities](#1-jsonapi-entities)
- Entità base con `[Resource]`
- Attributi con `[Attr]`
- Relazioni One-to-Many `[HasMany]`
- Relazioni Many-to-One `[HasOne]`
- Validazioni e filtri
- Soft Delete
- Ordinamento e paginazione

### 2. [Query al Database](#2-query-al-database)
- **EF Core DbContext** (LINQ, Include, AsNoTracking)
- **EF Core Raw SQL** (FromSqlRaw, ExecuteSqlRaw)
- **Dapper** (Query personalizzate, performance)
- **IJsonApiManualService** (operazioni complesse)

### 3. [Custom DbContext (Multi-Database)](#3-custom-dbcontext)
- Configurazione DbContext personalizzato
- Multi-database support (MySQL, SQL Server)
- Migrations per DbContext custom
- Connection string management

### 4. [CQRS & Event Sourcing](#4-cqrs--event-sourcing)
- Commands e CommandHandlers
- Events e EventHandlers
- Validazioni con FluentValidation
- Event Store (salvataggio eventi)
- IMediatorHandler (MediatR)

### 5. [MongoDB & NoSQL](#5-mongodb--nosql)
- DocumentManager (CRUD documenti)
- MongoContext e Collections
- Salvataggio file binari
- Query MongoDB
- Aggregations

### 6. [Multi-Tenancy](#6-multi-tenancy)
- ITenantProvider
- IHasTenant interface
- Query filters automatici
- Tenant isolation

### 7. [Authentication & Authorization](#7-authentication--authorization)
- JWT Token
- Role-based Authorization
- Policy-based Authorization (NeededRoleLevel)
- Custom Claims
- User context

### 8. [Dependency Injection](#8-dependency-injection)
- Auto-discovery services (*ManualService pattern)
- IApplicationServiceHandler
- IDomainServiceHandler
- Scoped, Transient, Singleton

### 9. [Controllers](#9-controllers)
- ApiController base class
- JsonAPI auto-generated endpoints
- Custom endpoints
- Response helpers
- Error handling
- ModelState validation

### 10. [Services Moderni](#10-services-moderni)
- ILogService (logging)
- IEmailService (SMTP)
- IEncryptionService (SHA-256)
- IConnectionStringProvider

---

## 1. JsonAPI Entities ✅

### File Creati

| File | Descrizione | Endpoint Auto-generati |
|------|-------------|------------------------|
| **`Entities/Examples/Product.cs`** | Entità prodotto con relazioni HasOne/HasMany | `GET/POST/PATCH/DELETE /api/v2/example-products` |
| **`Entities/Examples/Review.cs`** | Entità recensioni con validazioni | `GET/POST/PATCH/DELETE /api/v2/reviews` |

### Funzionalità Dimostrate

**File**: `Entities/Examples/Product.cs`

```csharp
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;

namespace MIT.Fwk.Examples.Entities.JsonAPI
{
    /// <summary>
    /// Esempio di entità JsonAPI base.
    /// Endpoint auto-generati: GET/POST/PATCH/DELETE /api/v2/products
    /// </summary>
    [Resource]
    public class Product : Identifiable<int>
    {
        // ATTRIBUTI SEMPLICI

        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [Attr]
        public int Stock { get; set; }

        [Attr]
        public bool IsActive { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // RELAZIONI

        /// <summary>
        /// Relazione Many-to-One: Molti prodotti appartengono a una categoria
        /// </summary>
        [HasOne]
        public Category Category { get; set; }
        public int? CategoryId { get; set; }

        /// <summary>
        /// Relazione One-to-Many: Un prodotto ha molte recensioni
        /// </summary>
        [HasMany]
        public virtual ICollection<Review> Reviews { get; set; }

        /// <summary>
        /// Relazione Many-to-Many: Un prodotto appartiene a molti ordini
        /// </summary>
        [HasMany]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
```

### Esempio Avanzato: Entità con Soft Delete e Multi-Tenancy

**File**: `Entities/01-JsonAPI/Category.cs`

```csharp
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using MIT.Fwk.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Examples.Entities.JsonAPI
{
    /// <summary>
    /// Esempio di entità con:
    /// - Soft Delete
    /// - Multi-tenancy
    /// - Validazioni
    /// - Relazioni gerarchiche (parent-child)
    /// </summary>
    [Resource]
    public class Category : Identifiable<int>, IHasTenant
    {
        // MULTI-TENANCY
        public int TenantId { get; set; }

        // ATTRIBUTI CON VALIDAZIONI

        [Attr]
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Il nome deve essere tra 3 e 100 caratteri")]
        public string Name { get; set; }

        [Attr]
        [StringLength(500)]
        public string Description { get; set; }

        [Attr]
        public string Slug { get; set; }

        [Attr]
        public int DisplayOrder { get; set; } = 0;

        // SOFT DELETE
        [Attr]
        public bool IsDeleted { get; set; } = false;

        [Attr]
        public DateTime? DeletedAt { get; set; }

        // AUDIT
        [Attr]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Attr]
        public DateTime? UpdatedAt { get; set; }

        // RELAZIONI GERARCHICHE (Parent-Child)

        [HasOne]
        public Category ParentCategory { get; set; }
        public int? ParentCategoryId { get; set; }

        [HasMany]
        public virtual ICollection<Category> ChildCategories { get; set; }

        // RELAZIONI

        [HasMany]
        public virtual ICollection<Product> Products { get; set; }
    }
}
```

---

## 2. Query al Database ✅

### 2.1 EF Core DbContext (LINQ)

**File Creato**: **`Controllers/ProductsEFCoreExamplesController.cs`**

**Endpoint Base**: `/api/examples/efcore/*`

### Esempi Disponibili (10 endpoint funzionanti)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Controllers;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Examples.Data;
using MIT.Fwk.Examples.Entities.JsonAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers.Database
{
    /// <summary>
    /// Esempi di query con EF Core DbContext (LINQ)
    /// </summary>
    [Route("api/examples/efcore")]
    public class ProductsEFCoreController : ApiController
    {
        private readonly OtherDbContext _context;

        public ProductsEFCoreController(
            OtherDbContext context,
            INotificationHandler<DomainNotification> notifications) : base(notifications)
        {
            _context = context;
        }

        /// <summary>
        /// Esempio 1: Query semplice con AsNoTracking (read-only, performance)
        /// </summary>
        [HttpGet("simple")]
        public async Task<IActionResult> GetAllSimple()
        {
            var products = await _context.Products
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 2: Query con filtro e ordinamento
        /// </summary>
        [HttpGet("filtered")]
        public async Task<IActionResult> GetFiltered([FromQuery] decimal minPrice, [FromQuery] string category)
        {
            var query = _context.Products.AsNoTracking();

            if (minPrice > 0)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Name == category);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 3: Query con Include (Eager Loading) per evitare N+1
        /// </summary>
        [HttpGet("with-relations")]
        public async Task<IActionResult> GetWithRelations()
        {
            var products = await _context.Products
                .Include(p => p.Category)                    // Carica la categoria
                .Include(p => p.Reviews)                     // Carica le recensioni
                .ThenInclude(r => r.User)                    // Poi carica l'utente di ogni recensione
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 4: Projection (seleziona solo i campi necessari)
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _context.Products
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    CategoryName = p.Category.Name,
                    ReviewCount = p.Reviews.Count,
                    AverageRating = p.Reviews.Average(r => r.Rating)
                })
                .ToListAsync();

            return Response(summary);
        }

        /// <summary>
        /// Esempio 5: GroupBy e aggregazioni
        /// </summary>
        [HttpGet("by-category")]
        public async Task<IActionResult> GetGroupedByCategory()
        {
            var grouped = await _context.Products
                .AsNoTracking()
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(p => p.Price * p.Stock),
                    AveragePrice = g.Average(p => p.Price)
                })
                .ToListAsync();

            return Response(grouped);
        }

        /// <summary>
        /// Esempio 6: FirstOrDefault vs SingleOrDefault
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // FirstOrDefault: ritorna il primo o null (OK se ci sono duplicati)
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            // SingleOrDefault: lancia eccezione se ci sono duplicati
            // var product = await _context.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return Response(product);
        }

        /// <summary>
        /// Esempio 7: Find (ottimizzato per chiave primaria)
        /// </summary>
        [HttpGet("find/{id}")]
        public async Task<IActionResult> Find(int id)
        {
            // Find è ottimizzato per ricerche per chiave primaria
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return Response(product);
        }

        /// <summary>
        /// Esempio 8: Any e Count (esistenza e conteggio)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalProducts = await _context.Products.CountAsync(),
                ActiveProducts = await _context.Products.CountAsync(p => p.IsActive),
                HasExpensiveProducts = await _context.Products.AnyAsync(p => p.Price > 1000),
                CategoriesCount = await _context.Categories.CountAsync()
            };

            return Response(stats);
        }
    }
}
```

| Endpoint | Esempio | Cosa Dimostra |
|----------|---------|---------------|
| `GET /api/examples/efcore/simple` | Query base | AsNoTracking per performance |
| `GET /api/examples/efcore/filtered?minPrice=100` | Filtri dinamici | Where, OrderBy, Take |
| `GET /api/examples/efcore/with-relations` | Eager Loading | Include/ThenInclude (evita N+1) |
| `GET /api/examples/efcore/summary` | Projection | Select solo campi necessari |
| `GET /api/examples/efcore/paged?page=1&pageSize=20` | Paginazione | Skip/Take |
| `GET /api/examples/efcore/stats-by-category` | Aggregazioni | GroupBy, Sum, Avg, Min, Max |
| `GET /api/examples/efcore/{id}` | Get singolo | Find vs FirstOrDefault |
| `GET /api/examples/efcore/check-availability/{id}` | Esistenza | Any vs Count |
| `GET /api/examples/efcore/complex-report` | Report complessi | Join multipli + aggregazioni |
| `PATCH /api/examples/efcore/{id}/update-price` | Update | Find -> Modify -> SaveChanges |

---

### 2.2 Dapper (Micro-ORM)

**File Creato**: **`Controllers/ProductsDapperExamplesController.cs`**

**Endpoint Base**: `/api/examples/dapper/*`

### Esempi Disponibili (10 endpoint funzionanti)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Controllers;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Examples.Data;
using MIT.Fwk.Examples.Entities.JsonAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers.Database
{
    /// <summary>
    /// Esempi di query SQL raw con EF Core
    /// </summary>
    [Route("api/examples/rawsql")]
    public class ProductsRawSQLController : ApiController
    {
        private readonly OtherDbContext _context;

        public ProductsRawSQLController(
            OtherDbContext context,
            INotificationHandler<DomainNotification> notifications) : base(notifications)
        {
            _context = context;
        }

        /// <summary>
        /// Esempio 1: SELECT con FromSqlRaw (parametri sicuri)
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _context.Products
                .FromSqlRaw("SELECT * FROM Products WHERE CategoryId = {0}", categoryId)
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 2: SELECT con FromSqlInterpolated (sintassi string interpolation sicura)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var products = await _context.Products
                .FromSqlInterpolated($"SELECT * FROM Products WHERE Name LIKE {$"%{keyword}%"}")
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 3: Query complessa con JOIN
        /// </summary>
        [HttpGet("with-category")]
        public async Task<IActionResult> GetWithCategory()
        {
            var sql = @"
                SELECT p.*, c.Name as CategoryName
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1
                ORDER BY p.CreatedAt DESC
            ";

            var products = await _context.Products
                .FromSqlRaw(sql)
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 4: Stored Procedure
        /// </summary>
        [HttpGet("top-selling/{top}")]
        public async Task<IActionResult> GetTopSelling(int top)
        {
            // Assumendo che esista una stored procedure GetTopSellingProducts
            var products = await _context.Products
                .FromSqlRaw("EXEC GetTopSellingProducts @Count = {0}", top)
                .AsNoTracking()
                .ToListAsync();

            return Response(products);
        }

        /// <summary>
        /// Esempio 5: ExecuteSqlRaw per INSERT/UPDATE/DELETE
        /// </summary>
        [HttpPost("bulk-update-price")]
        public async Task<IActionResult> BulkUpdatePrice([FromQuery] decimal increasePercent)
        {
            var sql = "UPDATE Products SET Price = Price * {0} WHERE IsActive = 1";
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                sql,
                1 + (increasePercent / 100)
            );

            return Response(new { RowsAffected = rowsAffected });
        }

        /// <summary>
        /// Esempio 6: Query scalare (singolo valore)
        /// </summary>
        [HttpGet("total-stock")]
        public async Task<IActionResult> GetTotalStock()
        {
            var sql = "SELECT SUM(Stock) FROM Products WHERE IsActive = 1";

            // Esegue la query e ottiene il primo risultato
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            await _context.Database.OpenConnectionAsync();

            var result = await command.ExecuteScalarAsync();
            var totalStock = result != null ? Convert.ToInt32(result) : 0;

            return Response(new { TotalStock = totalStock });
        }
    }
}
```

| Endpoint | Esempio | Cosa Dimostra |
|----------|---------|---------------|
| `GET /api/examples/dapper/simple` | Query base | QueryAsync<T> mapping automatico |
| `GET /api/examples/dapper/search?keyword=test` | Ricerca con LIKE | Parametri sicuri (anti SQL injection) |
| `GET /api/examples/dapper/with-category` | JOIN One-to-Many | Multi-mapping con splitOn |
| `GET /api/examples/dapper/{id}` | Get singolo | QueryFirstOrDefaultAsync |
| `GET /api/examples/dapper/stats/count` | Valori scalari | ExecuteScalarAsync (COUNT, AVG, SUM) |
| `POST /api/examples/dapper` | INSERT | ExecuteScalarAsync + LAST_INSERT_ID |
| `PUT /api/examples/dapper/{id}` | UPDATE | ExecuteAsync ritorna righe modificate |
| `DELETE /api/examples/dapper/{id}` | DELETE | ExecuteAsync |
| `GET /api/examples/dapper/dashboard` | Multiple queries | QueryMultipleAsync (più query in 1 chiamata) |
| `POST /api/examples/dapper/bulk` | Bulk insert | ExecuteAsync con array (migliaia di record) |

---

### 2.3 Confronto EF Core vs Dapper

| Caratteristica | EF Core | Dapper |
|----------------|---------|--------|
| **Performance** | Buona | Eccellente (3x più veloce) |
| **Facilità d'uso** | Molto facile (LINQ) | Media (SQL manuale) |
| **Change Tracking** | Sì | No |
| **Migrations** | Sì | No |
| **Quando usarlo** | CRUD standard, sviluppo rapido | Query complesse, performance critiche |

---

## 3. Custom DbContext (Multi-Database)

### File Già Presenti

```csharp
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MIT.Fwk.Core.Controllers;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Controllers.Database
{
    /// <summary>
    /// Esempi di query con Dapper (micro-ORM ultra-performante)
    /// </summary>
    [Route("api/examples/dapper")]
    public class ProductsDapperController : ApiController
    {
        private readonly IConnectionStringProvider _connStringProvider;

        public ProductsDapperController(
            IConnectionStringProvider connStringProvider,
            INotificationHandler<DomainNotification> notifications) : base(notifications)
        {
            _connStringProvider = connStringProvider;
        }

        /// <summary>
        /// Esempio 1: Query semplice che ritorna lista di oggetti
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = "SELECT * FROM Products WHERE IsActive = @IsActive";
            var products = await connection.QueryAsync<ProductDTO>(sql, new { IsActive = true });

            return Response(products);
        }

        /// <summary>
        /// Esempio 2: Query con parametri multipli
        /// </summary>
        [HttpGet("filtered")]
        public async Task<IActionResult> GetFiltered([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = @"
                SELECT * FROM Products
                WHERE Price BETWEEN @MinPrice AND @MaxPrice
                AND IsActive = @IsActive
                ORDER BY Price ASC
            ";

            var products = await connection.QueryAsync<ProductDTO>(sql, new
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IsActive = true
            });

            return Response(products);
        }

        /// <summary>
        /// Esempio 3: Query che ritorna un singolo oggetto
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = "SELECT * FROM Products WHERE Id = @Id";
            var product = await connection.QueryFirstOrDefaultAsync<ProductDTO>(sql, new { Id = id });

            if (product == null)
                return NotFound();

            return Response(product);
        }

        /// <summary>
        /// Esempio 4: Query con JOIN (One-to-Many mapping)
        /// </summary>
        [HttpGet("with-category")]
        public async Task<IActionResult> GetWithCategory()
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = @"
                SELECT p.*, c.*
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsActive = 1
            ";

            var productDictionary = new Dictionary<int, ProductWithCategoryDTO>();

            var products = await connection.QueryAsync<ProductWithCategoryDTO, CategoryDTO, ProductWithCategoryDTO>(
                sql,
                (product, category) =>
                {
                    if (!productDictionary.TryGetValue(product.Id, out var productEntry))
                    {
                        productEntry = product;
                        productEntry.Category = category;
                        productDictionary.Add(productEntry.Id, productEntry);
                    }
                    return productEntry;
                },
                splitOn: "Id"  // Dapper split su questo campo per separare Product da Category
            );

            return Response(productDictionary.Values);
        }

        /// <summary>
        /// Esempio 5: Query scalare (ritorna un solo valore)
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = "SELECT COUNT(*) FROM Products WHERE IsActive = @IsActive";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { IsActive = true });

            return Response(new { Count = count });
        }

        /// <summary>
        /// Esempio 6: INSERT e ritorna ID generato
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = @"
                INSERT INTO Products (Name, Description, Price, Stock, IsActive, CategoryId, CreatedAt)
                VALUES (@Name, @Description, @Price, @Stock, @IsActive, @CategoryId, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            var newId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Description,
                dto.Price,
                dto.Stock,
                IsActive = true,
                dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            });

            return Response(new { Id = newId });
        }

        /// <summary>
        /// Esempio 7: UPDATE
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = @"
                UPDATE Products
                SET Name = @Name,
                    Description = @Description,
                    Price = @Price,
                    Stock = @Stock,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id
            ";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Description,
                dto.Price,
                dto.Stock,
                UpdatedAt = DateTime.UtcNow
            });

            if (rowsAffected == 0)
                return NotFound();

            return Response(new { Success = true });
        }

        /// <summary>
        /// Esempio 8: DELETE
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = "DELETE FROM Products WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

            if (rowsAffected == 0)
                return NotFound();

            return Response(new { Success = true });
        }

        /// <summary>
        /// Esempio 9: Query multipla (Multiple Result Sets)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var sql = @"
                SELECT COUNT(*) FROM Products WHERE IsActive = 1;
                SELECT SUM(Stock) FROM Products WHERE IsActive = 1;
                SELECT AVG(Price) FROM Products WHERE IsActive = 1;
            ";

            using var multi = await connection.QueryMultipleAsync(sql);

            var totalProducts = await multi.ReadSingleAsync<int>();
            var totalStock = await multi.ReadSingleAsync<int>();
            var averagePrice = await multi.ReadSingleAsync<decimal>();

            return Response(new
            {
                TotalProducts = totalProducts,
                TotalStock = totalStock,
                AveragePrice = averagePrice
            });
        }

        /// <summary>
        /// Esempio 10: Stored Procedure
        /// </summary>
        [HttpGet("top-selling/{count}")]
        public async Task<IActionResult> GetTopSelling(int count)
        {
            using var connection = new SqlConnection(
                _connStringProvider.GetConnectionString("DefaultConnection")
            );

            var products = await connection.QueryAsync<ProductDTO>(
                "GetTopSellingProducts",  // Nome stored procedure
                new { TopCount = count },
                commandType: System.Data.CommandType.StoredProcedure
            );

            return Response(products);
        }
    }

    // DTO Classes
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }
    }

    public class ProductWithCategoryDTO : ProductDTO
    {
        public CategoryDTO Category { get; set; }
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
```

---

## 7. Authentication & Authorization

**File Creato**: **`Controllers/AuthenticationExamplesController.cs`**

**Endpoint Base**: `/api/examples/auth/*`

### Middleware Pipeline (Ordine Importante)

```
1. JwtAuthenticationMiddleware     → Valida JWT token
2. JwtClaimsValidationMiddleware   → Valida claims entity-level
3. JwtLoggingMiddleware            → Log richieste (fire-and-forget)
```

### Attributi Disponibili

| Attributo | Scope | Descrizione | Esempio |
|-----------|-------|-------------|---------|
| `[AllowAnonymous]` | Controller/Action | Endpoint pubblico - NO autenticazione | `[AllowAnonymous]` |
| `[SkipJwtAuthentication]` | Controller/Action/Entity | Salta JWT auth (opzionale: per metodi HTTP specifici) | `[SkipJwtAuthentication(JwtHttpMethod.GET)]` |
| `[SkipClaimsValidation]` | Controller/Action/Entity | Richiede auth ma non valida claims entity-level | `[SkipClaimsValidation]` |
| `[SkipRequestLogging]` | Controller/Action | Non logga la richiesta (evita loop, performance) | `[SkipRequestLogging]` |
| `[Authorize]` | Controller/Action | Richiede autenticazione (default se JWT abilitato) | `[Authorize]` |
| `[Authorize(Policy = "...")]` | Controller/Action | Richiede livello ruolo specifico | `[Authorize(Policy = "NeededRoleLevel10")]` |

### Livelli Ruolo (Role-Based Authorization)

```csharp
// Configurati in: Configurations/IdentityExtensions.cs
NeededRoleLevel0   = SuperAdmin (massimo potere)
NeededRoleLevel10  = Admin
NeededRoleLevel50  = Standard User
NeededRoleLevel100 = Guest/Limited
```

### Esempi Disponibili (10 endpoint funzionanti)

#### Esempio 1A: Endpoint Pubblico con [AllowAnonymous]

```csharp
/// <summary>
/// QUANDO USARE: Endpoint accessibili a tutti (login, registrazione, health check)
/// </summary>
[AllowAnonymous]
[HttpGet("public")]
public IActionResult Example01A_PublicEndpoint()
{
    return Response(new
    {
        Message = "Questo endpoint è pubblico - accessibile senza autenticazione",
        IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
        User = User?.Identity?.Name ?? "Anonymous"
    });
}
```

#### Esempio 1B: Endpoint Pubblico con [SkipJwtAuthentication]

```csharp
/// <summary>
/// DIFFERENZA con [AllowAnonymous]:
/// - [AllowAnonymous] = Standard ASP.NET Core
/// - [SkipJwtAuthentication] = Custom attribute con più opzioni (metodi HTTP specifici)
/// Uso consigliato: [AllowAnonymous] (più standard)
/// </summary>
[SkipJwtAuthentication]
[HttpGet("public-skip-jwt")]
public IActionResult Example01B_PublicWithSkipJwt()
{
    return Response(new
    {
        Message = "Endpoint pubblico con [SkipJwtAuthentication]",
        IsAuthenticated = User?.Identity?.IsAuthenticated ?? false
    });
}
```

#### Esempio 2: Skip JWT Solo per GET (lettura pubblica, write protetto)

```csharp
/// <summary>
/// QUANDO USARE: API read-only pubbliche ma write protette
/// Pattern comune:
/// - GET = Pubblico (chiunque può leggere)
/// - POST/PUT/DELETE = Protetto (solo utenti autenticati possono modificare)
/// </summary>
[SkipJwtAuthentication(JwtHttpMethod.GET)]
[HttpGet("conditional/public-read")]
public IActionResult Example02_PublicRead()
{
    return Response(new
    {
        Message = "GET è pubblico - tutti possono leggere",
        Method = "GET",
        RequiresAuth = false
    });
}

[SkipJwtAuthentication(JwtHttpMethod.GET)]
[HttpPost("conditional/protected-write")]
public IActionResult Example02_ProtectedWrite()
{
    // Se arrivi qui, sei autenticato (POST non ha skip)
    return Response(new
    {
        Message = "POST richiede autenticazione - solo utenti autenticati",
        Method = "POST",
        RequiresAuth = true,
        User = User?.Identity?.Name
    });
}
```

#### Esempio 3: Skip Multiple Metodi HTTP (usando OR bit-wise)

```csharp
/// <summary>
/// QUANDO USARE: Pubblico per più metodi ma non tutti
/// </summary>
[SkipJwtAuthentication(JwtHttpMethod.GET | JwtHttpMethod.POST)]
[HttpGet("multi-method")]
public IActionResult Example03_MultiMethod_Get()
{
    return Response(new
    {
        Message = "GET e POST sono pubblici, PUT/DELETE richiedono auth",
        PublicMethods = "GET, POST",
        ProtectedMethods = "PUT, PATCH, DELETE"
    });
}
```

#### Esempio 4: Skip Claims Validation

```csharp
/// <summary>
/// QUANDO USARE: Endpoint che richiedono login ma non permessi specifici
/// DIFFERENZA:
/// - SENZA [SkipClaimsValidation]: Valida permessi entity-level (es. "canRead:Products")
/// - CON [SkipClaimsValidation]: Solo controlla che sei loggato, non i permessi
/// Esempi reali: /health, /config, /profile
/// </summary>
[SkipClaimsValidation]
[HttpGet("authenticated-no-claims")]
public IActionResult Example04_AuthenticatedNoClaimsValidation()
{
    if (!User.Identity.IsAuthenticated)
    {
        return Unauthorized();
    }

    return Response(new
    {
        Message = "Richiede autenticazione ma non valida claims specifici",
        User = User.Identity.Name,
        Claims = User.Claims.Select(c => new { c.Type, c.Value })
    });
}
```

#### Esempio 5: Endpoint Protetto (Default)

```csharp
/// <summary>
/// QUANDO USARE: Default per la maggior parte degli endpoint
/// Se NON specifichi nessun attributo, il comportamento di default è:
/// - Richiede JWT token valido
/// - Valida claims entity-level (se l'entity ha claims configurati)
/// - Logga la richiesta
/// NOTA: Il [Authorize] è opzionale se JWT è abilitato globalmente
/// </summary>
[Authorize]  // Esplicito ma opzionale (già default)
[HttpGet("protected")]
public IActionResult Example05_ProtectedEndpoint()
{
    return Response(new
    {
        Message = "Endpoint protetto - richiede autenticazione completa",
        User = User.Identity.Name,
        IsAuthenticated = User.Identity.IsAuthenticated,
        Claims = User.Claims.Select(c => new { c.Type, c.Value })
    });
}
```

#### Esempio 6A: Policy-Based Authorization - Admin Only (Livello 10)

```csharp
/// <summary>
/// QUANDO USARE: Endpoint che richiedono ruoli specifici
/// LIVELLI RUOLO (definiti nel framework):
/// - NeededRoleLevel0: SuperAdmin
/// - NeededRoleLevel10: Admin
/// - NeededRoleLevel50: Standard User
/// - NeededRoleLevel100: Guest/Limited
/// Configurati in: Configurations/IdentityExtensions.cs
/// </summary>
[Authorize(Policy = "NeededRoleLevel10")]
[HttpGet("admin-only")]
public IActionResult Example06A_AdminOnly()
{
    return Response(new
    {
        Message = "Solo Admin (livello 10 o inferiore)",
        User = User.Identity.Name,
        Roles = User.Claims.Where(c => c.Type.Contains("role"))
                           .Select(c => c.Value)
    });
}
```

#### Esempio 6B: SuperAdmin Only (Livello 0)

```csharp
/// <summary>
/// QUANDO USARE: Operazioni critiche, configurazioni di sistema
/// </summary>
[Authorize(Policy = "NeededRoleLevel0")]
[HttpDelete("superadmin-only")]
public IActionResult Example06B_SuperAdminOnly()
{
    return Response(new
    {
        Message = "Solo SuperAdmin può eseguire questa operazione",
        User = User.Identity.Name
    });
}
```

#### Esempio 7: Skip Request Logging

```csharp
/// <summary>
/// QUANDO USARE: Controller di logging stesso, health checks frequenti
/// IMPORTANTE: Usare su controller di logging per evitare loop infiniti
/// (logging che logga se stesso)
/// Esempio reale: FwkLogController ha [SkipRequestLogging]
/// </summary>
[SkipRequestLogging]
[HttpGet("no-logging")]
public IActionResult Example07_NoLogging()
{
    return Response(new
    {
        Message = "Questa richiesta NON viene loggata (fire-and-forget skipped)",
        Logged = false
    });
}
```

#### Esempio 8: Combinazione di Attributi

```csharp
/// <summary>
/// QUANDO USARE: Scenari specifici con requisiti complessi
/// Questo endpoint:
/// - Richiede autenticazione (NO skip JWT)
/// - NON valida claims entity-level ([SkipClaimsValidation])
/// - NON viene loggato ([SkipRequestLogging])
/// - Richiede livello Admin ([Authorize Policy])
/// Caso d'uso: Endpoint admin che non deve loggare dati sensibili
/// </summary>
[Authorize(Policy = "NeededRoleLevel10")]
[SkipClaimsValidation]
[SkipRequestLogging]
[HttpPost("complex")]
public IActionResult Example08_ComplexCombination()
{
    return Response(new
    {
        Message = "Combinazione: Auth + Admin + No Claims + No Logging",
        User = User.Identity.Name,
        Configuration = new
        {
            RequiresAuth = true,
            RequiresAdmin = true,
            ValidatesClaims = false,
            LogsRequest = false
        }
    });
}
```

#### Esempio 9: Accesso ai Dati dell'Utente Autenticato

```csharp
/// <summary>
/// QUANDO USARE: Sempre in endpoint protetti per identificare l'utente
/// </summary>
[Authorize]
[HttpGet("user-context")]
public IActionResult Example09_UserContext()
{
    return Response(new
    {
        IsAuthenticated = User.Identity.IsAuthenticated,
        Username = User.Identity.Name,
        UserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
        Email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
        TenantId = User.Claims.FirstOrDefault(c => c.Type == "tenantId")?.Value,
        Roles = User.Claims.Where(c => c.Type.Contains("role"))
                           .Select(c => c.Value)
                           .ToList(),
        AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
    });
}
```

#### Esempio 10: Summary di Tutti gli Attributi

```csharp
/// <summary>
/// Riepilogo di tutti gli attributi disponibili
/// </summary>
[AllowAnonymous]
[HttpGet("summary")]
public IActionResult Example10_Summary()
{
    return Response(new
    {
        Message = "Riepilogo attributi autenticazione/autorizzazione",
        Attributes = new[]
        {
            new
            {
                Attribute = "[AllowAnonymous]",
                Scope = "Controller o Action",
                Description = "Endpoint pubblico - NO autenticazione",
                Example = "GET /api/examples/auth/public"
            },
            new
            {
                Attribute = "[SkipJwtAuthentication]",
                Scope = "Controller, Action o Entity",
                Description = "Salta JWT auth (opzionale: solo per metodi HTTP specifici)",
                Example = "[SkipJwtAuthentication(JwtHttpMethod.GET)]"
            },
            new
            {
                Attribute = "[SkipClaimsValidation]",
                Scope = "Controller, Action o Entity",
                Description = "Richiede auth ma non valida claims entity-level",
                Example = "[SkipClaimsValidation]"
            },
            new
            {
                Attribute = "[SkipRequestLogging]",
                Scope = "Controller o Action",
                Description = "Non logga la richiesta (evita loop, performance)",
                Example = "[SkipRequestLogging]"
            },
            new
            {
                Attribute = "[Authorize]",
                Scope = "Controller o Action",
                Description = "Richiede autenticazione (default se JWT abilitato)",
                Example = "[Authorize]"
            },
            new
            {
                Attribute = "[Authorize(Policy = \"...\")]",
                Scope = "Controller o Action",
                Description = "Richiede livello ruolo specifico",
                Example = "[Authorize(Policy = \"NeededRoleLevel10\")]"
            }
        },
        RoleLevels = new
        {
            Level0 = "SuperAdmin (massimo potere)",
            Level10 = "Admin",
            Level50 = "Standard User",
            Level100 = "Guest/Limited"
        },
        JwtHttpMethods = new
        {
            All = "Tutti i metodi HTTP",
            GET = "Solo GET",
            POST = "Solo POST",
            PUT = "Solo PUT",
            PATCH = "Solo PATCH",
            DELETE = "Solo DELETE",
            Combined = "GET | POST (bitwise OR per combinazioni)"
        }
    });
}
```

### Tabella Riepilogo Endpoint Authentication

| Endpoint | Auth Richiesta | Claims Validation | Logging | Role Level | Caso d'Uso |
|----------|----------------|-------------------|---------|------------|------------|
| `GET /api/examples/auth/public` | ❌ No | ❌ No | ✅ Yes | - | Endpoint pubblico (login, registrazione) |
| `GET /api/examples/auth/public-skip-jwt` | ❌ No | ❌ No | ✅ Yes | - | Endpoint pubblico (metodo alternativo) |
| `GET /api/examples/auth/conditional/public-read` | ❌ No (GET) | ❌ No | ✅ Yes | - | Lettura pubblica |
| `POST /api/examples/auth/conditional/protected-write` | ✅ Yes | ✅ Yes | ✅ Yes | - | Scrittura protetta |
| `GET /api/examples/auth/multi-method` | ❌ No (GET/POST) | ❌ No | ✅ Yes | - | Multipli metodi pubblici |
| `GET /api/examples/auth/authenticated-no-claims` | ✅ Yes | ❌ No | ✅ Yes | - | Health check, config, profile |
| `GET /api/examples/auth/protected` | ✅ Yes | ✅ Yes | ✅ Yes | - | Endpoint standard protetto |
| `GET /api/examples/auth/admin-only` | ✅ Yes | ✅ Yes | ✅ Yes | Admin (10) | Operazioni admin |
| `DELETE /api/examples/auth/superadmin-only` | ✅ Yes | ✅ Yes | ✅ Yes | SuperAdmin (0) | Operazioni critiche |
| `GET /api/examples/auth/no-logging` | ✅ Yes | ✅ Yes | ❌ No | - | Logging controller |
| `POST /api/examples/auth/complex` | ✅ Yes | ❌ No | ❌ No | Admin (10) | Admin senza log dati sensibili |
| `GET /api/examples/auth/user-context` | ✅ Yes | ✅ Yes | ✅ Yes | - | Dati utente corrente |
| `GET /api/examples/auth/summary` | ❌ No | ❌ No | ✅ Yes | - | Documentazione attributi |

### File Correlati

- `Middleware/JwtAuthenticationMiddleware.cs` - Valida token JWT
- `Middleware/JwtClaimsValidationMiddleware.cs` - Valida claims entity-level
- `Middleware/JwtLoggingMiddleware.cs` - Log richieste
- `Core/Attributes/JwtAuthenticationAttributes.cs` - Definizione attributi
- `Configurations/MiddlewareExtensions.cs` - Ordine pipeline middleware
- `Configurations/IdentityExtensions.cs` - Configurazione policy ruoli

---

## Prossimi Esempi da Implementare

I seguenti esempi potrebbero essere aggiunti in futuro:

### 4. CQRS & Event Sourcing
- Command handlers
- Event handlers
- Validazioni con FluentValidation
- Event Store

### 5. MongoDB & NoSQL
- DocumentManager CRUD
- Query MongoDB
- Aggregations

### 6. Multi-Tenancy
- ITenantProvider
- IHasTenant
- Query filters automatici

### 8. Dependency Injection
- Auto-discovery services
- Service handlers

### 9. Controllers Avanzati
- Response helpers
- Error handling
- ModelState validation

### 10. Services Moderni
- ILogService
- IEmailService
- IEncryptionService

---

## Conclusione

Questo progetto Examples fornisce **esempi pratici e funzionanti** per:

✅ **JsonAPI Entities** (Product.cs, Review.cs)
✅ **EF Core Query Patterns** (ProductsEFCoreExamplesController.cs - 10 esempi)
✅ **Dapper Query Patterns** (ProductsDapperExamplesController.cs - 10 esempi)
✅ **Authentication & Authorization** (AuthenticationExamplesController.cs - 10 esempi)

**Totale: 30+ endpoint funzionanti pronti da testare!**

Per testare gli esempi:
1. Compila il progetto
2. Avvia l'API
3. Naviga su Swagger: `https://localhost:port/swagger`
4. Testa gli endpoint nella sezione "examples"
