using System;

namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// JWT authentication configuration options.
    /// Maps to "Authentication:Jwt" section in customsettings.json
    /// </summary>
    public class JwtOptions
    {
        public bool Enabled { get; set; } = true;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int AccessTokenExpiresIn { get; set; } = 480;  // minutes
        public int RefreshTokenExpiresIn { get; set; } = 1440;  // minutes

        /// <summary>
        /// [DEPRECATED] Route exceptions configuration is now handled via [SkipJwtAuthentication] attribute.
        /// Apply the attribute to controllers or entities instead of using this string-based configuration.
        /// This property will be removed in v9.0.
        /// </summary>
        [Obsolete("Use [SkipJwtAuthentication] attribute on controllers/entities instead. This property will be removed in v9.0.", false)]
        public string RoutesExceptions { get; set; } = string.Empty;

        /// <summary>
        /// [DEPRECATED] Routes without claims configuration is now handled via [SkipClaimsValidation] attribute.
        /// Apply the attribute to controllers or entities instead of using this string-based configuration.
        /// This property will be removed in v9.0.
        /// </summary>
        [Obsolete("Use [SkipClaimsValidation] attribute on controllers/entities instead. This property will be removed in v9.0.", false)]
        public string RoutesWithoutClaims { get; set; } = string.Empty;

        /// <summary>
        /// [DEPRECATED] Routes without log configuration is now handled via [SkipRequestLogging] attribute.
        /// Apply the attribute to controllers instead of using this string-based configuration.
        /// This property will be removed in v9.0.
        /// </summary>
        [Obsolete("Use [SkipRequestLogging] attribute on controllers instead. This property will be removed in v9.0.", false)]
        public string RoutesWithoutLog { get; set; } = string.Empty;
    }
}
