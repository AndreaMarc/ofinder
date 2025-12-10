using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Scheduler;
using MIT.Fwk.WebApi.Configurations;
using MIT.Fwk.WebApi.Extension;
using MIT.Fwk.WebApi.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring Quartz.NET scheduler and background services.
    /// </summary>
    public static class QuartzExtensions
    {
        /// <summary>
        /// Configures Quartz.NET scheduler if enabled in configuration.
        /// </summary>
        public static IServiceCollection AddFrameworkScheduler(
            this IServiceCollection services,
            IConfiguration configuration,
            JsonApiDbContext dbContext)
        {
            bool enableJobs = configuration.GetValue<bool>("EnableJobs", false);

            if (!enableJobs)
            {
                Console.WriteLine("Quartz Scheduler: Disabled (EnableJobs=false)");
                return services;
            }

            Console.WriteLine("QuartzStartup...");
            Console.WriteLine("Starting Jobs...");

            // Create QuartzSchedulerOptions from configuration
            QuartzSchedulerOptions quartzOptions = new()
            {
                ToStart = configuration["Scheduler:ToStart"] ?? string.Empty,
                SerializerType = configuration["Scheduler:SerializerType"] ?? "json",
                ShutdownTimeoutMs = int.TryParse(configuration["Scheduler:ShutdownTimeoutMs"], out int timeout)
                    ? timeout
                    : 30000
            };

            IOptions<QuartzSchedulerOptions> options = Options.Create(quartzOptions);
            QuartzStartup quartzStartup = new(services, options);
            quartzStartup.Start();

            return services;
        }

        /// <summary>
        /// Registers WebSocket notification service as a background hosted service.
        /// The service will start automatically if enabled in database configuration.
        /// </summary>
        public static IServiceCollection AddFrameworkWebSocket(
            this IServiceCollection services,
            JsonApiDbContext dbContext)
        {
            try
            {
                var setup = dbContext.Setups.FirstOrDefault(x => x.environment == "web");
                if (setup != null && (setup.internalChat != "0" || setup.internalNotifications))
                {
                    // Register WebSocket service as hosted background service
                    services.AddHostedService<WebSocketNotificationService>();
                    Console.WriteLine("WebSocket notifications: Enabled (Background Service)");
                }
            }
            catch
            {
                // Ignore errors - WebSocket is optional
            }

            return services;
        }

        /// <summary>
        /// Initializes JWT token provider with configured options.
        /// Must be called after services are built.
        /// </summary>
        public static IServiceProvider InitializeJwtTokenProvider(this IServiceProvider serviceProvider)
        {
            try
            {
                // Initialize JwtTokenProvider with options from DI
                var jwtOptions = serviceProvider.GetService<IOptions<MIT.Fwk.Core.Options.JwtOptions>>();

                if (jwtOptions != null)
                {
                    JwtTokenProvider.Initialize(jwtOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to initialize JWT token provider: {ex.Message}");
            }

            return serviceProvider;
        }
    }
}
