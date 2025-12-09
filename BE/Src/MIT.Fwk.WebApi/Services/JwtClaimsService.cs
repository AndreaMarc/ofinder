using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Implementazione del servizio di validazione claims JWT.
    /// Gestisce autorizzazione granulare basata su permessi utente per entity specifiche.
    /// </summary>
    public class JwtClaimsService : IJwtClaimsService
    {
        private readonly IJsonApiManualService _jsonApiManualService;

        public JwtClaimsService(IJsonApiManualService jsonApiManualService)
        {
            _jsonApiManualService = jsonApiManualService ?? throw new ArgumentNullException(nameof(jsonApiManualService));
        }

        /// <summary>
        /// Verifica se l'utente ha il claim necessario per l'entity e metodo HTTP specificati.
        /// Logica estratta da JwtSignInManager.CheckClaim().
        /// </summary>
        public async Task<bool> HasClaimAsync(string username, string entityName, string httpMethod, string tenantId)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(entityName))
                return false;

            // Mappa metodo HTTP a suffisso claim (GET -> .read, POST -> .create, ecc.)
            string claimSuffix = httpMethod?.ToUpperInvariant() switch
            {
                "GET" => ".read",
                "POST" => ".create",
                "PATCH" => ".update",
                "DELETE" => ".delete",
                "PUT" => ".update", // PUT trattato come UPDATE
                _ => null
            };

            if (claimSuffix == null)
                return false;

            string fullClaimName = entityName + claimSuffix;

            // Verifica claim nel database tramite JsonApiManualService
            return await _jsonApiManualService.ExistClaim(username, fullClaimName, tenantId);
        }

        /// <summary>
        /// Ottiene il pool completo di claims dell'utente per il tenant.
        /// Logica estratta da JwtSignInManager.GetClaimsPool().
        /// </summary>
        public Task<List<string>> GetUserClaimsPoolAsync(string username, string tenantId)
        {
            if (string.IsNullOrEmpty(username))
                return Task.FromResult<List<string>>(null);

            // Recupera claims da database
            List<string> claimsPool = _jsonApiManualService.GetClaimsPoolByUsername(username, tenantId);
            return Task.FromResult(claimsPool);
        }

        /// <summary>
        /// Filtra il parametro "include" della query JsonAPI basandosi sui claims dell'utente.
        /// Rimuove relazioni per cui l'utente non ha permessi di lettura.
        /// Logica estratta da JwtAuthentication.CheckInclude().
        /// </summary>
        public void FilterIncludesByClaimsPool(HttpContext context, List<string> userClaims)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (userClaims == null || userClaims.Count == 0)
            {
                // Nessun claim: rimuovi tutto il parametro include
                if (context.Request.QueryString.HasValue)
                {
                    var emptyQueryDict = QueryHelpers.ParseQuery(context.Request.QueryString.Value);
                    emptyQueryDict.Remove("include");

                    if (emptyQueryDict.Count > 0)
                    {
                        var emptyQueryString = QueryString.Create(
                            emptyQueryDict.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value)));
                        context.Request.QueryString = emptyQueryString;
                    }
                    else
                    {
                        context.Request.QueryString = QueryString.Empty;
                    }
                }
                return;
            }

            // Estrai query string
            string originalQuery = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : string.Empty;

            if (string.IsNullOrWhiteSpace(originalQuery))
                return;

            // Parse query in dizionario
            Dictionary<string, StringValues> queryDictionary = QueryHelpers.ParseQuery(originalQuery);

            // Controlla se esiste parametro "include"
            if (!queryDictionary.TryGetValue("include", out StringValues includeValues))
                return;

            // Parse include: "comments,author,tags" -> ["comments", "author", "tags"]
            string includeStr = includeValues.ToString();
            List<string> includesList = includeStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .ToList();

            // Estrai solo la parte root delle relazioni (prima del punto)
            // Es: "author.profile.picture" -> "author"
            includesList = includesList
                .Select(x => x.Split('.')[0])
                .Distinct()
                .ToList();

            // Filtra: mantieni solo relazioni per cui l'utente ha claim .read
            IEnumerable<string> claimsPoolLower = userClaims.Select(x => x.ToLowerInvariant());
            List<string> allowedIncludes = includesList
                .Where(rel =>
                {
                    // Check claim sia singolare che plurale: "comment.read" o "comments.read"
                    string singular = rel.ToLowerInvariant().Singularize(false);
                    string claimSingular = $"{singular}.read";
                    string claimPlural = $"{rel.ToLowerInvariant()}.read";

                    return claimsPoolLower.Contains(claimSingular) || claimsPoolLower.Contains(claimPlural);
                })
                .ToList();

            // Ricostruisci query string
            if (allowedIncludes.Count > 0)
            {
                // Filtra anche gli include nested (author.profile -> solo se "author" è allowed)
                IEnumerable<string> originalIncludesFiltered = includeValues.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .Where(x => allowedIncludes.Contains(x.Split('.')[0]));

                queryDictionary["include"] = string.Join(",", originalIncludesFiltered);
            }
            else
            {
                // Nessun include autorizzato: rimuovi parametro
                queryDictionary.Remove("include");
            }

            // Applica nuova query string
            QueryString newQueryString = QueryString.Create(
                queryDictionary.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value)));

            context.Request.QueryString = newQueryString;
        }

        /// <summary>
        /// Verifica se l'utente è superadmin (bypass completo claims validation).
        /// </summary>
        public bool IsSuperAdmin(HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return false;

            return context.User.Claims
                .FirstOrDefault(x => x.Type == "superadmin")?.Value == "True";
        }
    }
}
