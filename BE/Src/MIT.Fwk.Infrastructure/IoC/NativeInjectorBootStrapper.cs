//using MIT.Fwk.Models.CommandHandlers;
//using MIT.Fwk.Models.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.IoC;
using MIT.Fwk.Core.Options;
using System;
using System.Linq;
//using MIT.Fwk.Domain.CommandHandlers; // FASE 9: CommandHandler removed - no longer used
//using MIT.Fwk.Domain.Commands; // FASE 9: Domain commands removed - legacy CQRS pattern
using MIT.Fwk.Infrastructure.AutoMapper;
using MIT.Fwk.Infrastructure.Bus;
//using MIT.Fwk.Infrastructure.CommandHandlers; // FASE 9: DomainCommandHandler removed
//using MIT.Fwk.Models.EventHandlers;
//using MIT.Fwk.Models.Events;
//using MIT.Fwk.Models.Interfaces;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Data.NoSql;
using MIT.Fwk.Infrastructure.Data.NoSql.Document;
using MIT.Fwk.Infrastructure.Data.Repositories;
using MIT.Fwk.Infrastructure.Handlers;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Services;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Licensing;
using MIT.Fwk.Infrastructure.Entities;

namespace MIT.Fwk.Infrastructure.IoC
{
    public class NativeInjectorBootStrapper
    {

        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Options pattern for typed configuration
            // Replaces ConfigurationHelper static access with DI-based IOptions<T>
            services.Configure<JwtOptions>(configuration.GetSection("Authentication:Jwt"));
            services.Configure<SmtpOptions>(configuration.GetSection("SMTP"));
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
            services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));
            services.Configure<LogOptions>(configuration.GetSection("Logging"));
            services.Configure<WebSocketOptions>(configuration.GetSection("WebSocket"));
            services.Configure<HttpClientOptions>(configuration.GetSection("HttpClient"));
            services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));
            services.Configure<SwaggerOptions>(configuration.GetSection("Swagger"));
            services.Configure<LicenseOptions>(configuration.GetSection("License"));

            // ASP.NET HttpContext dependency
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Core Services - Modern DI pattern (replaces static helpers)
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILogService, LogService>();

            // Connection String Provider - Modern DI pattern replaces ConfigurationHelper
            services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();

            // JWT Services - Modern attribute-based middleware (FASE 2-3 refactoring)
            // Registered in MIT.Fwk.WebApi via IdentityExtensions.AddJwtServices()
            // Services: IJwtAuthenticationService, IJwtClaimsService, IRequestLoggingService

            // Domain Bus (Mediator) -  CQRS
            services.AddScoped<IMediatorHandler, InMemoryBus>();

            // CUSTOM - APP
            AutoMapperConfig.RegisterMappings();

            // FASE 5: IAppService e IAppServiceV2 eliminati - usare JsonApiDbContext o IJsonApiManualService direttamente

            // Auto-discover application service handlers from loaded assemblies
            // This includes handlers from custom modules (e.g., MIT.Fwk.Examples)
            // Plugin loading removed - only searches in AppDomain assemblies
            System.Collections.Generic.List<object> customApps = ReflectionHelper.ResolveAll<IApplicationServiceHandler>();
            foreach (IApplicationServiceHandler app in customApps)
            {
                app.Configure(services);
            }


            // Domain - Events
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
            //services.AddScoped<INotificationHandler<CustomerRegisteredEvent>, CustomerEventHandler>();
            //services.AddScoped<INotificationHandler<CustomerUpdatedEvent>, CustomerEventHandler>();
            //services.AddScoped<INotificationHandler<CustomerRemovedEvent>, CustomerEventHandler>();

            // Domain - Commands: REMOVED (FASE 9)
            // DomainCommandHandler eliminato - usava IEntity (legacy pattern rimosso)
            // Modern approach: use DbContext directly or IJsonApiManualService for complex queries

            // Document/FwkLog Commands - REMOVED: handlers eliminati, services usano repository direttamente
            // DocumentCommandHandler e FwkLogCommandHandler eliminati (FASE 2 refactoring)
            // DocumentService e FwkLogService refactorati per usare repository diretto (FASE 9)

            // Infra - Data
            // FASE 5: IRepository e IRepositoryV2 eliminati
            // FASE 9: DomainCommandHandler eliminato (dipendeva da IEntity rimosso)

            services.AddScoped<MongoContext>(); // MongoDB context for DocumentFile and FwkLog
            // FASE 4: Repository Pattern eliminated - use factory pattern directly
            services.AddScoped<IDocFactory, DocFactory>();
            services.AddScoped<IFwkLogFactory, FwkLogFactory>();

            // FASE 7: IUnitOfWork removed - use DbContext.SaveChangesAsync() directly
            // services.AddScoped<IUnitOfWork, UnitOfWork>();

            //services.AddScoped<ICustomerRepository, CustomerRepository>();
            //services.AddScoped<CustomDbContext>();

            // CUSTOM - DOMAIN
            // Auto-discover domain service handlers from loaded assemblies
            // This includes handlers from custom modules (e.g., MIT.Fwk.Examples)
            // Plugin loading removed - only searches in AppDomain assemblies
            System.Collections.Generic.List<object> customDomains = ReflectionHelper.ResolveAll<IDomainServiceHandler>();
            foreach (IDomainServiceHandler dom in customDomains)
            {
                dom.Configure(services);
            }

            // DOCUMENT SERVICE

            //            IDomainServiceHandler customDomain = (IDomainServiceHandler)ReflectionHelper.Resolve<IDomainServiceHandler>(); //ReflectionHelper.CreateInstance(ConfigurationHelper.Get("DomainServiceHandler"));
            //            customDomain.Configure(services);
            services.AddScoped<IDocumentService, DocumentService>();

            // FWK LOG SERVICE

            //            IDomainServiceHandler customDomain = (IDomainServiceHandler)ReflectionHelper.Resolve<IDomainServiceHandler>(); //ReflectionHelper.CreateInstance(ConfigurationHelper.Get("DomainServiceHandler"));
            //            customDomain.Configure(services);
            services.AddScoped<IFwkLogService, FwkLogService>();

            // LICENSE SERVICE - Modern DI pattern replaces static License class
            // FASE 8B: ILicenseService enabled (replaces static License class)
            services.AddSingleton<ILicenseService, LicenseService>();
            // Note: Uncomment above line to enable license validation at startup
            // Configure LicenseOptions in appsettings.json under "License" section

            // Infra - Data EventSourcing removed - events published via MediatR only (no SQL persistence)
            // MongoDB logging handled by middleware + MongoLogBusManager job

            // Infra - Services
            ServiceFactory.ConfigureServices(services);

            // Auto-discovery for custom *ManualService implementations
            // This allows developers to create I*ManualService/Impl*ManualService pairs
            // without needing to manually register them or create ServiceHandlers
            RegisterFwkCustomServices(services);

            // Infra - Identity
            services.AddScoped<IUser, AspNetUserModel>();
        }

        /// <summary>
        /// Auto-discovers and registers all custom *ManualService implementations from loaded assemblies.
        /// Searches for interfaces matching pattern I*ManualService (e.g., IOtherManualService, IMyCustomManualService)
        /// and automatically registers them with their concrete implementations.
        /// </summary>
        /// <param name="services">Service collection for dependency injection</param>
        private static void RegisterFwkCustomServices(IServiceCollection services)
        {
            try
            {
                // Load all MIT.*.dll files to ensure custom modules are discovered
                foreach (string dll in System.IO.Directory.GetFiles(AppContext.BaseDirectory, "MIT.*.dll"))
                {
                    try
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(dll);
                        if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            System.Reflection.Assembly.LoadFrom(dll);
                        }
                    }
                    catch
                    {
                        // Ignore assemblies that cannot be loaded
                    }
                }

                // Get all loaded MIT assemblies (including custom modules like MIT.Fwk.Examples)
                var mitAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(ass => ass.FullName.StartsWith("MIT") && !ass.FullName.StartsWith("MIT.Fwk.Core") && !ass.FullName.StartsWith("MIT.Fwk.Infrastructure") && 
                                    !ass.FullName.StartsWith("MIT.Fwk.WebApi") && !ass.FullName.StartsWith("MIT.Fwk.Tests"))
                    .ToList();

                int registeredCount = 0;

                foreach (var assembly in mitAssemblies)
                {
                    // Find all implementation implementing "IFwkCustomService"
                    var customServices = assembly.GetTypes()
                        .Where(t => !t.IsInterface &&
                                    !t.IsAbstract &&
                                    typeof(IFwkCustomService).IsAssignableFrom(t)) // Skip framework's own service
                        .ToList();

                    foreach (var customService in customServices)
                    {
                        // Expected interface name: add "I" from service name
                        // E.g., OtherManualService -> IOtherManualService
                        string interfaceName = $"I{customService.Name}";

                        // Search for interface in all MIT assemblies
                        Type interfaceType = null;
                        foreach (var searchAssembly in mitAssemblies)
                        {
                            interfaceType = searchAssembly.GetTypes()
                                .FirstOrDefault(t => t.IsInterface &&
                                                     t.Name == interfaceName);

                            if (interfaceType != null)
                                break;
                        }

                        if (interfaceType != null && customService != null)
                        {
                            // Check if not already registered
                            if (!services.Any(sd => sd.ServiceType == interfaceType))
                            {
                                services.AddScoped(interfaceType, customService);
                                registeredCount++;

                                Console.WriteLine($"[Auto-Discovery] Registered: {interfaceType.Name} -> {customService.Name}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Auto-Discovery] WARNING: No implementation found for {interfaceType.Name} (expected class name: {customService.Name})");
                        }
                    }
                }

                if (registeredCount > 0)
                {
                    Console.WriteLine($"[Auto-Discovery] Successfully registered {registeredCount} custom Service(s)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auto-Discovery] Error during Service registration: {ex.Message}");
            }
        }
    }
}