using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Middleware
{
    /// <summary>
    /// Middleware per logging delle richieste HTTP su MongoDB.
    /// Responsabilità: Registrare richieste per audit e debugging.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE - Fire-and-Forget & Resilience:
    /// - Logging è asincrono e non-blocking (fire-and-forget)
    /// - Eccezioni nel logging NON bloccano mai la pipeline delle API
    /// - SEMPRE esegue _next(context) alla fine, anche se logging fallisce
    /// - Tutti gli errori loggati su file/console, mai su MongoDB (evita loop)
    ///
    /// Questo middleware:
    /// - Esegue DOPO autenticazione e validazione claims (ha accesso a user context)
    /// - Supporta [SkipRequestLogging] con filtraggio per metodo HTTP
    /// - Controlla header speciali (unitTest, fingerPrint con job sync)
    /// - Clona HttpContext per permettere logging asincrono
    /// - Logga: path, method, headers, body, user info, IP
    ///
    /// NON gestisce:
    /// - Autenticazione JWT (responsabilità di JwtAuthenticationMiddleware)
    /// - Validazione claims (responsabilità di JwtClaimsValidationMiddleware)
    /// </remarks>
    public class JwtLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtLoggingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public JwtLoggingMiddleware(RequestDelegate next, ILogger<JwtLoggingMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public async Task InvokeAsync(HttpContext context, IRequestLoggingService loggingService)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Request.Path.Value.Contains("swagger"))
            {
                await _next(context);
                return;
            }

            bool shouldLog = true;

            try
            {
                var endpoint = context.GetEndpoint();

                try
                {
                    // Check [SkipRequestLogging] - bypass logging per metodi HTTP specifici
                    var skipLoggingAttr = endpoint?.Metadata?.GetMetadata<SkipRequestLoggingAttribute>();
                    if (skipLoggingAttr != null && skipLoggingAttr.AppliesToMethod(context.Request.Method))
                    {
                        shouldLog = false;
                    }

                    var genericTypeAttributes = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>()
                                                .ControllerTypeInfo
                                                .BaseType
                                                .GenericTypeArguments;

                    if (genericTypeAttributes != null && genericTypeAttributes.Length > 0)
                    {
                        var entitySkipAuthAttr = genericTypeAttributes[0]
                                                                        .CustomAttributes
                                                                        .FirstOrDefault(x => x.AttributeType.Equals(typeof(SkipRequestLoggingAttribute))) ?? null;
                        if (entitySkipAuthAttr != null)
                        {
                            await _next(context);
                            return;
                        }
                    }
                }
                catch { }

                // Check header speciali (unitTest, fingerPrint job sync)
                if (shouldLog && loggingService != null && loggingService.ShouldSkipLogging(context))
                {
                    shouldLog = false;
                }

                // Fire-and-forget logging (NON aspettiamo il risultato)
                if (shouldLog && loggingService != null)
                {
                    // Clona context per logging asincrono (evita disposal issues)
                    _ = Task.Run(async () =>
                    {
                        // Crea un nuovo scope per il background task (evita concurrent DbContext access)
                        using var scope = _scopeFactory.CreateScope();
                        var scopedLoggingService = scope.ServiceProvider.GetRequiredService<IRequestLoggingService>();

                        try
                        {
                            // Clone context
                            DefaultHttpContext contextCopy = await scopedLoggingService.CloneHttpContextAsync(context);

                            // Log request asincrono
                            await scopedLoggingService.LogRequestAsync(contextCopy, context.User);
                        }
                        catch (Exception logEx)
                        {
                            // Log failure su file/console, NON su MongoDB (evita loop)
                            _logger.LogError(logEx, "Failed to log request to MongoDB - Path: {Path}, Method: {Method}",
                                context.Request.Path, context.Request.Method);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Log exception ma NON bloccare pipeline
                _logger.LogError(ex, "JwtLoggingMiddleware exception - continuing pipeline - Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);
            }

            // SEMPRE esegue next middleware (anche se logging fallisce)
            // Questo garantisce che problemi di logging non blocchino mai le API
            await _next(context);
        }
    }
}
