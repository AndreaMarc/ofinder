using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    /// <summary>
    /// Servizio per gestire la validazione dei claims JWT per autorizzazione granulare.
    /// Verifica permessi utente su entity specifiche (es. "Invoice.Read", "Customer.Write").
    /// </summary>
    public interface IJwtClaimsService
    {
        /// <summary>
        /// Verifica se l'utente ha il claim necessario per accedere all'entity specificata con il metodo HTTP richiesto.
        /// </summary>
        /// <param name="username">Username o email dell'utente da verificare.</param>
        /// <param name="entityName">Nome dell'entity (es. "Invoice", "Customer").</param>
        /// <param name="httpMethod">Metodo HTTP della richiesta (GET, POST, PUT, PATCH, DELETE).</param>
        /// <param name="tenantId">ID del tenant per multi-tenancy.</param>
        /// <returns>True se l'utente ha il claim richiesto, False altrimenti.</returns>
        Task<bool> HasClaimAsync(string username, string entityName, string httpMethod, string tenantId);

        /// <summary>
        /// Ottiene l'elenco completo dei claims dell'utente per il tenant specificato.
        /// Claims pool contiene permessi come "Invoice.Read", "Customer.Write", ecc.
        /// </summary>
        /// <param name="username">Username o email dell'utente.</param>
        /// <param name="tenantId">ID del tenant per multi-tenancy.</param>
        /// <returns>Lista di claims in formato "EntityName.Action".</returns>
        Task<List<string>> GetUserClaimsPoolAsync(string username, string tenantId);

        /// <summary>
        /// Filtra il parametro "include" della query JsonAPI basandosi sui claims dell'utente.
        /// Rimuove dal query string le relazioni per cui l'utente non ha permessi di lettura.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <param name="userClaims">Lista dei claims dell'utente.</param>
        /// <remarks>
        /// Modifica direttamente context.Request.QueryString rimuovendo include non autorizzati.
        /// JsonAPI standard: ?include=comments,author,tags
        /// Dopo filtering: ?include=comments (se utente non ha permessi su author e tags)
        /// </remarks>
        void FilterIncludesByClaimsPool(HttpContext context, List<string> userClaims);

        /// <summary>
        /// Verifica se l'utente è superadmin (bypass completo di claims validation).
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <returns>True se l'utente ha claim "superadmin" = "True", False altrimenti.</returns>
        bool IsSuperAdmin(HttpContext context);
    }
}
