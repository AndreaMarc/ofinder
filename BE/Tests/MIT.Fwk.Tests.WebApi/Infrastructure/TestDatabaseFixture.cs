using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Helpers;
using System.Reflection;

namespace MIT.Fwk.Tests.WebApi.Infrastructure
{
    /// <summary>
    /// Test fixture for database initialization and migrations.
    /// Implements IDisposable to clean up resources after all tests complete.
    /// Shared across all test classes via IClassFixture&lt;TestDatabaseFixture&gt;.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; }

        private bool _migrationsApplied = false;
        private readonly object _migrationLock = new();

        public TestDatabaseFixture()
        {
            Configuration = TestServiceProvider.GetConfigurationInstance();
            ServiceProvider = TestServiceProvider.GetServiceProvider();

            // Apply migrations once at startup if enabled
            ApplyMigrationsIfEnabled();
        }

        /// <summary>
        /// Applies database migrations if EnableAutoMigrations is true in configuration.
        /// Uses DatabaseMigrationOrder from customsettings.json to determine execution order.
        /// </summary>
        private void ApplyMigrationsIfEnabled()
        {
            if (_migrationsApplied) return;

            lock (_migrationLock)
            {
                if (_migrationsApplied) return;

                bool enableAutoMigrations = Configuration.GetValue<bool>("EnableAutoMigrations", false);
                if (!enableAutoMigrations)
                {
                    Console.WriteLine("[TestFixture] Auto-migrations disabled. Skipping database migration.");
                    _migrationsApplied = true;
                    return;
                }

                Console.WriteLine("[TestFixture] Auto-migrations enabled. Applying database migrations...");

                // Get migration order from configuration
                var migrationOrder = Configuration.GetSection("DatabaseMigrationOrder").Get<List<string>>();
                if (migrationOrder == null || migrationOrder.Count == 0)
                {
                    throw new InvalidOperationException(
                        "DatabaseMigrationOrder is required when EnableAutoMigrations is true. " +
                        "Please add 'DatabaseMigrationOrder' array to customsettings.json.");
                }

                using var scope = ServiceProvider.CreateScope();

                foreach (var contextName in migrationOrder)
                {
                    ApplyMigration(scope, contextName);
                }

                Console.WriteLine("[TestFixture] All migrations applied successfully.");
                _migrationsApplied = true;
            }
        }

        /// <summary>
        /// Applies migration for a specific DbContext by name.
        /// </summary>
        private void ApplyMigration(IServiceScope scope, string contextName)
        {
            try
            {
                // Handle JsonApiDbContext (main framework context)
                if (contextName == "JsonApiDbContext")
                {
                    var jsonApiContext = scope.ServiceProvider.GetRequiredService<JsonApiDbContext>();
                    Console.WriteLine($"[TestFixture] Applying migrations for {contextName}...");
                    jsonApiContext.Database.Migrate();
                    Console.WriteLine($"[TestFixture] ✓ {contextName} migrations applied.");
                    return;
                }

                // Handle custom DbContexts implementing IJsonApiDbContext
                // Use reflection to find and resolve the DbContext type
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName?.StartsWith("MIT.") == true);

                foreach (var assembly in assemblies)
                {
                    var contextType = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == contextName &&
                                           t.IsClass &&
                                           !t.IsAbstract &&
                                           typeof(DbContext).IsAssignableFrom(t));

                    if (contextType != null)
                    {
                        var context = scope.ServiceProvider.GetRequiredService(contextType) as DbContext;
                        if (context != null)
                        {
                            Console.WriteLine($"[TestFixture] Applying migrations for {contextName}...");
                            context.Database.Migrate();
                            Console.WriteLine($"[TestFixture] ✓ {contextName} migrations applied.");
                            return;
                        }
                    }
                }

                Console.WriteLine($"[TestFixture] ⚠ Warning: DbContext '{contextName}' not found. Skipping migration.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestFixture] ✗ Error applying migrations for {contextName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cleanup method called after all tests complete.
        /// </summary>
        public void Dispose()
        {
            // Optional: Add any cleanup logic here
            // For example, dropping test databases or cleaning up resources
            Console.WriteLine("[TestFixture] Test fixture disposed.");
        }
    }
}
