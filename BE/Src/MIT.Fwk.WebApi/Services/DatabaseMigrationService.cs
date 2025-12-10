using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Service for automatically applying Entity Framework migrations for all DbContexts
    /// that implement IMigrationDbContext at application startup.
    ///
    /// DbContexts must implement IMigrationDbContext to be included in automatic migrations.
    /// This allows generated/external DbContexts to opt-out of migrations by implementing
    /// only IJsonApiDbContext (for JsonAPI discovery) without IMigrationDbContext.
    /// </summary>
    public class DatabaseMigrationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseMigrationService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseMigrationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Discovers and applies pending migrations for all DbContexts that implement IMigrationDbContext.
        /// </summary>
        /// <returns>True if all migrations applied successfully, false if any failed</returns>
        public async Task<bool> ApplyMigrationsAsync()
        {
            try
            {
                _logger.LogInformation("=================================================");
                _logger.LogInformation("Starting automatic database migrations...");
                _logger.LogInformation("=================================================");

                // Discover all DbContext types that implement IMigrationDbContext
                List<Type> dbContextTypes = DiscoverDbContextTypes();

                if (!dbContextTypes.Any())
                {
                    _logger.LogWarning("No DbContext types implementing IMigrationDbContext found.");
                    return true;
                }

                _logger.LogInformation($"Found {dbContextTypes.Count} DbContext(s) implementing IMigrationDbContext:");
                foreach (Type type in dbContextTypes)
                {
                    _logger.LogInformation($"  - {type.Name}");
                }
                _logger.LogInformation("");

                // Read migration order from configuration
                List<Type> orderedContextTypes = OrderDbContextsByConfiguration(dbContextTypes);

                bool allSucceeded = true;

                // Apply migrations for each DbContext in order
                foreach (Type contextType in orderedContextTypes)
                {
                    bool success = await ApplyMigrationsForContextTypeAsync(contextType);
                    if (!success)
                    {
                        allSucceeded = false;
                        _logger.LogWarning($"Migrations failed for {contextType.Name}, but continuing with other contexts...");
                    }
                }

                _logger.LogInformation("=================================================");
                if (allSucceeded)
                {
                    _logger.LogInformation("All database migrations completed successfully");
                }
                else
                {
                    _logger.LogWarning("Database migrations completed with some errors");
                }
                _logger.LogInformation("=================================================");

                return allSucceeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during database migration process.");
                return false;
            }
        }

        /// <summary>
        /// Orders DbContext types based on the DatabaseMigrationOrder configuration.
        /// Throws an error if the configuration is missing or if discovered DbContexts are not in the configuration.
        /// </summary>
        private List<Type> OrderDbContextsByConfiguration(List<Type> dbContextTypes)
        {
            try
            {
                // Read DatabaseMigrationOrder from configuration
                List<string> migrationOrder = _configuration.GetSection("DatabaseMigrationOrder").Get<List<string>>();

                // Validate configuration exists
                if (migrationOrder == null || !migrationOrder.Any())
                {
                    string errorMessage = "ERROR: 'DatabaseMigrationOrder' parameter not found in customsettings.json or is empty!";
                    _logger.LogError(errorMessage);
                    _logger.LogError("Please add the following configuration to customsettings.json:");
                    _logger.LogError("  \"DatabaseMigrationOrder\": [");
                    _logger.LogError("    \"JsonApiDbContext\",");
                    _logger.LogError("    \"OtherDbContext\"");
                    _logger.LogError("  ]");

                    Console.WriteLine("=================================================");
                    Console.WriteLine("ERROR: Missing DatabaseMigrationOrder configuration!");
                    Console.WriteLine("=================================================");
                    Console.WriteLine("Please add 'DatabaseMigrationOrder' to customsettings.json");
                    Console.WriteLine("Example:");
                    Console.WriteLine("  \"DatabaseMigrationOrder\": [");
                    Console.WriteLine("    \"JsonApiDbContext\",");
                    Console.WriteLine("    \"OtherDbContext\"");
                    Console.WriteLine("  ]");
                    Console.WriteLine("=================================================");

                    // Return unordered list (fallback behavior)
                    return dbContextTypes;
                }

                _logger.LogInformation("=================================================");
                _logger.LogInformation("Migration order from configuration:");
                for (int i = 0; i < migrationOrder.Count; i++)
                {
                    _logger.LogInformation($"  {i + 1}. {migrationOrder[i]}");
                }
                _logger.LogInformation("=================================================");

                // Validate that all discovered DbContexts are in the configuration
                List<string> discoveredContextNames = dbContextTypes.Select(t => t.Name).ToList();
                List<string> missingInConfig = discoveredContextNames.Except(migrationOrder).ToList();
                List<string> extraInConfig = migrationOrder.Except(discoveredContextNames).ToList();

                if (missingInConfig.Any())
                {
                    string errorMessage = $"ERROR: The following DbContexts are discovered but NOT in DatabaseMigrationOrder configuration: {string.Join(", ", missingInConfig)}";
                    _logger.LogError(errorMessage);

                    Console.WriteLine("=================================================");
                    Console.WriteLine("ERROR: DbContext(s) missing from configuration!");
                    Console.WriteLine("=================================================");
                    Console.WriteLine($"Discovered DbContexts NOT in configuration: {string.Join(", ", missingInConfig)}");
                    Console.WriteLine("Please add them to 'DatabaseMigrationOrder' in customsettings.json");
                    Console.WriteLine("=================================================");
                }

                if (extraInConfig.Any())
                {
                    _logger.LogWarning($"WARNING: The following DbContexts are in configuration but NOT discovered: {string.Join(", ", extraInConfig)}");
                }

                // Order DbContexts according to configuration
                List<Type> orderedTypes = new List<Type>();

                foreach (string contextName in migrationOrder)
                {
                    Type contextType = dbContextTypes.FirstOrDefault(t => t.Name == contextName);
                    if (contextType != null)
                    {
                        orderedTypes.Add(contextType);
                    }
                }

                // Add any DbContexts that are discovered but not in configuration at the end
                foreach (Type contextType in dbContextTypes)
                {
                    if (!orderedTypes.Contains(contextType))
                    {
                        orderedTypes.Add(contextType);
                        _logger.LogWarning($"Adding {contextType.Name} at the end (not in configuration order)");
                    }
                }

                _logger.LogInformation("Final migration order:");
                for (int i = 0; i < orderedTypes.Count; i++)
                {
                    _logger.LogInformation($"  {i + 1}. {orderedTypes[i].Name}");
                }
                _logger.LogInformation("");

                return orderedTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading DatabaseMigrationOrder configuration. Using default order.");
                return dbContextTypes;
            }
        }

        /// <summary>
        /// Discovers all DbContext types that implement IMigrationDbContext from loaded assemblies.
        /// Only DbContexts with IMigrationDbContext will have migrations applied.
        /// </summary>
        private List<Type> DiscoverDbContextTypes()
        {
            List<Type> dbContextTypes = [];

            try
            {
                // Get all loaded assemblies with "MIT" in the name
                List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && a.FullName.Contains("MIT"))
                    .ToList();

                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        List<Type> types = assembly.GetTypes()
                            .Where(t => t.IsClass && !t.IsAbstract
                                && typeof(DbContext).IsAssignableFrom(t)
                                && typeof(IMigrationDbContext).IsAssignableFrom(t))
                            .ToList();

                        dbContextTypes.AddRange(types);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        _logger.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering DbContext types.");
            }

            return dbContextTypes;
        }

        /// <summary>
        /// Applies migrations for a specific DbContext type.
        /// </summary>
        private async Task<bool> ApplyMigrationsForContextTypeAsync(Type contextType)
        {
            try
            {
                _logger.LogInformation("=================================================");
                _logger.LogInformation($"Applying {contextType.Name} migrations...");
                _logger.LogInformation("=================================================");

                await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                DbContext context = scope.ServiceProvider.GetService(contextType) as DbContext;

                if (context == null)
                {
                    _logger.LogWarning($"{contextType.Name} not registered in DI. Skipping migrations.");
                    _logger.LogInformation("");
                    return true;
                }

                bool result = await ApplyMigrationsForContextAsync(context, contextType.Name);

                _logger.LogInformation("=================================================");
                _logger.LogInformation($"{contextType.Name} migrations completed {(result ? "successfully" : "with errors")}");
                _logger.LogInformation("=================================================");
                _logger.LogInformation("");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying migrations for {contextType.Name}.");
                _logger.LogInformation("");
                return false;
            }
        }

        /// <summary>
        /// Core method that applies migrations to a specific DbContext instance.
        /// Ensures the database exists before applying migrations.
        /// </summary>
        private async Task<bool> ApplyMigrationsForContextAsync(DbContext context, string contextName)
        {
            try
            {
                // Step 1: Ensure database exists
                _logger.LogInformation($"Checking if database exists for {contextName}...");
                bool databaseExists = await EnsureDatabaseExistsAsync(context, contextName);

                if (!databaseExists)
                {
                    _logger.LogError($"Failed to ensure database exists for {contextName}.");
                    return false;
                }

                // Step 2: Check and apply migrations
                _logger.LogInformation($"Checking migrations for {contextName}...");

                // Get pending migrations
                IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                List<string> pendingList = pendingMigrations.ToList();

                if (pendingList.Any())
                {
                    _logger.LogInformation($"Found {pendingList.Count} pending migration(s) for {contextName}:");
                    foreach (string migration in pendingList)
                    {
                        _logger.LogInformation($"  - {migration}");
                    }

                    _logger.LogInformation($"Applying migrations for {contextName}...");
                    await context.Database.MigrateAsync();
                    _logger.LogInformation($"Successfully applied migrations for {contextName}.");
                }
                else
                {
                    _logger.LogInformation($"No pending migrations for {contextName}.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to apply migrations for {contextName}.");
                return false;
            }
        }

        /// <summary>
        /// Ensures the database exists. If it doesn't, creates it.
        /// </summary>
        /// <returns>True if the database exists or was created successfully, false otherwise</returns>
        private async Task<bool> EnsureDatabaseExistsAsync(DbContext context, string contextName)
        {
            try
            {
                // First check if we can connect to the database
                bool canConnect = await context.Database.CanConnectAsync();

                if (canConnect)
                {
                    _logger.LogInformation($"Database for {contextName} already exists.");
                    return true;
                }

                // Database doesn't exist - create it
                _logger.LogInformation($"Database for {contextName} does not exist. Creating database...");

                // Try to get the relational database creator
                IRelationalDatabaseCreator databaseCreator = context.Database.GetService<IDatabaseCreator>() as IRelationalDatabaseCreator;

                if (databaseCreator != null)
                {
                    // Use EF Core's built-in database creator
                    // This creates the database structure without applying migrations
                    await databaseCreator.CreateAsync();
                    _logger.LogInformation($"Database for {contextName} created successfully.");
                    return true;
                }
                else
                {
                    // Fallback: try to create using SQL
                    _logger.LogInformation($"Using fallback method to create database for {contextName}...");
                    return await CreateDatabaseUsingRawSqlAsync(context, contextName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring database exists for {contextName}.");
                return false;
            }
        }

        /// <summary>
        /// Fallback method to create database using raw SQL.
        /// Extracts database name from connection string and creates it on the master database.
        /// </summary>
        private async Task<bool> CreateDatabaseUsingRawSqlAsync(DbContext context, string contextName)
        {
            try
            {
                // Get connection string from the context
                string connectionString = context.Database.GetConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError($"Could not retrieve connection string for {contextName}.");
                    return false;
                }

                // Extract database name from connection string
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                string databaseName = null;
                if (builder.ContainsKey("Database"))
                {
                    databaseName = builder["Database"].ToString();
                }
                else if (builder.ContainsKey("Initial Catalog"))
                {
                    databaseName = builder["Initial Catalog"].ToString();
                }

                if (string.IsNullOrEmpty(databaseName))
                {
                    _logger.LogError($"Could not extract database name from connection string for {contextName}.");
                    return false;
                }

                _logger.LogInformation($"Creating database '{databaseName}'...");

                // Build connection string to master database
                builder["Database"] = "master";
                builder["Initial Catalog"] = "master";
                string masterConnectionString = builder.ConnectionString;

                // Create database on master
                using (DbConnection connection = context.Database.GetDbConnection().GetType().GetConstructor(new[] { typeof(string) })
                    ?.Invoke(new object[] { masterConnectionString }) as DbConnection)
                {
                    if (connection == null)
                    {
                        _logger.LogError($"Could not create connection to master database for {contextName}.");
                        return false;
                    }

                    await connection.OpenAsync();

                    using DbCommand command = connection.CreateCommand();

                    // Check if database exists
                    command.CommandText = $"SELECT COUNT(*) FROM sys.databases WHERE name = N'{databaseName}'";
                    object result = await command.ExecuteScalarAsync();
                    int count = Convert.ToInt32(result);

                    if (count == 0)
                    {
                        // Create database
                        command.CommandText = $"CREATE DATABASE [{databaseName}]";
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Database '{databaseName}' created successfully.");
                    }
                    else
                    {
                        _logger.LogInformation($"Database '{databaseName}' already exists.");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating database using raw SQL for {contextName}.");
                return false;
            }
        }

        /// <summary>
        /// Gets migration status for all DbContexts that implement IMigrationDbContext (useful for diagnostics).
        /// </summary>
        public async Task<Dictionary<string, MigrationStatus>> GetMigrationStatusAsync()
        {
            Dictionary<string, MigrationStatus> statusDict = [];

            try
            {
                List<Type> dbContextTypes = DiscoverDbContextTypes();

                foreach (Type contextType in dbContextTypes)
                {
                    try
                    {
                        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
                        DbContext context = scope.ServiceProvider.GetService(contextType) as DbContext;

                        if (context == null)
                        {
                            statusDict[contextType.Name] = new MigrationStatus
                            {
                                ContextName = contextType.Name,
                                Error = $"{contextType.Name} not registered in DI"
                            };
                            continue;
                        }

                        IEnumerable<string> appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                        statusDict[contextType.Name] = new MigrationStatus
                        {
                            ContextName = contextType.Name,
                            AppliedMigrations = appliedMigrations.ToList(),
                            PendingMigrations = pendingMigrations.ToList(),
                            DatabaseExists = await context.Database.CanConnectAsync()
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting migration status for {contextType.Name}.");
                        statusDict[contextType.Name] = new MigrationStatus
                        {
                            ContextName = contextType.Name,
                            Error = ex.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status.");
            }

            return statusDict;
        }
    }

    /// <summary>
    /// Represents the migration status for a DbContext.
    /// </summary>
    public class MigrationStatus
    {
        public string ContextName { get; set; }
        public List<string> AppliedMigrations { get; set; } = [];
        public List<string> PendingMigrations { get; set; } = [];
        public bool DatabaseExists { get; set; }
        public string Error { get; set; }

        public bool HasPendingMigrations => PendingMigrations?.Any() ?? false;
        public bool IsHealthy => DatabaseExists && !HasPendingMigrations && string.IsNullOrEmpty(Error);
    }
}
