using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using MIT.Fwk.Core.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace MIT.Fwk.WebApi.Configurations
{
    /// <summary>
    /// Operation filter che aggiunge parametri header richiesti dal middleware JWT.
    /// Aggiunge fingerPrint e tenantId agli endpoint che richiedono autenticazione.
    /// </summary>
    public class AddRequiredHeadersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
                return;

            // Verifica se l'endpoint richiede autenticazione
            bool requiresAuth = true;

            // Check [AllowAnonymous]
            var allowAnonymous = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AllowAnonymousAttribute>()
                .Any();

            if (allowAnonymous)
                requiresAuth = false;

            // Check [SkipJwtAuthentication]
            var skipJwt = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<SkipJwtAuthenticationAttribute>()
                .Any();

            if (skipJwt)
                requiresAuth = false;

            // Se non richiede autenticazione, non aggiungere headers
            if (!requiresAuth)
                return;

            // Inizializza Parameters se null (OpenAPI 2.x usa IList<IOpenApiParameter>)
            if (operation.Parameters == null)
                operation.Parameters = new List<IOpenApiParameter>();

            // Aggiungi fingerPrint header
            if (!operation.Parameters.Any(p => p.Name == "fingerPrint"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "fingerPrint",
                    In = ParameterLocation.Header,
                    Description = "Hash del dispositivo (fingerprint) per validazione autenticazione",
                    Required = false, // Opzionale ma consigliato
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }

            // Aggiungi tenantId header
            if (!operation.Parameters.Any(p => p.Name == "tenantId"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "tenantId",
                    In = ParameterLocation.Header,
                    Description = "ID del tenant per autenticazione multi-tenant",
                    Required = false, // Opzionale
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }
        }
    }
}
