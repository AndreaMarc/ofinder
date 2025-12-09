namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Provides connection strings with optional decryption support.
    /// Replaces static ConfigurationHelper methods with dependency injection pattern.
    /// </summary>
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// Gets the default connection string (DefaultConnection).
        /// Automatically decrypts if encrypted with AES encryption key.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the NoSQL/MongoDB connection string.
        /// Falls back to DocumentDBConnection if NoSQLConnection not configured.
        /// </summary>
        string NoSQLConnectionString { get; }

        /// <summary>
        /// Gets a custom connection string by name (e.g., DbContext class name).
        /// Automatically decrypts if encrypted.
        /// </summary>
        /// <param name="name">Connection string name from dbconnections.json</param>
        /// <param name="fallbackToNoSql">If true and not found, return NoSQL connection; otherwise return DefaultConnection</param>
        /// <returns>Connection string (decrypted if necessary)</returns>
        string GetConnectionString(string name, bool fallbackToNoSql = false);
    }
}
