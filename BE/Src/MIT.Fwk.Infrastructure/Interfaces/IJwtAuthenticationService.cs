using Microsoft.AspNetCore.Http;
using MIT.Fwk.Core.Models;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    /// <summary>
    /// Servizio per gestire l'autenticazione JWT.
    /// Estrae e valida token JWT, esegue sign-in degli utenti.
    /// </summary>
    public interface IJwtAuthenticationService
    {
        /// <summary>
        /// Tenta di autenticare l'utente corrente utilizzando il token JWT nell'header Authorization.
        /// Esegue sign-in tramite ASP.NET Core Identity se il token è valido.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <returns>True se l'autenticazione è riuscita, False altrimenti.</returns>
        Task<bool> TryAuthenticateAsync(HttpContext context);

        /// <summary>
        /// Valida un refresh token e, se valido, esegue sign-in dell'utente associato.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <param name="deviceHash">Hash del dispositivo per validazione refresh token.</param>
        /// <returns>True se il refresh token è valido e sign-in è riuscito, False altrimenti.</returns>
        Task<bool> ValidateRefreshTokenAsync(HttpContext context, string deviceHash);

        /// <summary>
        /// Estrae e valida l'header Authorization dalla richiesta HTTP.
        /// </summary>
        /// <param name="context">Il contesto HTTP della richiesta corrente.</param>
        /// <returns>Un oggetto JwtAuthenticationHeaderValue con il token parsato e validato.</returns>
        JwtAuthenticationHeaderValue ParseAuthorizationHeader(HttpContext context);

        /// <summary>
        /// Verifica se il JWT è abilitato nella configurazione.
        /// </summary>
        /// <returns>True se JWT è abilitato, False altrimenti.</returns>
        bool IsJwtEnabled();
    }
}
