using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JsonApiDotNetCore.Configuration;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.IoC;
using MIT.Fwk.WebApi.Extension;
using MIT.Fwk.WebApi.Middleware;
using MIT.Fwk.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Configurations
{
    # nullable enable
    /// <summary>
    /// Extension methods for configuring the middleware pipeline.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Configures the complete middleware pipeline for the application.
        /// </summary>
        public static WebApplication UseFrameworkMiddleware(
            this WebApplication app,
            IConfiguration configuration)
        {
            // CRITICAL: Forwarded Headers MUST be first to properly detect HTTPS behind reverse proxy
            // This reads X-Forwarded-Proto, X-Forwarded-Host, X-Forwarded-For headers from nginx
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor,
                // For production with nginx, we need to accept any proxy (or configure specific IPs)
                KnownNetworks = { },
                KnownProxies = { }
            });

            // Enable request buffering and synchronous IO
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                IHttpBodyControlFeature? syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
                if (syncIoFeature != null)
                {
                    syncIoFeature.AllowSynchronousIO = true;
                }
                await next();
            });

            // Routing
            app.UseRouting();

            // JsonAPI middleware
            app.UseJsonApi();

            // Status code pages
            app.UseStatusCodePages();

            // Developer exception page in development
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // HTTPS redirection
            if (configuration.GetValue<bool>("EnableSSL", false))
            {
                app.UseHttpsRedirection();
            }

            // CORS - DISABLED: nginx handles CORS headers (prevents duplicate headers)
            // app.UseCors(c =>
            // {
            //     c.DisallowCredentials();
            //     c.AllowAnyHeader();
            //     c.AllowAnyMethod();
            //
            //     string allowedOrigin = configuration["AllowedCorsOrigin"] ?? "*";
            //     if (allowedOrigin != "*")
            //     {
            //         c.WithOrigins(allowedOrigin);
            //     }
            //     else
            //     {
            //         c.AllowAnyOrigin();
            //     }
            // });

            // Authentication & Authorization Pipeline (FASE 2-3 refactoring)
            // CRITICAL ORDER: Basic → JWT Auth → JWT Claims → JWT Logging
            app.UseMiddleware<JwtAuthenticationMiddleware>(); // JWT token validation & sign-in
            app.UseMiddleware<JwtClaimsValidationMiddleware>(); // Entity-level claims validation
            app.UseMiddleware<JwtLoggingMiddleware>(); // Request logging (fire-and-forget, non-blocking)

            // Static files (wwwroot) - for Swagger custom JavaScript
            app.UseStaticFiles();

            // Swagger/OpenAPI 3.0 - MUST be before UseMvc()
            app.UseFrameworkSwagger(configuration);

            // Auto-discover and configure IApplicationBuilderHandler implementations
            List<object> builders = ReflectionHelper.ResolveAll<IApplicationBuilderHandler>();
            foreach (IApplicationBuilderHandler builder in builders.Cast<IApplicationBuilderHandler>())
            {
                builder.Configure(app, app.Environment, app.Services);
            }

            // MVC
            app.UseMvc();

            // Map controllers (modern ASP.NET Core 6+ pattern)
            app.MapControllers();

            return app;
        }

        /// <summary>
        /// Applies database migrations if enabled in configuration.
        /// Must be called after app is built but before it runs.
        /// </summary>
        public static async Task<WebApplication> ApplyDatabaseMigrationsAsync(
            this WebApplication app,
            IConfiguration configuration)
        {
            bool enableAutoMigrations = configuration.GetValue<bool>("EnableAutoMigrations", false);

            if (!enableAutoMigrations)
                return app;

            Console.WriteLine("=================================================");
            Console.WriteLine("Applying DbContext migrations...");
            Console.WriteLine("=================================================");

            try
            {
                using IServiceScope scope = app.Services.CreateScope();
                DatabaseMigrationService migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
                bool success = await migrationService.ApplyMigrationsAsync();

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
                // Don't throw - allow app to start even if migrations fail
            }

            return app;
        }

        /// <summary>
        /// Prints diagnostic information about the framework configuration.
        /// </summary>
        public static WebApplication PrintFrameworkDiagnostics(
            this WebApplication app,
            IConfiguration configuration)
        {
#if DOCKER
            try
            {
                var connStr = configuration.GetConnectionString("DefaultConnection");
                var noSqlStr = configuration.GetConnectionString("NoSQLConnection");
                Console.WriteLine($"SQL connection {((connStr?.Length > 50) ? connStr.Substring(0, 50) : connStr)}");
                Console.WriteLine($"NoSQL connection {((noSqlStr?.Length > 50) ? noSqlStr.Substring(0, 50) : noSqlStr)}");
            }
            catch { }
#endif

            try
            {
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine($"LogPath: {configuration["Logging:LogPath"]}");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Database Connections:");

                // Read connection strings from configuration
                var connectionStrings = configuration.GetSection("ConnectionStrings");
                foreach (var conn in connectionStrings.GetChildren())
                {
                    Console.WriteLine($"  {conn.Key}: {conn.Value}");
                }

                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Framework Mode: Fork-based (no external plugins)");
                Console.WriteLine("Discovered DbContexts:");

                // List discovered DbContexts
                List<object> discoveredContexts = ReflectionHelper.ResolveAll<IJsonApiDbContext>();
                foreach (object ctx in discoveredContexts)
                {
                    Console.WriteLine($"  - {ctx.GetType().Name}");
                }
                Console.WriteLine("-------------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing diagnostics: {ex.Message}");
            }

            return app;
        }
    }
}
