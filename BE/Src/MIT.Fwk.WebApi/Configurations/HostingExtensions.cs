using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Services;
using MIT.Fwk.WebApi.Services;
using System;
using System.IO;
using System.Reflection;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring hosting services and configuration sources.
    /// Modernizes the configuration approach previously in Configurator.cs and Startup.cs.
    /// </summary>
    public static class HostingExtensions
    {
        /// <summary>
        /// Configures all JSON configuration sources for the framework.
        /// Loads: dbconnections.json, customsettings.json, appsettings.json
        /// </summary>
        public static IConfigurationBuilder AddFrameworkConfiguration(
            this IConfigurationBuilder builder,
            IHostEnvironment environment)
        {
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                              ?? Directory.GetCurrentDirectory();

            builder
                .SetBasePath(basePath)
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (environment.IsDevelopment())
            {
                // Add user secrets for development
                builder.AddUserSecrets<Program>(optional: true);
            }

            return builder;
        }

        /// <summary>
        /// Registers license check service (enabled only in RELEASE builds, not in DEBUG or DOCKER).
        /// </summary>
        public static IServiceCollection AddLicenseCheck(this IServiceCollection services)
        {
#if !DEBUG && !DOCKER
            // Only register license check in production (RELEASE builds)
            services.AddHostedService<LicenseCheckService>();
#endif
            return services;
        }

        /// <summary>
        /// Adds framework-specific logging configuration.
        /// </summary>
        public static ILoggingBuilder AddFrameworkLogging(this ILoggingBuilder builder)
        {
            builder.AddConsole();
            builder.AddDebug();

            // Add file logging if needed
            // builder.AddFile("Logs/webapi-{Date}.txt");

            return builder;
        }

        /// <summary>
        /// Initializes critical services that need to run before the application starts.
        /// </summary>
        public static WebApplication InitializeFramework(this WebApplication app)
        {
            // Any initialization logic can go here
            // For example: ensure database creation, seed data, etc.

            return app;
        }
    }
}
