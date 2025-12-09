using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Middleware
{
    /// <summary>
    /// Middleware per validazione claims JWT su entity specifiche.
    /// Responsabilità: Verificare permessi granulari utente (es. "Invoice.Read", "Customer.Write").
    /// </summary>
    /// <remarks>
    /// Questo middleware:
    /// - Esegue DOPO JwtAuthenticationMiddleware (richiede user autenticato)
    /// - Supporta [SkipClaimsValidation] con filtraggio per metodo HTTP
    /// - Verifica superadmin claim (bypass completo validazione)
    /// - Per controller JsonAPI: valida claim "{entity}.{action}" (es. "Invoice.read")
    /// - Filtra parametro "include" del query string basandosi sui claims utente
    /// - Ritorna 401 Unauthorized se claim mancante
    ///
    /// DEFAULT per entity JsonAPI [Resource]:
    /// - TUTTE le entity richiedono validazione claims
    /// - Solo entity con [SkipClaimsValidation] sono escluse
    ///
    /// NON gestisce:
    /// - Autenticazione JWT (responsabilità di JwtAuthenticationMiddleware)
    /// - Logging richieste (responsabilità di JwtLoggingMiddleware)
    /// </remarks>
    public class JwtClaimsValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtClaimsValidationMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IJwtClaimsService claimsService)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (claimsService == null)
                throw new ArgumentNullException(nameof(claimsService));

            if (context.Request.Path.Value.Contains("swagger"))
            {
                await _next(context);
                return;
            }

            // Skip se utente non autenticato (JwtAuthenticationMiddleware già gestito)
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var endpoint = context.GetEndpoint();

            // Check [AllowAnonymous] - bypass autenticazione
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var skipAuthAttr = endpoint?.Metadata?.GetMetadata<SkipClaimsValidationAttribute>();
            if (skipAuthAttr != null && skipAuthAttr.AppliesToMethod(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var genericTypeAttributes = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>()
                                                            .ControllerTypeInfo
                                                            .BaseType
                                                            .GenericTypeArguments;

            if (genericTypeAttributes != null && genericTypeAttributes.Length > 0)
            {
                var entitySkipAuthAttr = genericTypeAttributes[0]
                                            .CustomAttributes
                                            .FirstOrDefault(x => x.AttributeType.Equals(typeof(SkipJwtAuthenticationAttribute)) || x.AttributeType.Equals(typeof(SkipClaimsValidationAttribute))) ?? null;
                if (entitySkipAuthAttr != null)
                {
                    await _next(context);
                    return;
                }
            }

            // Check superadmin - bypass completo validazione claims
            if (claimsService.IsSuperAdmin(context))
            {
                // Superadmin: filtra include comunque per sicurezza, poi passa
                string tenantId = context.Request.Headers.TryGetValue("tenantId", out StringValues tid) ? tid.ToString() : "";
                var userClaims = await claimsService.GetUserClaimsPoolAsync(context.User.Identity.Name, tenantId);
                if (userClaims != null && userClaims.Count > 0)
                {
                    claimsService.FilterIncludesByClaimsPool(context, userClaims);
                }

                await _next(context);
                return;
            }

            // Verifica se è un controller JsonAPI
            var descriptor = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>();
            if (descriptor == null)
            {
                // Non è un controller: skip validazione claims
                await _next(context);
                return;
            }

            bool isJsonApiController = descriptor.ControllerTypeInfo.BaseType?.Assembly.ManifestModule.Name == "JsonApiDotNetCore.dll";

            if (!isJsonApiController)
            {
                // Non è JsonAPI: skip validazione claims (controller custom gestiscono auth con [Authorize])
                await _next(context);
                return;
            }

            // È un controller JsonAPI: valida claims per entity
            try
            {
                RouteData routeData = context.GetRouteData();
                string entityName = routeData.Values["controller"]?.ToString();

                if (string.IsNullOrEmpty(entityName))
                {
                    // Impossibile determinare entity: 404
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                // Check [SkipClaimsValidation] sull'entity stessa (per entity pubbliche)
                Type entityType = ResolveEntityTypeFromControllerName(entityName, descriptor);
                if (entityType != null)
                {
                    var entitySkipAttr = entityType.GetCustomAttribute<SkipClaimsValidationAttribute>();
                    if (entitySkipAttr != null && entitySkipAttr.AppliesToMethod(context.Request.Method))
                    {
                        // Entity pubblica per questo metodo HTTP: skip claims
                        await _next(context);
                        return;
                    }
                }

                // Valida claim per entity (singolare e plurale)
                string tenantId = context.Request.Headers.TryGetValue("tenantId", out StringValues tid2) ? tid2.ToString() : "";
                string username = context.User.Identity.Name;
                string entityNameSingular = entityName.Singularize(false);

                bool hasClaim = await claimsService.HasClaimAsync(username, entityNameSingular, context.Request.Method, tenantId)
                             || await claimsService.HasClaimAsync(username, entityName, context.Request.Method, tenantId);

                if (!hasClaim)
                {
                    // Claim mancante: 401 Unauthorized
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                // Claim OK: filtra include basandosi su claims utente
                var userClaimsPool = await claimsService.GetUserClaimsPoolAsync(username, tenantId);
                if (userClaimsPool != null && userClaimsPool.Count > 0)
                {
                    claimsService.FilterIncludesByClaimsPool(context, userClaimsPool);
                }

                // Passa al prossimo middleware
                await _next(context);
            }
            catch
            {
                // Errore nella validazione: 404
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        /// <summary>
        /// Risolve il tipo entity dal nome del controller JsonAPI.
        /// Cerca negli assembly caricati per entity con attributo [Resource].
        /// </summary>
        private Type ResolveEntityTypeFromControllerName(string controllerName, ControllerActionDescriptor descriptor)
        {
            try
            {
                // Prova a risolvere entity dal tipo del controller JsonAPI
                // JsonApiDotNetCore usa pattern: {EntityName}Controller per entity {EntityName}
                var controllerType = descriptor.ControllerTypeInfo.AsType();

                // Cerca GenericTypeArguments dal controller (alcuni controller JsonAPI espongono entity type)
                var baseType = controllerType.BaseType;
                while (baseType != null && baseType != typeof(object))
                {
                    if (baseType.IsGenericType && baseType.GetGenericArguments().Length > 0)
                    {
                        var entityType = baseType.GetGenericArguments().FirstOrDefault();
                        if (entityType != null && entityType.GetCustomAttribute<JsonApiDotNetCore.Resources.Annotations.ResourceAttribute>() != null)
                        {
                            return entityType;
                        }
                    }
                    baseType = baseType.BaseType;
                }

                // Fallback: non trovato
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
