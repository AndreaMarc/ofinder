using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring Entity Framework Core and database contexts.
    /// Handles auto-discovery of IJsonApiDbContext implementations.
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Configures all database contexts for the application.
        /// Auto-discovers all DbContexts implementing IJsonApiDbContext from loaded assemblies.
        /// </summary>
        public static IServiceCollection AddFrameworkDatabases(
            this IServiceCollection services,
            IConfiguration configuration,
            out List<Type> dbContextTypes)
        {
            // Add tenant provider and HTTP context accessor
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddHttpContextAccessor();

            // Configure main JsonApiDbContext provider (SQL or MySQL)
            ConfigureJsonApiDbContextProvider(configuration);

            // Register JsonApiDbContext
            RegisterJsonApiDbContext(services, configuration);

            // Register MITApplicationDbContext (Identity-only, no migrations)
            RegisterMITApplicationDbContext(services, configuration);

            // Auto-discover and register all custom DbContexts (e.g., OtherDbContext from MIT.Fwk.Examples)
            dbContextTypes = DiscoverAndRegisterDbContexts(services, configuration);

            RegisterDbContextRepositories(services, dbContextTypes);

            // Register DatabaseMigrationService (needed for automatic migrations at startup)
            services.AddScoped<DatabaseMigrationService>();

            return services;
        }

        /// <summary>
        /// Configures the SQL provider for JsonApiDbContext (SQL Server or MySQL).
        /// </summary>
        private static void ConfigureJsonApiDbContextProvider(IConfiguration configuration)
        {
            if (configuration["JsonApiDbContext"] == "MySql")
            {
                JsonApiDbContext._UseSqlServer = false;
            }
        }

        /// <summary>
        /// Registers the main JsonApiDbContext with the DI container.
        /// </summary>
        private static void RegisterJsonApiDbContext(
            IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("JsonApiDbContext")
                                      ?? throw new InvalidOperationException("JsonApiDbContext connection string not found");

            if (JsonApiDbContext._UseSqlServer)
            {
                services.AddDbContext<JsonApiDbContext>(options =>
                        options.UseSqlServer(connectionString),
                    ServiceLifetime.Scoped);
            }
            else
            {
                services.AddDbContext<JsonApiDbContext>(options =>
                        options.UseMySQL(connectionString),
                    ServiceLifetime.Scoped);
            }
        }

        /// <summary>
        /// Dinamically registers repositories for all DbContexts.
        /// </summary>
        private static void RegisterDbContextRepositories(
            IServiceCollection services,
            List<Type> dbContextTypes)
        {
            var repositories = ReflectionHelper.GetAllTypes<IRepositorySupportsTransaction>() ?? new List<Type>();

            foreach (var dbContext in dbContextTypes)
            {
                var dbContextType = dbContext;

                if (!dbContextType.IsSubclassOf(typeof(JsonApiDbContext)) || dbContextType.IsInstanceOfType(typeof(JsonApiDbContext)))
                {
                    var repository = repositories.FirstOrDefault(rep =>
                        (rep.Name + "").StartsWith(dbContextType.Name + "Repository"));

                    var entityTypes = dbContextType.GetProperties()
                        .Where(p => p.PropertyType.IsGenericType &&
                                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                        .Select(p => p.PropertyType.GetGenericArguments()[0]);

                    // Escludi ogni entità dalle migrations
                    foreach (var entityType in entityTypes)
                    {
                        var count = entityType.BaseType.GetGenericArguments().Length;
                        
                        if (count == 0)
                            continue;

                        var repositoryGenericType = repository.MakeGenericType(entityType, entityType.BaseType.GetGenericArguments()[0]);

                        var method = typeof(JsonApiDotNetCore.Configuration.ServiceCollectionExtensions)
                            .GetMethod("AddResourceRepository", BindingFlags.Public | BindingFlags.Static);

                        var generic = method.MakeGenericMethod(repositoryGenericType);

                        generic.Invoke(null, new object[] { services });
                    }
                }
            }
        }

        /// <summary>
        /// Registers MITApplicationDbContext for ASP.NET Identity.
        /// Uses same connection string as JsonApiDbContext.
        /// Does NOT participate in migrations (tables already exist).
        /// </summary>
        private static void RegisterMITApplicationDbContext(
            IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("JsonApiDbContext")
                                      ?? configuration.GetConnectionString("DefaultConnection")
                                      ?? throw new InvalidOperationException("Connection string not found for MITApplicationDbContext");

            // Configure provider flag from JsonApiDbContext setting
            if (configuration["JsonApiDbContext"] == "MySql")
            {
                MITApplicationDbContext._UseSqlServer = false;
            }

            if (MITApplicationDbContext._UseSqlServer)
            {
                services.AddDbContext<MITApplicationDbContext>(options =>
                        options.UseSqlServer(connectionString),
                    ServiceLifetime.Scoped);
            }
            else
            {
                services.AddDbContext<MITApplicationDbContext>(options =>
                        options.UseMySQL(connectionString),
                    ServiceLifetime.Scoped);
            }

            Console.WriteLine($"[MITApplicationDbContext] Registered (Provider: {(MITApplicationDbContext._UseSqlServer ? "SQL Server" : "MySQL")}, no migrations)");
        }

        /// <summary>
        /// Discovers all DbContexts implementing IJsonApiDbContext and registers them.
        /// Returns the list of discovered DbContext types for JsonAPI configuration.
        /// </summary>
        private static List<Type> DiscoverAndRegisterDbContexts(
            IServiceCollection services,
            IConfiguration configuration)
        {
            // Auto-discover all DbContexts implementing IJsonApiDbContext from loaded assemblies
            List<object> dbContextInstances = ReflectionHelper.ResolveAll<IJsonApiDbContext>() ?? new List<object>();
            List<Type> dbContextTypes = new();

            foreach (object dbContextInstance in dbContextInstances)
            {
                Type dbContextType = dbContextInstance.GetType();
                dbContextTypes.Add(dbContextType);

                // Skip JsonApiDbContext (already registered)
                if (dbContextType == typeof(JsonApiDbContext))
                    continue;

                // Configure provider (SQL or MySQL) from configuration
                bool isSqlServer = configuration[dbContextType.Name] == "Sql";
                string dbConnectionString = configuration.GetConnectionString(dbContextType.Name)
                                            ?? throw new InvalidOperationException($"Connection string for {dbContextType.Name} not found");

                // Set static _UseSqlServer field if present
                var staticField = dbContextType.GetField("_UseSqlServer", BindingFlags.Public | BindingFlags.Static);
                staticField?.SetValue(null, isSqlServer);

                // Register DbContext with DI using reflection
                RegisterDbContextDynamically(services, dbContextType, dbConnectionString, isSqlServer);
            }

            return dbContextTypes;
        }

        /// <summary>
        /// Dynamically registers a DbContext type with the DI container.
        /// </summary>
        private static void RegisterDbContextDynamically(
            IServiceCollection services,
            Type dbContextType,
            string connectionString,
            bool isSqlServer)
        {
            var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
                .GetMethod("AddDbContext", 1,
                    new[] {
                        typeof(IServiceCollection),
                        typeof(Action<DbContextOptionsBuilder>),
                        typeof(ServiceLifetime),
                        typeof(ServiceLifetime)
                    });

            if (addDbContextMethod == null)
                throw new InvalidOperationException("AddDbContext method not found via reflection");

            MethodInfo genericMethod = addDbContextMethod.MakeGenericMethod(dbContextType);

            Action<DbContextOptionsBuilder> optionsAction = isSqlServer
                ? options => options.UseSqlServer(connectionString)
                : options => options.UseMySQL(connectionString);

            genericMethod.Invoke(null, new object[]
            {
                services,
                optionsAction,
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped
            });
        }

    }
}
