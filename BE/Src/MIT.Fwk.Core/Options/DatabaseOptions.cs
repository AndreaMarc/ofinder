namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// Database configuration options.
    /// Provides typed access to database-related settings from configuration.
    /// </summary>
    public class DatabaseOptions
    {
        /// <summary>
        /// Default SQL provider (Sql or MySql)
        /// </summary>
        public string DefaultProvider { get; set; } = "Sql";

        /// <summary>
        /// Default JsonAPI SQL provider (Sql or MySql)
        /// </summary>
        public string DefaultJsonApiProvider { get; set; } = "Sql";

        /// <summary>
        /// Default NoSQL provider (MongoDB, DocumentDB, etc.)
        /// </summary>
        public string NoSQLProvider { get; set; } = "MongoDB";

        /// <summary>
        /// Query timeout in seconds (default: 30)
        /// </summary>
        public int QueryTimeout { get; set; } = 30;

        /// <summary>
        /// AES encryption key for connection string decryption.
        /// Must be 32 characters (256-bit key).
        /// </summary>
        public string EncryptionKey { get; set; } = "2BB2AE87CABD4EFA8DE6CA723411CF7F";
    }
}
