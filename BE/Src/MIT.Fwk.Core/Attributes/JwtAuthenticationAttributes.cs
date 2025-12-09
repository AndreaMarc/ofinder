using System;

namespace MIT.Fwk.Core.Attributes
{
    /// <summary>
    /// Metodi HTTP supportati per filtraggio attributi JWT.
    /// Usare come flags per specificare più metodi contemporaneamente.
    /// </summary>
    [Flags]
    public enum JwtHttpMethod
    {
        /// <summary>
        /// Nessun metodo HTTP.
        /// </summary>
        None = 0,

        /// <summary>
        /// Metodo HTTP GET.
        /// </summary>
        GET = 1,

        /// <summary>
        /// Metodo HTTP POST.
        /// </summary>
        POST = 2,

        /// <summary>
        /// Metodo HTTP PUT.
        /// </summary>
        PUT = 4,

        /// <summary>
        /// Metodo HTTP PATCH.
        /// </summary>
        PATCH = 8,

        /// <summary>
        /// Metodo HTTP DELETE.
        /// </summary>
        DELETE = 16,

        /// <summary>
        /// Tutti i metodi HTTP (GET | POST | PUT | PATCH | DELETE).
        /// </summary>
        All = GET | POST | PUT | PATCH | DELETE
    }

    /// <summary>
    /// Marca controller o action da escludere dall'autenticazione JWT.
    /// Usare per endpoint pubblici come /login, /register, /swagger.
    /// </summary>
    /// <example>
    /// <code>
    /// // Skip autenticazione per tutto il controller
    /// [SkipJwtAuthentication]
    /// public class AccountController : ControllerBase { }
    ///
    /// // Skip autenticazione solo per GET
    /// [HttpGet]
    /// [SkipJwtAuthentication(JwtHttpMethod.GET)]
    /// public IActionResult GetPublicData() { }
    ///
    /// // Skip autenticazione per GET e POST
    /// [SkipJwtAuthentication(JwtHttpMethod.GET | JwtHttpMethod.POST)]
    /// public class PublicController : ControllerBase { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SkipJwtAuthenticationAttribute : Attribute
    {
        /// <summary>
        /// Metodi HTTP per cui vale lo skip dell'autenticazione (default: All).
        /// </summary>
        public JwtHttpMethod Methods { get; set; } = JwtHttpMethod.All;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipJwtAuthenticationAttribute"/>.
        /// Lo skip vale per tutti i metodi HTTP.
        /// </summary>
        public SkipJwtAuthenticationAttribute()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipJwtAuthenticationAttribute"/>.
        /// </summary>
        /// <param name="methods">Metodi HTTP per cui vale lo skip.</param>
        public SkipJwtAuthenticationAttribute(JwtHttpMethod methods)
        {
            Methods = methods;
        }
    }

    /// <summary>
    /// Marca controller, action o entity JsonAPI da escludere dalla validazione claims.
    /// Usare per endpoint che richiedono autenticazione ma non claims specifici (es. /health, /config).
    /// Per entity JsonAPI: usare per entity pubbliche come Setup, Translation che non richiedono permessi specifici.
    /// </summary>
    /// <remarks>
    /// DEFAULT per entity JsonAPI [Resource]: TUTTE richiedono validazione claims.
    /// Applicare questo attributo solo per eccezioni (entity pubbliche, configurazioni, ecc.).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Entity JsonAPI - skip claims solo per GET (lettura pubblica, scrittura protetta)
    /// [Resource]
    /// [SkipClaimsValidation(JwtHttpMethod.GET)]
    /// public class Setup : Identifiable&lt;int&gt; { }
    ///
    /// // Controller - skip claims per tutti i metodi
    /// [SkipClaimsValidation]
    /// public class HealthController : ControllerBase { }
    ///
    /// // Action - skip claims per POST e PATCH
    /// [HttpPost]
    /// [SkipClaimsValidation(JwtHttpMethod.POST | JwtHttpMethod.PATCH)]
    /// public IActionResult SpecialEndpoint() { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SkipClaimsValidationAttribute : Attribute
    {
        /// <summary>
        /// Metodi HTTP per cui vale lo skip della validazione claims (default: All).
        /// </summary>
        public JwtHttpMethod Methods { get; set; } = JwtHttpMethod.All;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipClaimsValidationAttribute"/>.
        /// Lo skip vale per tutti i metodi HTTP.
        /// </summary>
        public SkipClaimsValidationAttribute()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipClaimsValidationAttribute"/>.
        /// </summary>
        /// <param name="methods">Metodi HTTP per cui vale lo skip.</param>
        public SkipClaimsValidationAttribute(JwtHttpMethod methods)
        {
            Methods = methods;
        }
    }

    /// <summary>
    /// Marca controller o action da escludere dal logging HTTP delle richieste.
    /// Usare per endpoint come /log stesso, endpoint ad alta frequenza, o per questioni di privacy.
    /// </summary>
    /// <example>
    /// <code>
    /// // Controller - skip logging per tutti i metodi
    /// [SkipRequestLogging]
    /// public class LogController : ControllerBase { }
    ///
    /// // Action - skip logging solo per GET
    /// [HttpGet]
    /// [SkipRequestLogging(JwtHttpMethod.GET)]
    /// public IActionResult HighFrequencyEndpoint() { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SkipRequestLoggingAttribute : Attribute
    {
        /// <summary>
        /// Metodi HTTP per cui vale lo skip del logging (default: All).
        /// </summary>
        public JwtHttpMethod Methods { get; set; } = JwtHttpMethod.All;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipRequestLoggingAttribute"/>.
        /// Lo skip vale per tutti i metodi HTTP.
        /// </summary>
        public SkipRequestLoggingAttribute()
        {
        }

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="SkipRequestLoggingAttribute"/>.
        /// </summary>
        /// <param name="methods">Metodi HTTP per cui vale lo skip.</param>
        public SkipRequestLoggingAttribute(JwtHttpMethod methods)
        {
            Methods = methods;
        }
    }
}
