using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Infrastructure.IoC;
using MIT.Fwk.WebApi.Configurations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Options;
using MIT.Fwk.WebApi.Services;
using AutoMapper;
using MIT.Fwk.Infrastructure.AutoMapper;

namespace MIT.Fwk.Tests.WebApi.Infrastructure
{
    /// <summary>
    /// Provides Dependency Injection container for integration tests.
    /// Mimics Program.cs ConfigureServices without running the full web application.
    /// </summary>
    public class TestServiceProvider
    {
        private static IServiceProvider? _serviceProvider;
        private static readonly object _lock = new();
        private static IConfiguration? _configuration;

        /// <summary>
        /// Gets or creates the singleton service provider for tests.
        /// Thread-safe initialization.
        /// </summary>
        public static IServiceProvider GetServiceProvider()
        {
            if (_serviceProvider != null) return _serviceProvider;

            lock (_lock)
            {
                if (_serviceProvider != null) return _serviceProvider;

                _configuration = GetConfiguration();
                var services = new ServiceCollection();

                ConfigureTestServices(services, _configuration);

                _serviceProvider = services.BuildServiceProvider();

                // Initialize JwtTokenProvider (required by JWT token generation in tests)
                InitializeJwtTokenProvider(_serviceProvider);

                return _serviceProvider;
            }
        }

        /// <summary>
        /// Gets the configuration for tests.
        /// </summary>
        private static IConfiguration GetConfiguration()
        {
            string webApiLocation = Path.GetDirectoryName(Assembly.Load("MIT.Fwk.WebApi").Location)
                                   ?? throw new InvalidOperationException("Cannot find MIT.Fwk.WebApi assembly location");

            var builder = new ConfigurationBuilder()
                .SetBasePath(webApiLocation)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        /// <summary>
        /// Configures services for test environment.
        /// Mirrors Program.cs ConfigureServices but without web-specific setup.
        /// </summary>
        private static void ConfigureTestServices(IServiceCollection services, IConfiguration configuration)
        {
            // 0. Add Logging (required by AutoMapper and other services)
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning); // Reduce noise in tests
            });

            // 0.5. Register IConfiguration (required by AuditableSignInManager and other services)
            services.AddSingleton<IConfiguration>(configuration);

            // 0.7. Add HttpContextAccessor (MUST be before Identity/SignInManager registration)
            // SignInManager requires IHttpContextAccessor in constructor
            services.AddHttpContextAccessor();

            // 0.8. Add MediatR (required by InMemoryBus -> IMediatorHandler)
            // InMemoryBus is registered in NativeInjectorBootStrapper.RegisterServices (step 2)
            services.AddMediatR(cfg =>
            {
                // Register handlers from Infrastructure (includes InMemoryBus, DomainNotificationHandler)
                cfg.RegisterServicesFromAssembly(Assembly.Load("MIT.Fwk.Infrastructure"));
                // Register handlers from WebApi (includes controller-specific handlers if any)
                cfg.RegisterServicesFromAssembly(Assembly.Load("MIT.Fwk.WebApi"));
            });

            // 1. Database configuration (EF Core + auto-discovery)
            List<Type> dbContextTypes;
            services.AddFrameworkDatabases(configuration, out dbContextTypes);

            // 1.5. Register MITApplicationDbContext (Identity-only, same as WebApi)
            RegisterMITApplicationDbContextForTests(services, configuration);

            // 2. Register core framework services (IoC)
            // This includes: Encryption, Email, Log, ConnectionString providers, MediatR, etc.
            NativeInjectorBootStrapper.RegisterServices(services, configuration);

            // 3. Framework application services (IJsonApiManualService, INotificationService, IGoogleService, etc.)
            services.AddFrameworkServices(configuration);

            // 4. Identity (UserManager, SignInManager, RoleManager)
            // Create temporary JsonApiDbContext for identity setup
            using var scope = services.BuildServiceProvider().CreateScope();
            var jsonDbContext = scope.ServiceProvider.GetRequiredService<JsonApiDbContext>();
            services.AddFrameworkIdentity(configuration, jsonDbContext);

            // 5. JsonAPI configuration (AFTER Identity to avoid DbContext conflicts)
            services.AddFrameworkJsonApi(dbContextTypes);

            // 6. Add AutoMapper with explicit profile registration
            services.AddAutoMapper(cfg =>
            {
                // Register AutoMapper profiles from framework and custom modules
                AutoMapperConfig.RegisterMappings();
            }, AppDomain.CurrentDomain.GetAssemblies());

            // Note: HttpContextAccessor is registered at step 0.7 (before Identity)
            // Note: Authentication middleware, Controllers, Swagger are NOT needed for direct DB tests
        }

        /// <summary>
        /// Registers MITApplicationDbContext for tests.
        /// Uses same connection string as JsonApiDbContext.
        /// </summary>
        private static void RegisterMITApplicationDbContextForTests(
            IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("JsonApiDbContext")
                                      ?? configuration.GetConnectionString("DefaultConnection")
                                      ?? throw new InvalidOperationException("Connection string not found for MITApplicationDbContext");

            // Configure provider
            if (configuration["SqlProvider"] == "MySql" ||
                configuration["JsonApiSqlProvider"] == "MySql")
            {
                MITApplicationDbContext._UseSqlServer = false;
            }

            if (MITApplicationDbContext._UseSqlServer)
            {
                services.AddDbContext<MITApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString),
                    ServiceLifetime.Transient);
            }
            else
            {
                services.AddDbContext<MITApplicationDbContext>(options =>
                    options.UseMySQL(connectionString),
                    ServiceLifetime.Transient);
            }
        }

        /// <summary>
        /// Gets the current configuration instance.
        /// </summary>
        public static IConfiguration GetConfigurationInstance()
        {
            return _configuration ?? GetConfiguration();
        }

        /// <summary>
        /// Initializes JwtTokenProvider with configured options from DI.
        /// Required for JWT token generation in AccountController tests.
        /// Mirrors Program.cs initialization (line 236).
        /// </summary>
        private static void InitializeJwtTokenProvider(IServiceProvider serviceProvider)
        {
            try
            {
                var jwtOptions = serviceProvider.GetService<IOptions<JwtOptions>>();
                if (jwtOptions != null)
                {
                    MIT.Fwk.WebApi.Extension.JwtTokenProvider.Initialize(jwtOptions);
                }
                else
                {
                    Console.WriteLine("Warning: JwtOptions not found in DI container - JWT token generation may fail in tests");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize JWT token provider in tests: {ex.Message}");
            }
        }
    }
}
