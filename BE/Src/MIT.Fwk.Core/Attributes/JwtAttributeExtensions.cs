using System;

namespace MIT.Fwk.Core.Attributes
{
    /// <summary>
    /// Extension methods per validare se gli attributi JWT si applicano al metodo HTTP corrente.
    /// </summary>
    public static class JwtAttributeExtensions
    {
        /// <summary>
        /// Verifica se l'attributo SkipJwtAuthentication si applica al metodo HTTP specificato.
        /// </summary>
        /// <param name="attr">L'attributo da verificare.</param>
        /// <param name="httpMethod">Il metodo HTTP della richiesta (GET, POST, PUT, PATCH, DELETE).</param>
        /// <returns>True se l'attributo si applica al metodo HTTP, False altrimenti.</returns>
        /// <example>
        /// <code>
        /// var attr = endpoint?.Metadata?.GetMetadata&lt;SkipJwtAuthenticationAttribute&gt;();
        /// if (attr?.AppliesToMethod(context.Request.Method) == true)
        /// {
        ///     // Skip autenticazione per questo metodo HTTP
        /// }
        /// </code>
        /// </example>
        public static bool AppliesToMethod(this SkipJwtAuthenticationAttribute attr, string httpMethod)
        {
            if (attr == null || string.IsNullOrEmpty(httpMethod))
                return false;

            return httpMethod.ToUpperInvariant() switch
            {
                "GET" => attr.Methods.HasFlag(JwtHttpMethod.GET),
                "POST" => attr.Methods.HasFlag(JwtHttpMethod.POST),
                "PUT" => attr.Methods.HasFlag(JwtHttpMethod.PUT),
                "PATCH" => attr.Methods.HasFlag(JwtHttpMethod.PATCH),
                "DELETE" => attr.Methods.HasFlag(JwtHttpMethod.DELETE),
                _ => false // Metodo HTTP non supportato
            };
        }

        /// <summary>
        /// Verifica se l'attributo SkipClaimsValidation si applica al metodo HTTP specificato.
        /// </summary>
        /// <param name="attr">L'attributo da verificare.</param>
        /// <param name="httpMethod">Il metodo HTTP della richiesta (GET, POST, PUT, PATCH, DELETE).</param>
        /// <returns>True se l'attributo si applica al metodo HTTP, False altrimenti.</returns>
        /// <example>
        /// <code>
        /// var attr = entityType?.GetCustomAttribute&lt;SkipClaimsValidationAttribute&gt;();
        /// if (attr?.AppliesToMethod(context.Request.Method) == true)
        /// {
        ///     // Skip validazione claims per questo metodo HTTP
        /// }
        /// </code>
        /// </example>
        public static bool AppliesToMethod(this SkipClaimsValidationAttribute attr, string httpMethod)
        {
            if (attr == null || string.IsNullOrEmpty(httpMethod))
                return false;

            return httpMethod.ToUpperInvariant() switch
            {
                "GET" => attr.Methods.HasFlag(JwtHttpMethod.GET),
                "POST" => attr.Methods.HasFlag(JwtHttpMethod.POST),
                "PUT" => attr.Methods.HasFlag(JwtHttpMethod.PUT),
                "PATCH" => attr.Methods.HasFlag(JwtHttpMethod.PATCH),
                "DELETE" => attr.Methods.HasFlag(JwtHttpMethod.DELETE),
                _ => false // Metodo HTTP non supportato
            };
        }

        /// <summary>
        /// Verifica se l'attributo SkipRequestLogging si applica al metodo HTTP specificato.
        /// </summary>
        /// <param name="attr">L'attributo da verificare.</param>
        /// <param name="httpMethod">Il metodo HTTP della richiesta (GET, POST, PUT, PATCH, DELETE).</param>
        /// <returns>True se l'attributo si applica al metodo HTTP, False altrimenti.</returns>
        /// <example>
        /// <code>
        /// var attr = endpoint?.Metadata?.GetMetadata&lt;SkipRequestLoggingAttribute&gt;();
        /// if (attr?.AppliesToMethod(context.Request.Method) == true)
        /// {
        ///     // Skip logging per questo metodo HTTP
        /// }
        /// </code>
        /// </example>
        public static bool AppliesToMethod(this SkipRequestLoggingAttribute attr, string httpMethod)
        {
            if (attr == null || string.IsNullOrEmpty(httpMethod))
                return false;

            return httpMethod.ToUpperInvariant() switch
            {
                "GET" => attr.Methods.HasFlag(JwtHttpMethod.GET),
                "POST" => attr.Methods.HasFlag(JwtHttpMethod.POST),
                "PUT" => attr.Methods.HasFlag(JwtHttpMethod.PUT),
                "PATCH" => attr.Methods.HasFlag(JwtHttpMethod.PATCH),
                "DELETE" => attr.Methods.HasFlag(JwtHttpMethod.DELETE),
                _ => false // Metodo HTTP non supportato
            };
        }

        /// <summary>
        /// Converte una stringa del metodo HTTP nell'enum JwtHttpMethod corrispondente.
        /// </summary>
        /// <param name="httpMethod">Il metodo HTTP come stringa (GET, POST, PUT, PATCH, DELETE).</param>
        /// <returns>L'enum JwtHttpMethod corrispondente, o JwtHttpMethod.None se non riconosciuto.</returns>
        public static JwtHttpMethod ToJwtHttpMethod(this string httpMethod)
        {
            if (string.IsNullOrEmpty(httpMethod))
                return JwtHttpMethod.None;

            return httpMethod.ToUpperInvariant() switch
            {
                "GET" => JwtHttpMethod.GET,
                "POST" => JwtHttpMethod.POST,
                "PUT" => JwtHttpMethod.PUT,
                "PATCH" => JwtHttpMethod.PATCH,
                "DELETE" => JwtHttpMethod.DELETE,
                _ => JwtHttpMethod.None
            };
        }

        /// <summary>
        /// Verifica se un metodo HTTP (come stringa) corrisponde a uno dei flag nell'enum JwtHttpMethod.
        /// </summary>
        /// <param name="methods">I flag dei metodi HTTP da verificare.</param>
        /// <param name="httpMethod">Il metodo HTTP della richiesta come stringa.</param>
        /// <returns>True se il metodo HTTP è incluso nei flag, False altrimenti.</returns>
        /// <example>
        /// <code>
        /// JwtHttpMethod allowedMethods = JwtHttpMethod.GET | JwtHttpMethod.POST;
        /// if (allowedMethods.Includes("GET")) // Returns true
        /// {
        ///     // Metodo GET è consentito
        /// }
        /// </code>
        /// </example>
        public static bool Includes(this JwtHttpMethod methods, string httpMethod)
        {
            if (string.IsNullOrEmpty(httpMethod))
                return false;

            JwtHttpMethod requestMethod = httpMethod.ToJwtHttpMethod();
            return requestMethod != JwtHttpMethod.None && methods.HasFlag(requestMethod);
        }
    }
}
