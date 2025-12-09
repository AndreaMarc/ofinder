namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// SMTP configuration options for email sending.
    /// Bind to configuration section "SMTP" in appsettings.json or customsettings.json.
    /// </summary>
    public class SmtpOptions
    {
        /// <summary>
        /// Enable/disable email sending. If false, SendMail will return null.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// SMTP server hostname (e.g., smtp.gmail.com)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP server port (e.g., 587 for TLS, 465 for SSL)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// SMTP username (can be encrypted with AES encryption key)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// SMTP password (can be encrypted with AES encryption key)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Display name for sender (e.g., "My Application")
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Enable SSL/TLS encryption (default: true)
        /// </summary>
        public bool EnableSSL { get; set; } = true;

        /// <summary>
        /// Ignore SSL certificate validation errors (for self-signed certs). Use with caution.
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; }
    }
}
