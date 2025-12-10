using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring Swagger/OpenAPI 3.0 documentation.
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Adds Swagger/OpenAPI 3.0 generation services.
        /// </summary>
        public static IServiceCollection AddFrameworkSwagger(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                // Add custom operation filter
                options.OperationFilter<SwaggerDefaultValues>();

                // Configure API documentation - OpenAPI 3.0
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "2.0",  // Changed from "v2" to semantic version
                    Title = configuration["Swagger:Title"] ?? "MIT.Fwk API",
                    Description = configuration["Swagger:Description"] ?? "Maestrale Framework API",
                    Contact = new OpenApiContact
                    {
                        Name = configuration["Swagger:Owner"] ?? "Maestrale",
                        Email = configuration["Swagger:Email"] ?? "info@maestrale.com",
                        Url = new Uri(configuration["Swagger:Website"] ?? "https://www.maestrale.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = configuration["Swagger:License"] ?? "Proprietary"
                    }
                });

                // Basic Authentication scheme
                options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using username and password."
                });

                // Bearer/JWT Authentication scheme
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
                });

                // Note: In Swashbuckle 10.x (OpenAPI 3.1), security requirements are applied
                // automatically based on [Authorize] attributes. If you need to manually add
                // global security requirements, use an operation filter instead of AddSecurityRequirement.
                // See: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md

                // The security definitions above (Basic and Bearer) are sufficient for Swagger UI
                // to show the "Authorize" button and allow users to authenticate.

                // Optional: Include XML comments if available
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // if (File.Exists(xmlPath))
                // {
                //     options.IncludeXmlComments(xmlPath);
                // }
            });

            return services;
        }

        /// <summary>
        /// Configures Swagger UI middleware with OpenAPI 3.0.
        /// </summary>
        public static IApplicationBuilder UseFrameworkSwagger(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            bool enableSwagger = configuration.GetValue<bool>("EnableSwagger", false);

            if (!enableSwagger)
                return app;

            // Enable Swagger middleware
            // Swashbuckle 9.x generates OpenAPI 3.1 by default
            app.UseSwagger(options => {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
            });

            // Enable Swagger UI
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/v2/swagger.json", "MIT.Fwk Project API v2.0");
                s.RoutePrefix = "swagger"; // Swagger UI at /swagger

                // Optional: Customize UI
                s.DocumentTitle = configuration["Swagger:Title"] ?? "MIT.Fwk API Documentation";
                s.DisplayRequestDuration(); // Show request duration in UI
                s.EnableDeepLinking(); // Enable deep linking for operations
                s.EnableFilter(); // Enable filter box
            });

            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($"Swagger UI enabled (OpenAPI 3.0): {configuration["Swagger:Title"]}");
            Console.WriteLine($"Swagger endpoint: /swagger");
            Console.WriteLine("-------------------------------------------------");

            return app;
        }
    }
}
