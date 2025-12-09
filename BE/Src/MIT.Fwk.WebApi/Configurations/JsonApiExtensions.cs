using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.WebApi.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Extension methods for configuring JsonAPI and MVC.
    /// </summary>
    public static class JsonApiExtensions
    {
        /// <summary>
        /// Configures JsonAPI with auto-discovery of resources and DbContexts.
        /// </summary>
        public static IServiceCollection AddFrameworkJsonApi(
            this IServiceCollection services,
            List<Type> dbContextTypes)
        {
            // Add scoped services required by JsonAPI
            services.AddScoped<IncludeParser>();

            // Configure JsonAPI using reflection to support dynamic DbContexts
            var addJsonApiMethod = typeof(JsonApiDotNetCore.Configuration.ServiceCollectionExtensions)
                .GetMethod("AddJsonApi", new[]
                {
                    typeof(IServiceCollection),
                    typeof(Action<JsonApiOptions>),
                    typeof(Action<ServiceDiscoveryFacade>),
                    typeof(Action<ResourceGraphBuilder>),
                    typeof(IMvcCoreBuilder),
                    typeof(ICollection<Type>)
                });

            if (addJsonApiMethod == null)
                throw new InvalidOperationException("AddJsonApi method not found via reflection");

            addJsonApiMethod.Invoke(null,
            [
                services,
                new Action<JsonApiOptions>(options =>
                {
                    // Configure JsonAPI options
                    options.Namespace = "api/v{version}";
                    options.UseRelativeLinks = true;
                    options.IncludeTotalResourceCount = true;
                    options.SerializerOptions.WriteIndented = true;
                    options.ClientIdGeneration = ClientIdGenerationMode.Allowed;
                    options.AllowUnknownQueryStringParameters = true;
                    options.DefaultPageSize = null;
                    options.DefaultHasManyCapabilities = HasManyCapabilities.All;
                    options.IncludeExceptionStackTraceInErrors = true;

                    // Add custom converters
                    options.SerializerOptions.Converters.Add(new JsonDateTimeOffsetConverter());
                    options.SerializerOptions.Converters.Add(new JsonDateTimeConverter());
                }),
                new Action<ServiceDiscoveryFacade>(discovery =>
                {
                    // Auto-discover JsonAPI resources from current assembly (WebApi - for controllers)
                    discovery.AddCurrentAssembly();

                    // Add Infrastructure assembly for entities ([Resource] types)
                    discovery.AddAssembly(typeof(MIT.Fwk.Infrastructure.Entities.Performer).Assembly);
                }),
                null, // ResourceGraphBuilder (use default)
                null, // IMvcCoreBuilder (use default)
                dbContextTypes // Dynamically discovered DbContext types
            ]);

            return services;
        }

        /// <summary>
        /// Configures MVC controllers with Newtonsoft.Json and System.Text.Json support.
        /// </summary>
        public static IServiceCollection AddFrameworkControllers(
            this IServiceCollection services,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // Add WebApi configuration from WebApiServiceCollection
            services.AddWebApi(configuration, options =>
            {
                options.OutputFormatters.Remove(new XmlDataContractSerializerOutputFormatter());
                options.UseCentralRoutePrefix(new RouteAttribute("api/v{version}"));
            });

            // Add controllers with Newtonsoft.Json and System.Text.Json
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonDateTimeOffsetConverter());
                    options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                });

            // Enable MVC with endpoint routing disabled (for compatibility)
            services.AddMvc(options => options.EnableEndpointRouting = false);

            return services;
        }

        /// <summary>
        /// Discovers and registers JsonAPI controllers from loaded assemblies.
        /// </summary>
        public static IServiceCollection AddFrameworkApplicationParts(this IServiceCollection services)
        {
            IMvcBuilder mvcBuilder = services.AddMvc();

            // Auto-discover JsonApiController<,> implementations
            List<Assembly> jsonApiControllers = ReflectionHelper.LoadAllV2(typeof(JsonApiDotNetCore.Controllers.JsonApiController<,>));
            foreach (Assembly controller in jsonApiControllers)
            {
                mvcBuilder.AddApplicationPart(controller);
            }

            return services;
        }
    }

    /// <summary>
    /// Custom DateTimeOffset converter for System.Text.Json (from Startup.cs).
    /// </summary>
    public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }

    /// <summary>
    /// Custom DateTime converter for System.Text.Json (from Startup.cs).
    /// </summary>
    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}
