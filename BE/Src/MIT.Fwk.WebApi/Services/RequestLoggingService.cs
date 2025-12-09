using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Implementazione del servizio di logging delle richieste HTTP.
    /// Logga richieste su MongoDB per audit e debugging.
    /// Logica estratta da JwtSignInManager.LogToMongo().
    /// </summary>
    public class RequestLoggingService : IRequestLoggingService
    {
        private readonly ILogService _logService;
        private readonly IJsonApiManualService _jsonApiManualService;

        public RequestLoggingService(ILogService logService, IJsonApiManualService jsonApiManualService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _jsonApiManualService = jsonApiManualService ?? throw new ArgumentNullException(nameof(jsonApiManualService));
        }

        /// <summary>
        /// Logga la richiesta HTTP su MongoDB.
        /// Logica estratta da JwtSignInManager.LogToMongo().
        /// </summary>
        public async Task<bool> LogRequestAsync(HttpContext context, ClaimsPrincipal user)
        {
            if (context == null)
                return false;

            try
            {
                // Estrai user info se autenticato
                MITApplicationUser currentUser = null;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    string userEmail = user.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        User userEntity = await _jsonApiManualService.GetUserByEmail(userEmail);
                        if (userEntity != null)
                        {
                            currentUser = new MITApplicationUser
                            {
                                Id = userEntity.Id,
                                Email = userEntity.Email,
                                UserName = userEntity.UserName,
                                LastAccess = userEntity.LastAccess,
                                LastIp = userEntity.LastIp
                            };
                        }
                    }
                }

                // Estrai route path con query string
                string routePath = context.Request.Path + context.Request.QueryString;

                // Serializza headers
                string headers = Newtonsoft.Json.JsonConvert.SerializeObject(context.Request.Headers.ToList());

                // Leggi body (se disponibile)
                string payload = "";
                if (context.Request.Body != null && context.Request.Body.CanRead)
                {
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                    using (StreamReader reader = new(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        payload = await reader.ReadToEndAsync();
                    }
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                // Wrap in array se non è già un array JSON
                if (!string.IsNullOrEmpty(payload) && !payload.StartsWith('['))
                {
                    payload = $"[{payload}]";
                }

                // Crea log object
                LogToMongo logToMongo = new()
                {
                    Model = null,
                    CurrentUser = currentUser,
                    RoutePath = routePath,
                    Headers = headers,
                    PayLoad = payload,
                    RequestMethod = context.Request.Method,
                    LogType = "Middleware"
                };

                // Log via bus (ForMongo usa MediatR)
                _logService.ForMongo(Newtonsoft.Json.JsonConvert.SerializeObject(logToMongo));

                return true;
            }
            catch (Exception ex)
            {
                // NON propagare eccezione - logga solo su file/console
                _logService.Error($"RequestLoggingService.LogRequestAsync failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica se la richiesta deve essere skippata dal logging.
        /// Controlla header speciali (unitTest, fingerPrint con job sync).
        /// Logica estratta da JwtAuthentication.cs (righe 45-64).
        /// </summary>
        public bool ShouldSkipLogging(HttpContext context)
        {
            if (context == null)
                return false;

            // Check header "unitTest: True" - usato nei test automatici
            if (context.Request.Headers.TryGetValue("unitTest", out StringValues testNoLog) &&
                testNoLog == "True")
            {
                return true;
            }

            // Check fingerPrint header per job sincronizzazione alyante
            if (context.Request.Headers.TryGetValue("fingerPrint", out StringValues deviceHashLog) &&
                deviceHashLog.ToString().Contains("job sincronizzazione alyante", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clona il HttpContext per logging asincrono.
        /// Copia request path, method, headers, body per evitare disposal issues.
        /// Logica estratta da JwtAuthentication.cs (righe 93-130).
        /// </summary>
        public async Task<DefaultHttpContext> CloneHttpContextAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            DefaultHttpContext contextCopy = new();

            // Copia request metadata
            contextCopy.Request.Scheme = context.Request.Scheme;
            contextCopy.Request.Host = context.Request.Host;
            contextCopy.Request.Path = context.Request.Path;
            contextCopy.Request.PathBase = context.Request.PathBase;
            contextCopy.Request.Method = context.Request.Method;
            contextCopy.Request.QueryString = context.Request.QueryString;
            contextCopy.Request.ContentType = context.Request.ContentType;
            contextCopy.Request.ContentLength = context.Request.ContentLength;

            // Copia headers
            foreach (KeyValuePair<string, StringValues> header in context.Request.Headers)
            {
                contextCopy.Request.Headers[header.Key] = header.Value;
            }

            // Copia body in MemoryStream (per permettere multiple letture)
            if (context.Request.Body != null && context.Request.Body.CanRead)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                MemoryStream buffer = new();
                await context.Request.Body.CopyToAsync(buffer);
                buffer.Seek(0, SeekOrigin.Begin);
                contextCopy.Request.Body = buffer;

                // Reset original body position
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            // Copia service provider per accesso a DI
            contextCopy.RequestServices = context.RequestServices;

            // Copia user principal se autenticato
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                contextCopy.User = context.User;
            }

            return contextCopy;
        }
    }
}
