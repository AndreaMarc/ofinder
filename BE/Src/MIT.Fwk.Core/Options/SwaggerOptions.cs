namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// Swagger/OpenAPI documentation options.
    /// Bind to configuration section "Swagger" in customsettings.json.
    /// </summary>
    public class SwaggerOptions
    {
        /// <summary>
        /// API documentation title
        /// </summary>
        public string Title { get; set; } = "MIT.Fwk Project";

        /// <summary>
        /// API documentation description
        /// </summary>
        public string Description { get; set; } = "MIT.Fwk API Swagger surface";

        /// <summary>
        /// License/copyright information
        /// </summary>
        public string License { get; set; } = "Copyright(c) 2020 - Maestrale Information Technology S.r.l.";

        /// <summary>
        /// Contact email for API support
        /// </summary>
        public string Email { get; set; } = "info@maestrale.com";

        /// <summary>
        /// Company/organization website
        /// </summary>
        public string Website { get; set; } = "http://www.maestrale.com";

        /// <summary>
        /// API owner/maintainer name
        /// </summary>
        public string Owner { get; set; } = "Maestrale Information Technology";
    }
}
