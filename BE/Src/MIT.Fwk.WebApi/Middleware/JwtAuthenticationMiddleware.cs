using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Middleware
{
    /// <summary>
    /// Middleware per autenticazione JWT.
    /// Responsabilità: Validare token JWT e eseguire sign-in utenti.
    /// </summary>
    /// <remarks>
    /// Questo middleware:
    /// - Verifica header Authorization con token JWT
    /// - Valida firma e expiration del token
    /// - Esegue sign-in tramite ASP.NET Core Identity
    /// - Supporta [SkipJwtAuthentication] e [AllowAnonymous] con filtraggio per metodo HTTP
    /// - Ritorna 401 Unauthorized se token mancante o invalido
    ///
    /// NON gestisce:
    /// - Validazione claims (responsabilità di JwtClaimsValidationMiddleware)
    /// - Logging richieste (responsabilità di JwtLoggingMiddleware)
    /// </remarks>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IJwtAuthenticationService authService)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (authService == null)
                throw new ArgumentNullException(nameof(authService));

            // Verifica JWT abilitato nella configurazione
            if (!authService.IsJwtEnabled())
            {
                // JWT disabilitato: passa al prossimo middleware senza autenticazione
                await _next(context);
                return;
            }

            if(context.Request.Path.Value.Contains("swagger"))
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

            // Check [SkipJwtAuthentication] - bypass autenticazione per metodi HTTP specifici
            var skipAuthAttr = endpoint?.Metadata?.GetMetadata<SkipJwtAuthenticationAttribute>();
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
                                                                .FirstOrDefault(x => x.AttributeType.Equals(typeof(SkipJwtAuthenticationAttribute))) ?? null;
                if (entitySkipAuthAttr != null)
                {
                    await _next(context);
                    return;
                }
            }

            // Verifica se già autenticato (es. da middleware precedenti)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Tenta autenticazione JWT
            bool authenticated = await authService.TryAuthenticateAsync(context);

            if (!authenticated)
            {
                // Autenticazione fallita: 401 Unauthorized
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Autenticazione riuscita: passa al prossimo middleware
            await _next(context);
        }
    }
}
