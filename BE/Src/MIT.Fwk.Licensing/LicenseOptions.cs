#nullable enable

namespace MIT.Fwk.Licensing
{
    /// <summary>
    /// Configuration options for license service
    /// </summary>
    public class LicenseOptions
    {
        /// <summary>
        /// Path to the license file. If null, will be auto-detected from assembly location.
        /// </summary>
        public string? LicenseFilePath { get; set; }

        /// <summary>
        /// Primary encryption key resource name embedded in assembly
        /// </summary>
        public string PrimaryKeyResource { get; set; } = "MIT.Fwk.Licensing.License.txt";

        /// <summary>
        /// Fallback encryption key resource name (for backward compatibility)
        /// </summary>
        public string FallbackKeyResource { get; set; } = "MIT.Fwk.Licensing.Resource.resources";

        /// <summary>
        /// Trial encryption key resource name
        /// </summary>
        public string TrialKeyResource { get; set; } = "MIT.Fwk.License.Resource.resources";
    }
}
