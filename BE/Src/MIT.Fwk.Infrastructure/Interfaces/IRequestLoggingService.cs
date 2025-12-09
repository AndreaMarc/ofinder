using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    /// <summary>
    /// Servizio per gestire il logging delle richieste HTTP verso MongoDB.
    /// Registra richieste, headers, body, user info per audit e debugging.
    /// </summary>
    public interface IRequestLoggingService
    {
        /// <summary>
        /// Logga la richiesta HTTP corrente su MongoDB.
        /// Include path, method, headers, body, user info, IP, ecc.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta da loggare.</param>
        /// <param name="user">Il principal dell'utente autenticato (se disponibile).</param>
        /// <returns>True se il logging è riuscito, False altrimenti.</returns>
        /// <remarks>
        /// Questo metodo NON deve mai lanciare eccezioni che blocchino la pipeline.
        /// Tutti gli errori devono essere gestiti internamente e loggati su file/console.
        /// </remarks>
        Task<bool> LogRequestAsync(HttpContext context, ClaimsPrincipal user);

        /// <summary>
        /// Verifica se la richiesta corrente dovrebbe essere esclusa dal logging.
        /// Controlla header speciali (unitTest, fingerPrint) e altre condizioni.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <returns>True se la richiesta deve essere skippata dal logging, False altrimenti.</returns>
        bool ShouldSkipLogging(HttpContext context);

        /// <summary>
        /// Crea una copia del HttpContext per logging asincrono (fire-and-forget).
        /// Clona request path, method, headers, body per evitare problemi con contesto già disposed.
        /// </summary>
        /// <param name="context">Il contesto HTTP originale da clonare.</param>
        /// <returns>Un nuovo DefaultHttpContext con i dati clonati.</returns>
        Task<DefaultHttpContext> CloneHttpContextAsync(HttpContext context);
    }
}
