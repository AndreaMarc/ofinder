using DasMulli.Win32.ServiceUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.IoC;
using MIT.Fwk.Licensing;
using MIT.Fwk.WebApi.Configurations;
using MIT.Fwk.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi
{
    public class Program
    {
        private static readonly AutoResetEvent _closingEvent = new(false);

        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Handle command-line arguments that don't require full app initialization
                if (ShouldHandleCommandLineOnly(args))
                {
                    return HandleCommandLineActions(args);
                }

                // Build and run the web application
                return await RunWebApplicationAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] WebApi.Main: {ex.Message}");
#if DEBUG || DOCKER
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
#endif
                return 1;
            }
        }

        /// <summary>
        /// Determines if we should handle command-line args without starting the web app.
        /// </summary>
        private static bool ShouldHandleCommandLineOnly(string[] args)
        {
            return Array.Exists(args, arg =>
                arg == "--help" ||
                arg == "-key" ||
                arg == "-lic" ||
                arg == "--register-service" ||
                arg == "--unregister-service");
        }

        /// <summary>
        /// Handles command-line actions that don't require the web app to run.
        /// </summary>
        private static int HandleCommandLineActions(string[] args)
        {
            // Create minimal license service for license operations
            ILicenseService licenseService = CreateLicenseService();
            CommandLineHandler handler = new(licenseService);

            CommandLineAction action = handler.DetermineAction(args);

            return action switch
            {
                CommandLineAction.GenerateKey => handler.HandleGenerateKey(args),
                CommandLineAction.ActivateLicense => handler.HandleActivateLicense(args),
                CommandLineAction.RegisterService => handler.HandleRegisterService(args),
                CommandLineAction.UnregisterService => handler.HandleUnregisterService(args),
                CommandLineAction.ShowHelp => handler.HandleShowHelp(),
                _ => 0
            };
        }

        /// <summary>
        /// Builds and runs the web application.
        /// </summary>
        private static async Task<int> RunWebApplicationAsync(string[] args)
        {
#if DEBUG
            Console.WriteLine("Starting in DEBUG mode... [license OFF]");
#elif DOCKER
            Console.WriteLine("Starting in CONTAINER mode... [license OFF]");
#else
            // Production mode - validate license before starting
            ValidateLicense();
#endif

            // Determine execution mode
            bool runAsService = Array.Exists(args, arg => arg == "--run-as-service");
            bool isDocker = false;
#if DOCKER
            isDocker = true;
#endif

            WebApplicationBuilder builder = CreateWebApplicationBuilder(args, runAsService);

            // Configure services (uses Startup.cs pattern for now)
            ConfigureServices(builder);

            WebApplication app = builder.Build();

            // Configure middleware pipeline (uses modern extension methods)
            await ConfigureMiddleware(app);

            // Run based on execution mode
            if (isDocker)
            {
                return await RunInDockerModeAsync(app);
            }
            else if (runAsService)
            {
                // Windows Service mode - app.Run() will use WindowsServiceLifetime
                await app.RunAsync();
                return 0;
            }
            else
            {
                // Interactive mode
                return await RunInteractiveAsync(app);
            }
        }

        /// <summary>
        /// Creates and configures the WebApplicationBuilder.
        /// </summary>
        private static WebApplicationBuilder CreateWebApplicationBuilder(string[] args, bool runAsService)
        {
            WebApplicationOptions options = new()
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory
            };

            WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

            // Configure framework configuration sources (modern pattern)
            builder.Configuration.AddFrameworkConfiguration(builder.Environment);

            // Configure Kestrel with SSL support
            builder.WebHost.ConfigureKestrelEndpoints(builder.Configuration);
            builder.WebHost.ConfigureKestrelLimits();

            // Cross-platform service support
            if (runAsService)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows Service lifetime is managed by DasMulli.Win32.ServiceUtils
                    // No additional configuration needed here
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux systemd integration
                    builder.Host.UseSystemd();
                }
            }

            // Configure logging
            builder.Logging.AddFrameworkLogging();

            return builder;
        }

        /// <summary>
        /// Configures services using modern extension methods.
        /// Replaces the old Startup.ConfigureServices pattern.
        /// </summary>
        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            IConfiguration configuration = builder.Configuration;
            IServiceCollection services = builder.Services;

            // 1. Database configuration (EF Core + auto-discovery)
            List<Type> dbContextTypes;
            services.AddFrameworkDatabases(configuration, out dbContextTypes);

            // 1.5. Apply migrations IMMEDIATELY if enabled (before any queries are executed)
            bool enableAutoMigrations = configuration.GetValue<bool>("EnableAutoMigrations", false);
            if (enableAutoMigrations)
            {
                ApplyMigrationsSync(services, dbContextTypes, configuration);
            }

            // Create temporary JsonApiDbContext for setup queries (NOW SAFE - migrations applied)
            using var scope = services.BuildServiceProvider().CreateScope();
            var jsonDbContext = scope.ServiceProvider.GetRequiredService<JsonApiDbContext>();

            // 2. JsonAPI configuration
            services.AddFrameworkJsonApi(dbContextTypes);

            // 3. Controllers and MVC
            services.AddFrameworkControllers(configuration);
            services.AddFrameworkApplicationParts();

            // 4. Identity and Authentication
            services.AddFrameworkIdentity(configuration, jsonDbContext);
            services.AddFrameworkAuthentication();
            services.AddJwtServices(); // JWT middleware services (FASE 2-3 refactoring)

            // 5. Authorization policies
            services.AddFrameworkAuthorization();

            // 6. AutoMapper
            services.AddFrameworkAutoMapper();

            // 7. Session
            services.AddFrameworkSession();

            // 8. HTTPS redirection
            services.AddFrameworkHttps(configuration);

            // 9. CORS
            services.AddFrameworkCors(configuration);

            // 10. Swagger/OpenAPI 3.0
            services.AddFrameworkSwagger(configuration);

            // 11. MediatR (CQRS)
            services.AddFrameworkMediatR();

            // 12. Framework services (includes Firebase singleton)
            services.AddFrameworkServices(configuration);

            // 13. WebSocket notifications
            services.AddFrameworkWebSocket(jsonDbContext);

            // 14. Native Injector (DI bootstrapping)
            NativeInjectorBootStrapper.RegisterServices(services, configuration);

            // 15. Quartz scheduler
            services.AddFrameworkScheduler(configuration, jsonDbContext);

            // 16. License check service (only in RELEASE)
            services.AddLicenseCheck();

            // 17. Initialize JWT token provider
            using (var provider = services.BuildServiceProvider())
            {
                provider.InitializeJwtTokenProvider();
            }
        }

        /// <summary>
        /// Applies database migrations synchronously during service configuration.
        /// This ensures tables exist before any queries are executed during startup.
        /// </summary>
        private static void ApplyMigrationsSync(IServiceCollection services, List<Type> dbContextTypes, IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("=================================================");
                Console.WriteLine("Applying DbContext migrations during startup...");
                Console.WriteLine("=================================================");

                using var scope = services.BuildServiceProvider().CreateScope();
                var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();

                // Apply migrations synchronously (block until complete)
                bool success = migrationService.ApplyMigrationsAsync().GetAwaiter().GetResult();

                if (success)
                {
                    Console.WriteLine("=================================================");
                    Console.WriteLine("DbContext migrations completed successfully");
                    Console.WriteLine("=================================================");
                }
                else
                {
                    Console.WriteLine("=================================================");
                    Console.WriteLine("WARNING: Migrations failed - check logs");
                    Console.WriteLine("=================================================");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("=================================================");
                Console.WriteLine($"ERROR applying migrations: {ex.Message}");
                Console.WriteLine("=================================================");
#if DEBUG
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
#endif
                // Don't throw - allow app to start even if migrations fail
            }
        }

        /// <summary>
        /// Configures middleware pipeline using modern extension methods.
        /// Replaces the old Startup.Configure pattern.
        /// </summary>
        private static async Task ConfigureMiddleware(WebApplication app)
        {
            // Configure complete middleware pipeline
            app.UseFrameworkMiddleware(app.Configuration);

            // NOTE: Migrations are now applied during ConfigureServices (before any queries)
            // This ensures database tables exist before Identity/WebSocket/Scheduler configuration

            // Print diagnostic information
            app.PrintFrameworkDiagnostics(app.Configuration);

            await Task.CompletedTask; // Keep async signature for consistency
        }

        /// <summary>
        /// Runs the application in Docker container mode.
        /// </summary>
        private static async Task<int> RunInDockerModeAsync(WebApplication app)
        {
            Console.WriteLine("Running in container mode...");

            // Start the application in background
            Task appRunTask = app.RunAsync();

            // Setup cancellation
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutdown signal received...");
                e.Cancel = true;
                _closingEvent.Set();
            };

            // Wait for cancellation signal
            await Task.Run(() => _closingEvent.WaitOne());

            // Stop the application gracefully
            await app.StopAsync();

            Console.WriteLine("Application stopped");
            return 0;
        }

        /// <summary>
        /// Runs the application in interactive console mode.
        /// </summary>
        private static async Task<int> RunInteractiveAsync(WebApplication app)
        {
            Console.WriteLine("Running interactively, press ENTER to stop or Ctrl+C to force exit.");

            // Start the application in background
            CancellationTokenSource cts = new();
            Task appRunTask = app.RunAsync(cts.Token);

            // Setup console cancellation
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Ctrl+C pressed - shutting down...");
                e.Cancel = true;
                cts.Cancel();
            };

            // Wait for user input or app termination
            Task readLineTask = Task.Run(() => Console.ReadLine());
            Task completedTask = await Task.WhenAny(appRunTask, readLineTask);

            if (completedTask == readLineTask)
            {
                Console.WriteLine("Stopping application...");
                await cts.CancelAsync();
                await app.StopAsync();
            }

            Console.WriteLine("Application stopped");
            return 0;
        }

        /// <summary>
        /// Validates license at startup (production only).
        /// </summary>
        private static void ValidateLicense()
        {
            ILicenseService licenseService = CreateLicenseService();
            if (!licenseService.IsValid())
            {
                throw new InvalidLicenseException("Invalid license - application cannot start");
            }
        }

        /// <summary>
        /// Creates a minimal LicenseService instance before DI is configured.
        /// Used for command-line operations and initial validation.
        /// </summary>
        private static ILicenseService CreateLicenseService()
        {
            IEncryptionService encryptionService = new EncryptionService();
            LicenseOptions options = new(); // Uses defaults
            IOptions<LicenseOptions> optionsWrapper = Options.Create(options);

            return new LicenseService(encryptionService, optionsWrapper);
        }
    }
}
