# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MIT.FWK (Maestrale Framework) is an enterprise-grade .NET 8.0 framework implementing Domain-Driven Design (DDD) with CQRS (Command Query Responsibility Segregation) and Event Sourcing patterns. The framework uses a **fork-based architecture** where developers fork the repository and add custom business logic directly to the codebase.

## Architecture & Fork-Based Approach

### Core Concept

**Fork-Based Architecture** (v8.x+):
- **Single Repository**: Framework and custom modules in one repository
- **Direct Compilation**: All code compiles together - no dynamic loading
- **Project References**: Custom modules referenced via standard .csproj references
- **Better Performance**: No reflection overhead for plugin discovery
- **Simpler Deployment**: Single binary output to `C:\MaeFWK\Runtime\Bin`
- **Improved IDE Support**: Full IntelliSense, refactoring, and debugging

**Migration Note**: Previous versions used external plugin loading (`C:\MaeFWK\MIT.Customs\`). This approach has been deprecated.

### Layer Architecture

```
Common Layer (MIT.Fwk.Core)
├── Interfaces, base models, helpers
├── Domain contracts (IUnitOfWork, IDocument)
└── Multi-tenancy support (ITenantProvider)

Domain Layer (MIT.Fwk.Domain)
├── Command/Event handlers (CQRS)
└── Domain commands and validations

Infrastructure Layer
├── MIT.Fwk.Infra.Bus (MediatR-based CQRS bus)
├── MIT.Fwk.Infra.IoC (DI bootstrapping)
├── MIT.Fwk.Infra.Identity (ASP.NET Core Identity + multi-tenant)
├── MIT.Fwk.Infra.EF (EF Core DbContext + migrations)
├── MIT.Fwk.Infra.Data (MongoDB for logging/documents)
└── MIT.Fwk.Infra.Services (IJsonApiManualService, IDocumentService, IFwkLogService)

Presentation Layer (MIT.Fwk.WebApi)
└── Controllers (extend ApiController), JsonAPI endpoints, Swagger

Custom Modules (MIT.Fwk.Examples)
└── Example custom modules (OtherDBManagementExample)

Scheduling (MIT.Fwk.Scheduler)
└── Quartz.NET jobs
```

**Architecture Evolution (v8.0+)**:
- ❌ **Removed Legacy Layers** (v8.0 refactoring):
  - SqlManager/SqlManagerV2 (replaced by EF Core DbContext)
  - IRepository/IRepositoryV2 (replaced by DbContext.Set<T>())
  - IAppService/IAppServiceV2 (replaced by IJsonApiManualService + DbContext)
  - BaseController/BaseControllerV2 (replaced by ApiController)
  - BaseAuthController/BaseAuthControllerV2 (replaced by ApiController)

- ✅ **Modern Data Access Pattern**:
  - **EF Core DbContext**: Primary ORM for CRUD operations
  - **IJsonApiManualService**: Complex queries and cross-entity operations
  - **Direct DbContext injection**: Controllers inject DbContext directly
  - **AsNoTracking()**: For read-only queries (performance optimization)

### Module Discovery Mechanism

**Auto-Discovery Pattern**:
1. Framework auto-discovers implementations of key interfaces from loaded assemblies
2. No external path scanning - uses `AppDomain.CurrentDomain.GetAssemblies()`
3. Key discoverable interfaces:
   - `IJsonApiDbContext` - Auto-discovered DbContexts for JsonAPI
   - `IApplicationServiceHandler` - Optional service registration hooks
   - `IDomainServiceHandler` - Optional domain registration hooks

**Service Registration** (NativeInjectorBootStrapper.cs):
```csharp
// Auto-discovers handlers from all loaded MIT assemblies
var customApps = ReflectionHelper.ResolveAll<IApplicationServiceHandler>();
foreach (IApplicationServiceHandler app in customApps)
    app.Configure(services);

var customDomains = ReflectionHelper.ResolveAll<IDomainServiceHandler>();
foreach (IDomainServiceHandler dom in customDomains)
    dom.Configure(services);
```

### Custom Module Structure

Custom modules are added as projects within the solution:

```
Src/MIT.Fwk.Examples/
└── OtherDBManagementExample/
    ├── Data/
    │   ├── OtherDbContext.cs (implements IJsonApiDbContext)
    │   └── Migrations/
    │       └── OtherDbContext/  (auto-generated, per-context folder)
    │           ├── 20250122_InitialCreate.cs
    │           └── OtherDbContextModelSnapshot.cs
    ├── Entities/
    │   ├── Product.cs
    │   └── Category.cs
    ├── Services/
    │   └── ProductService.cs
    ├── Controllers/
    │   └── ProductsController.cs (optional - JsonAPI auto-generates)
    └── README.md
```

## Building the Framework

### Build the Complete Application

```bash
# Build main Web API project (compiles all dependencies including custom modules)
dotnet build Src/MIT.Fwk.WebApi/MIT.Fwk.WebApi.csproj --configuration Release

# PostBuild event automatically copies to C:\MaeFWK\Runtime\Bin
```

**Important**: PostBuild events copy everything to `C:\MaeFWK\Runtime\Bin`. Framework binaries and custom modules all deploy together.

### Build Configurations
- `Debug` - Development with full symbols
- `Release` - Production optimized
- `DOCKER` - Container deployment (disables licensing)
- Platform: `AnyCPU` or `x64`

## Running the Application

### Local Development
```bash
# Navigate to runtime directory
cd C:\MaeFWK\Runtime\Bin

# Run the Web API
.\startupWebApi.bat

# Or run directly with dotnet
dotnet MIT.Fwk.WebApi.dll
```

**Critical**: Always stop the running API before recompiling. PostBuild events will fail if files are locked.

### Run as Windows Service
```bash
# Register service (must run as Administrator)
MIT.Fwk.WebApi.exe --register-service --service-name "MyServiceName"

# Unregister service
MIT.Fwk.WebApi.exe --unregister-service --service-name "MyServiceName"
```

### Docker Container
```bash
# Framework automatically detects DOCKER build configuration
# License checks are disabled in container mode
docker run <container-with-framework>
```

### Swagger UI
Access at: `http://localhost:<port>/swagger` (if `EnableSwagger=true` in config)

## Configuration Files

### Framework Configuration (`C:\MaeFWK\Runtime\Bin\`)

**customsettings.json** - Framework settings:
```json
{
  // PluginsPath is DEPRECATED (fork-based architecture)
  // Specify database provider for custom DbContexts:
  "OtherDbContext": "Sql",  // or "MySql" for OtherDbContext
  "JsonApiSqlProvider": "Sql",  // Default provider
  "EnableSwagger": true,
  "EnableSSL": false,
  "EnableJobs": true,  // Quartz.NET scheduler
  "EnableAutoMigrations": true,
  "DatabaseMigrationOrder": [  // REQUIRED: Order of migration execution
    "JsonApiDbContext",         // Must be first (Identity + Setup tables)
    "OtherDbContext"            // Custom contexts follow
  ],
  "AllowedCorsOrigin": "*"
}
```

**Critical**: `DatabaseMigrationOrder` is **required** when `EnableAutoMigrations: true`. The framework will show an error if this parameter is missing or if discovered DbContexts are not listed. See [MIGRATIONS-ORDER-CONFIG.md](MIGRATIONS-ORDER-CONFIG.md).

**dbconnections.json** - Connection strings (key = DbContext class name):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MainDB;...",
    "JsonApiConnection": "Server=localhost;Database=MainDB;...",
    "OtherDbContext": "Server=localhost;Database=SecondaryDB;...",
    "NoSQLConnection": "mongodb://localhost:27017/FWK"
  }
}
```

**Key Principle**: DbContext class name MUST match the key in dbconnections.json exactly.

## JsonAPI & Entity Framework

### JsonAPI Entity Pattern
Entities with `[Resource]` attribute auto-generate CRUD endpoints:

```csharp
[Resource]
[Table("products")]
public class Product : Identifiable<int>
{
    [Attr]
    public string Name { get; set; }

    [HasMany]
    public virtual ICollection<Order> Orders { get; set; }
}
```

Generates endpoints: `GET/POST/PATCH/DELETE /api/v2/products`

### Custom DbContext Pattern (Multi-Database Support)

```csharp
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class OtherDbContext : DbContext, IJsonApiDbContext
{
    public static bool _UseSqlServer = true;
    private readonly ITenantProvider _tenantProvider;

    public OtherDbContext(DbContextOptions<OtherDbContext> options, ITenantProvider tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // Parameterless constructor for design-time
    public OtherDbContext() : this(_UseSqlServer
        ? new DbContextOptionsBuilder<OtherDbContext>()
            .UseSqlServer(ConfigurationHelper.GetStringFromDbConnection(nameof(OtherDbContext)))
            .Options
        : new DbContextOptionsBuilder<OtherDbContext>()
            .UseMySQL(ConfigurationHelper.GetStringFromDbConnection(nameof(OtherDbContext)))
            .Options, null)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (_UseSqlServer)
                optionsBuilder.UseSqlServer(ConfigurationHelper.GetStringFromDbConnection(nameof(OtherDbContext)));
            else
                optionsBuilder.UseMySQL(ConfigurationHelper.GetStringFromDbConnection(nameof(OtherDbContext)));
        }
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
}
```

**Critical**:
- MUST implement `IJsonApiDbContext` for auto-discovery
- `nameof(OtherDbContext)` must match key in dbconnections.json
- Flag `_UseSqlServer` set from configuration at startup

## Adding Custom Modules

### Step 1: Create Module Project

Create a new project in `Src/MIT.Fwk.Examples/` or similar:

```bash
dotnet new classlib -n MIT.Fwk.MyModule -f net8.0
```

### Step 2: Add Project Reference

In `MIT.Fwk.WebApi.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\MIT.Fwk.MyModule\MIT.Fwk.MyModule.csproj" />
</ItemGroup>
```

### Step 3: Create DbContext (Optional)

If you need a separate database:
```csharp
public class MyModuleDbContext : DbContext, IJsonApiDbContext
{
    // Follow pattern from OtherDbContext example
}
```

### Step 4: Create Entities

```csharp
[Resource]
[Table("my_entities")]
public class MyEntity : Identifiable<int>
{
    [Attr]
    public string Name { get; set; }
}
```

### Step 5: Register in Startup (if needed)

In `Startup.cs`, add your assembly to JsonAPI discovery:
```csharp
discovery.AddAssembly(typeof(MyModuleDbContext).Assembly);
```

### Step 6: Configure Database

Add to `customsettings.json`:
```json
{
  "MyModuleDbContext": "Sql",
  "DatabaseMigrationOrder": [
    "JsonApiDbContext",
    "OtherDbContext",
    "MyModuleDbContext"
  ]
}
```

**Important**: The `DatabaseMigrationOrder` parameter defines the order in which migrations are applied. `JsonApiDbContext` must always be first (contains Identity tables). See [MIGRATIONS-ORDER-CONFIG.md](MIGRATIONS-ORDER-CONFIG.md) for details.

Add to `dbconnections.json`:
```json
{
  "ConnectionStrings": {
    "MyModuleDbContext": "Server=localhost;Database=MyModuleDB;..."
  }
}
```

### Step 7: Run Migrations

**Option A: Using PowerShell Script (RECOMMENDED)**

```powershell
# Create initial migration for your new DbContext
.\Add-Migration.ps1 -Name "InitialCreate" -Context "MyModuleDbContext"

# Apply migration
.\Add-Migration.ps1 -Update -Context "MyModuleDbContext"

# Or enable auto-migrations in customsettings.json
# Migrations will be applied automatically on API startup
```

**Option B: Using EF Core CLI directly**

```bash
cd Src/MIT.Fwk.MyModule
dotnet ef migrations add InitialCreate \
  --context MyModuleDbContext \
  --startup-project ../MIT.Fwk.WebApi \
  --output-dir Migrations/MyModuleDbContext

dotnet ef database update \
  --context MyModuleDbContext \
  --startup-project ../MIT.Fwk.WebApi
```

**Note**: Each DbContext has its own migrations table (`__EFMigrationsHistory_{ContextName}`) configured automatically via `.MigrationsHistoryTable()` in `OnConfiguring()` and migrations folder (`Migrations/{ContextName}/`). See [MIGRATIONS-GUIDE.md](MIGRATIONS-GUIDE.md) for details.

## Dependency Injection Auto-Discovery

### Zero-Configuration Service Registration (v8.0+)

**NEW**: The framework automatically discovers and registers custom services with **zero configuration**!

#### Auto-Discovery for `*ManualService` Pattern

Create a custom service following the naming convention:
1. Interface: `I*ManualService` (e.g., `IOtherManualService`, `IMyCustomManualService`)
2. Implementation: `*ManualService` (e.g., `OtherManualService`, `MyCustomManualService`)

**That's it!** The framework automatically:
- Scans all MIT assemblies at startup
- Finds interfaces matching `I*ManualService` pattern
- Locates corresponding implementations
- Registers them as `AddScoped<IInterface, Implementation>`

**Example:**

```csharp
// Interfaces/IOtherManualService.cs
public interface IOtherManualService
{
    Task<List<T>> GetAllAsync<T, TId>() where T : Identifiable<TId> where TId : IConvertible;
    Task<T> CreateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;
    // ... more generic methods
}

// Services/OtherManualService.cs
public class OtherManualService : IOtherManualService
{
    private readonly OtherDbContext _context;

    public OtherManualService(OtherDbContext context)
    {
        _context = context;
    }

    // ... implementation
}
```

**Console output at startup:**
```
[Auto-Discovery] Registered: IOtherManualService -> OtherManualService
[Auto-Discovery] Successfully registered 1 custom ManualService(s)
```

**No ServiceHandler needed!** Services are ready for dependency injection.

#### Auto-Discovery Implementation

The `NativeInjectorBootStrapper.RegisterManualServices()` method:
1. Loads all MIT.*.dll assemblies
2. Scans for interfaces ending with "ManualService" and starting with "I"
3. Searches for implementation (removes "I" prefix)
4. Registers with DI container if not already registered
5. Logs registration to console

**Excluded from auto-discovery:**
- `IJsonApiManualService` (framework's own service, registered separately)

### Optional: Manual Service Handlers (Legacy)

For services that don't follow the `*ManualService` pattern, you can still use service handlers:

#### Application Services Registration

```csharp
public class MyModuleAppServiceHandler : IApplicationServiceHandler
{
    public void Configure(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
        services.AddScoped<IProductService, ProductService>();
    }
}
```

**Auto-Discovery**: Framework automatically finds and executes all `IApplicationServiceHandler` implementations.

#### Domain Services Registration

```csharp
public class MyModuleDomainServiceHandler : IDomainServiceHandler
{
    public void Configure(IServiceCollection services)
    {
        services.AddScoped<MyModuleDbContext>();
        services.AddScoped<IMyRepository, MyRepository>();
    }
}
```

**Note**: Service handlers are **optional** and only needed for services not following the `*ManualService` naming convention.

## Testing

### Automated Tests
Test project location: `Tests/MIT.Fwk.Tests.WebApi/`

**JsonAPI Entity Testing**:
- Entities with `[Resource]` attribute are automatically tested
- Framework generates CRUD tests automatically

**Running Tests**:
1. Ensure API is running: `startupWebApi.bat`
2. Run test suite via Test Explorer or CLI
3. Results include list of tested controllers and errors

## Key Technologies

- **.NET 8.0** - Target framework
- **JsonApiDotNetCore 5.6** - JSON:API specification implementation
- **MediatR 12.4** - CQRS mediator pattern
- **AutoMapper 13.0** - Object mapping
- **Entity Framework Core 8.0** - ORM
- **Dapper** - Micro-ORM for performance-critical queries
- **ASP.NET Core Identity** - Authentication/Authorization
- **JWT Bearer** - Token authentication
- **Quartz.NET 3.13** - Job scheduling
- **MongoDB Driver 3.1** - NoSQL for logging
- **Swagger/OpenAPI** - API documentation

## Modern Dependency Injection Services (v8.0+)

### Core Services

Framework provides modern DI-based services to replace legacy static helpers:

**Configuration & Connection Strings**:
```csharp
public class MyService
{
    private readonly IConnectionStringProvider _connStringProvider;
    private readonly IOptions<DatabaseOptions> _dbOptions;

    public MyService(IConnectionStringProvider connStringProvider,
                     IOptions<DatabaseOptions> dbOptions)
    {
        _connStringProvider = connStringProvider;
        _dbOptions = dbOptions;
    }

    public void Connect()
    {
        // Automatic AES decryption if encrypted
        string connStr = _connStringProvider.GetConnectionString("JsonApiConnection");
        int timeout = _dbOptions.Value.QueryTimeout;
    }
}
```

**Logging**:
```csharp
public class MyService
{
    private readonly ILogService _logService;

    public MyService(ILogService logService)
    {
        _logService = logService;
    }

    public void DoWork()
    {
        _logService.Info("Operation started", "MyContext");
        _logService.Error("Failed", exception);
        _logService.ForMongo("Event for MongoDB", eventData);
    }
}
```

**Email Service**:
```csharp
public class MyService
{
    private readonly IEmailService _emailService;
    private readonly IOptions<SmtpOptions> _smtpOptions;

    public MyService(IEmailService emailService, IOptions<SmtpOptions> smtpOptions)
    {
        _emailService = emailService;
        _smtpOptions = smtpOptions;
    }

    public void SendNotification()
    {
        if (!_smtpOptions.Value.Enabled)
            return;

        var results = _emailService.SendMail(
            recipients: "user@example.com",
            body: "Your notification message",
            subject: "Notification"
        );

        foreach (var (recipient, success, errorMessage) in results)
        {
            if (!success)
                _logService.Error($"Failed to send to {recipient}: {errorMessage}");
        }
    }
}
```

**Encryption Service (SHA-256)**:
```csharp
public class MyService
{
    private readonly IEncryptionService _encryptionService;

    public MyService(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public void Encrypt()
    {
        string encrypted = _encryptionService.EncryptString(plainText, key);
        string decrypted = _encryptionService.DecryptString(encrypted, key);

        // Modern SHA-256 (not deprecated SHA-1)
        byte[] signature = _encryptionService.Sign(data, certPath);
        bool valid = _encryptionService.Verify(data, signature, certPath);
    }
}
```

### Typed Configuration Options

Use `IOptions<T>` pattern for configuration:

**SmtpOptions**:
```csharp
public class SmtpOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }  // Encrypted
    public string Password { get; set; }  // Encrypted
    public string Sender { get; set; }
    public bool EnableSSL { get; set; }
}
```

**DatabaseOptions**:
```csharp
public class DatabaseOptions
{
    public string DefaultProvider { get; set; } = "Sql";
    public string NoSQLProvider { get; set; } = "MongoDB";
    public int QueryTimeout { get; set; } = 30;
    public string EncryptionKey { get; set; }
}
```

Register in `appsettings.json`:
```json
{
  "Database": {
    "DefaultProvider": "Sql",
    "QueryTimeout": 30,
    "EncryptionKey": "YOUR_KEY_HERE"
  },
  "Smtp": {
    "Enabled": true,
    "Host": "smtp.example.com",
    "Port": 587,
    "EnableSSL": true
  }
}
```

### Deprecated Patterns (Will be removed in v9.0)

The following static helpers are **deprecated** - use DI services instead:

❌ **ConfigurationHelper** → Use `IConfiguration` / `IOptions<T>` / `IConnectionStringProvider`
❌ **LogHelper** → Use `ILogService`
❌ **MailHelper** → Use `IEmailService`
❌ **EncryptionHelper** → Use `IEncryptionService` (also fixes SHA-1 → SHA-256)
❌ **MapperWrapper** → Use `IMapper` with DI
❌ **BaseEntity** → Use EF Core POCOs or `Identifiable<T>` from JsonAPI
❌ **ValueObject<T>** → Use C# `record` types
❌ **IRepository/IRepositoryV2** → Use `DbContext.Set<T>()` directly
❌ **IAppService/IAppServiceV2** → Use `DbContext` + `IJsonApiManualService`
❌ **BaseController/BaseControllerV2** → Use `ApiController` directly
❌ **DTOFactory** → Use `IMapper` with DI

See `CORE-REFACTORING-V9.md` for complete migration guide.

## Multi-Tenancy

Framework supports tenant isolation via `ITenantProvider`:
```csharp
services.AddScoped<ITenantProvider, TenantProvider>();
```

Entities implementing `IHasTenant` automatically filter by tenant context. Multi-tenancy is applied via EF Core query filters in `OnModelCreating()`.

## Authentication & Authorization

### JWT Authentication
- Default scheme: `JwtBearerDefaults.AuthenticationScheme`
- Token validation: `JwtTokenProvider.GetValidationParameters()`
- Basic Auth also supported

### Authorization Levels
Pre-defined role-level policies (0-100 scale):
- `NeededRoleLevel0` - SuperAdmin only
- `NeededRoleLevel10` - Admin
- `NeededRoleLevel50` - Standard user
- Up to `NeededRoleLevel100`

### Usage in Controllers
```csharp
[HttpDelete("{id}")]
[Authorize(Policy = "NeededRoleLevel10")]
public async Task<IActionResult> Delete(int id) { }
```

## Modern Controller Pattern (v8.0+)

All controllers extend `ApiController` directly. No base controller wrappers.

### Basic CRUD Controller with DbContext

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Controllers;
using MIT.Fwk.Infrastructure.Identity.Data;

[Authorize]
[Route("api/[controller]")]
public class ProductsController : ApiController
{
    private readonly JsonApiDbContext _context;
    private readonly IMapper _mapper;

    public ProductsController(
        JsonApiDbContext context,
        IMapper mapper,
        INotificationHandler<DomainNotification> notifications) : base(notifications)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: Read-only query with AsNoTracking for performance
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Products
            .AsNoTracking()
            .ToListAsync();
        var dtos = _mapper.Map<List<ProductDTO>>(entities);
        return Response(dtos);
    }

    // GET: Single entity
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var entity = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
            return NotFound();

        var dto = _mapper.Map<ProductDTO>(entity);
        return Response(dto);
    }

    // POST: Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductDTO dto)
    {
        if (!ModelState.IsValid)
        {
            NotifyModelStateErrors();
            return Response(dto);
        }

        var entity = _mapper.Map<Product>(dto);
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<ProductDTO>(entity);
        return Response(resultDto);
    }

    // PUT: Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDTO dto)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null)
            return NotFound();

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<ProductDTO>(entity);
        return Response(resultDto);
    }

    // DELETE: Soft or hard delete
    [HttpDelete("{id}")]
    [Authorize(Policy = "NeededRoleLevel10")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity == null)
            return NotFound();

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();

        return Response(true);
    }
}
```

### Controller with Complex Queries (IJsonApiManualService)

For complex queries involving joins, aggregations, or cross-entity operations:

```csharp
[Authorize]
[Route("api/reports")]
public class ReportsController : ApiController
{
    private readonly IJsonApiManualService _jsonApiService;

    public ReportsController(
        IJsonApiManualService jsonApiService,
        INotificationHandler<DomainNotification> notifications) : base(notifications)
    {
        _jsonApiService = jsonApiService;
    }

    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        // Complex query with joins
        var orders = _jsonApiService.GetAllQueryable<Order, int>()
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToList();

        var summary = orders.GroupBy(o => o.Customer.Name)
            .Select(g => new {
                Customer = g.Key,
                TotalOrders = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount)
            });

        return Response(summary);
    }
}
```

### Authentication Controller Pattern

For controllers requiring user context:

```csharp
[Authorize]
[Route("api/user")]
public class UserProfileController : ApiController
{
    private readonly UserManager<MITApplicationUser> _userManager;
    private readonly IJsonApiManualService _jsonApiService;

    public UserProfileController(
        UserManager<MITApplicationUser> userManager,
        IJsonApiManualService jsonApiService,
        INotificationHandler<DomainNotification> notifications) : base(notifications)
    {
        _userManager = userManager;
        _jsonApiService = jsonApiService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Get current user from JWT claims
        string username = Request.HttpContext.User.Claims
            .FirstOrDefault(x => x.Type == "username")?.Value;

        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var user = await _userManager.FindByEmailAsync(username)
                ?? await _userManager.FindByNameAsync(username);

        if (user == null)
            return NotFound();

        var profile = await _jsonApiService.GetUserProfileById(user.Id);
        return Response(profile);
    }
}
```

## EF Core Best Practices

### 1. Use AsNoTracking for Read-Only Queries
```csharp
// ✅ Good - no change tracking overhead
var products = await _context.Products
    .AsNoTracking()
    .ToListAsync();

// ❌ Bad - tracks entities unnecessarily for read-only
var products = await _context.Products.ToListAsync();
```

### 2. Prefer Async Methods
```csharp
// ✅ Good
await _context.SaveChangesAsync();
var entity = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

// ❌ Bad
_context.SaveChanges();
var entity = _context.Products.FirstOrDefault(p => p.Id == id);
```

### 3. Use Find() for Primary Key Lookups
```csharp
// ✅ Good - optimized for PK lookups
var product = await _context.Products.FindAsync(id);

// ❌ Less efficient for PK lookups
var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
```

### 4. Avoid N+1 Queries with Include
```csharp
// ✅ Good - single query with join
var orders = await _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
    .AsNoTracking()
    .ToListAsync();

// ❌ Bad - N+1 queries
var orders = await _context.Orders.ToListAsync();
// Each order.Customer access triggers a separate query
```

### 5. Use Projections to Reduce Data Transfer
```csharp
// ✅ Good - only select needed fields
var summaries = await _context.Orders
    .Select(o => new OrderSummaryDTO {
        OrderId = o.Id,
        CustomerName = o.Customer.Name,
        Total = o.TotalAmount
    })
    .AsNoTracking()
    .ToListAsync();

// ❌ Bad - loads all entity data then maps
var orders = await _context.Orders.Include(o => o.Customer).ToListAsync();
var summaries = _mapper.Map<List<OrderSummaryDTO>>(orders);
```

## Event Sourcing

All domain events stored in SQL via `IEventStore`:
- `EventStoreSQLRepository` - Persistence
- `SqlEventStore` - Event store implementation
- Command handlers use `IMediatorHandler` to dispatch events

## Scheduler System

Quartz.NET jobs in `MIT.Fwk.Scheduler`:
- `EventStoreRetentionManager` - Cleanup old events
- `MongoLogBusManager` - Log aggregation to MongoDB
- `SyncManager` - Data synchronization

Enable via: `"EnableJobs": true` in customsettings.json

## Important Conventions

1. **Module Structure**: Create modules in `Src/MIT.Fwk.Examples/` or similar
2. **DbContext Keys**: Class name = dbconnections.json key (exact match required)
3. **Interface Implementation**: `IJsonApiDbContext` required for auto-discovery
4. **Project References**: Add custom modules to `MIT.Fwk.WebApi.csproj`
5. **Build Order**: Framework handles build order automatically via project references
6. **Stop Before Compile**: API must be stopped before rebuilding
7. **Virtual Navigation**: Use `virtual` for EF navigation properties (lazy loading)

## Migration from Plugin-Based Architecture

If migrating from the old plugin-based architecture:

1. **Remove External Plugins**: Move code from `C:\MaeFWK\MIT.Customs\` into solution
2. **Create Module Projects**: Add projects to `Src/MIT.Fwk.Examples/` or similar
3. **Update References**: Replace dynamic loading with project references
4. **Remove Plugin Handlers**: `IAppPartManager` and `IPolicyManager` no longer needed
5. **Update Configuration**: Remove `PluginsPath` from customsettings.json
6. **Rebuild**: Clean solution and rebuild

## Troubleshooting

**Module Not Discovered**:
- Verify project reference in `MIT.Fwk.WebApi.csproj`
- Check that DbContext implements `IJsonApiDbContext`
- Ensure assembly is loaded (added to `AddJsonApi` discovery)

**DbContext Not Found**:
- Verify `IJsonApiDbContext` implementation
- Check key in dbconnections.json matches class name exactly
- Ensure `_UseSqlServer` flag matches configuration

**Build Failures**:
- Stop running API (files are locked)
- Clean solution: `dotnet clean`
- Rebuild: `dotnet build`

**License Errors** (Production only):
- Generate key: `MIT.Fwk.WebApi.exe -key Mae2019!`
- Activate: `MIT.Fwk.WebApi.exe -lic <key> -v <months>`
- Disabled in DEBUG and DOCKER builds

## Further Documentation

See `CUSTOMIZATION_GUIDE.md` for detailed step-by-step customization instructions and examples.
