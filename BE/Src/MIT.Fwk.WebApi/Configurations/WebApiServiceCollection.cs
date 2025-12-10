using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MIT.Fwk.WebApi.Configurations
{
    public static class WebApiServiceCollectionExtensions
    {
        public static IMvcBuilder AddWebApi(this IServiceCollection services, IConfiguration configuration = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // FASE 7: Configuration injected - no longer using ConfigurationHelper
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Configuration must be provided. ConfigurationHelper.AppConfig has been removed.");
            }

            IMvcCoreBuilder builder = services.AddMvcCore();
            //.AddNewtonsoftJson(o =>
            // {
            //     o.SerializerSettings.Converters.Add(new StringEnumConverter());
            //     o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // });

            builder.AddApiExplorer();
            builder.AddCors(o =>
            {
                o.AddPolicy("AllowSpecificOrigin", options =>
                {
                    string allowedOrigins = configuration["AllowedCorsOrigin"] ?? "*";
                    options.WithOrigins(allowedOrigins);
                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                    options.DisallowCredentials();
                });
            });

            return new MvcBuilder(builder.Services, builder.PartManager);
        }

        public static IMvcBuilder AddWebApi(this IServiceCollection services, IConfiguration configuration, Action<MvcOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            IMvcBuilder builder = services.AddWebApi(configuration);
            builder.Services.Configure(setupAction);

            return builder;
        }

        internal class MvcBuilder : IMvcBuilder
        {
            /// <summary>
            /// Initializes a new <see cref="MvcBuilder"/> instance.
            /// </summary>
            /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
            /// <param name="manager">The <see cref="ApplicationPartManager"/> of the application.</param>
            public MvcBuilder(IServiceCollection services, ApplicationPartManager manager)
            {
                Services = services ?? throw new ArgumentNullException(nameof(services));
                PartManager = manager ?? throw new ArgumentNullException(nameof(manager));
            }

            /// <inheritdoc />
            public IServiceCollection Services { get; }

            /// <inheritdoc />
            public ApplicationPartManager PartManager { get; }
        }

    }
}