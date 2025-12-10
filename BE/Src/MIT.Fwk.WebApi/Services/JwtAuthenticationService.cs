using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.WebApi.Extension; // For JwtSignInManager
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Services
{
    /// <summary>
    /// Implementazione del servizio di autenticazione JWT.
    /// Gestisce validazione token e sign-in utenti tramite JwtSignInManager.
    /// </summary>
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        /// <summary>
        /// Tenta di autenticare l'utente utilizzando il token JWT.
        /// </summary>
        public async Task<bool> TryAuthenticateAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Verifica JWT abilitato
            if (!JwtTokenProvider.Enabled)
                return false;

            // Verifica che non sia già autenticato
            if (context.User.Identity?.IsAuthenticated == true)
                return true;

            // Verifica header Authorization
            if (!context.Request.Headers.ContainsKey("Authorization"))
                return false;

            // Parse e valida header Authorization
            JwtAuthenticationHeaderValue authenticationHeader = ParseAuthorizationHeader(context);
            if (!authenticationHeader.IsValid)
                return false;

            // Estrae headers per validazione
            StringValues deviceHash = context.Request.Headers.TryGetValue("fingerPrint", out var dh) ? dh : StringValues.Empty;
            StringValues tenantId = context.Request.Headers.TryGetValue("tenantId", out var tid) ? tid : StringValues.Empty;

            // Crea sign-in manager e tenta autenticazione
            JwtSignInManager authenticationManager = new(context, authenticationHeader);

            // Prima prova con token normale, poi con refresh token
            bool authenticated = await authenticationManager.TrySignInUser(deviceHash.ToString(), tenantId.ToString())
                              || await authenticationManager.TrySignInRefresh(deviceHash.ToString());

            return authenticated;
        }

        /// <summary>
        /// Valida un refresh token.
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(HttpContext context, string deviceHash)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!JwtTokenProvider.Enabled)
                return false;

            JwtAuthenticationHeaderValue authenticationHeader = ParseAuthorizationHeader(context);
            if (!authenticationHeader.IsValid)
                return false;

            JwtSignInManager authenticationManager = new(context, authenticationHeader);
            return await authenticationManager.TrySignInRefresh(deviceHash);
        }

        /// <summary>
        /// Estrae e valida l'header Authorization.
        /// </summary>
        public JwtAuthenticationHeaderValue ParseAuthorizationHeader(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string authorizationHeader = context.Request.Headers["Authorization"]
                .FirstOrDefault(header => header?.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) == true);

            return new JwtAuthenticationHeaderValue(authorizationHeader);
        }

        /// <summary>
        /// Verifica se JWT è abilitato.
        /// </summary>
        public bool IsJwtEnabled()
        {
            return JwtTokenProvider.Enabled;
        }
    }
}
