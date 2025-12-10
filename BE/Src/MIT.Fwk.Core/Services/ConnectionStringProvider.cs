using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using System;

namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Implementation of IConnectionStringProvider.
    /// Provides connection strings with automatic AES decryption support.
    /// </summary>
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseOptions _databaseOptions;
        private readonly IEncryptionService _encryptionService;

        public ConnectionStringProvider(IConfiguration configuration, IOptions<DatabaseOptions> databaseOptions, IEncryptionService encryptionService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _databaseOptions = databaseOptions?.Value ?? throw new ArgumentNullException(nameof(databaseOptions));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public string ConnectionString
        {
            get
            {
                string connStr = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connStr))
                {
                    throw new InvalidOperationException("DefaultConnection not configured in dbconnections.json");
                }

                return TryDecrypt(connStr);
            }
        }

        public string NoSQLConnectionString
        {
            get
            {
                // Try NoSQLConnection first
                string connStr = _configuration.GetConnectionString("NoSQLConnection");

                if (!string.IsNullOrEmpty(connStr))
                {
                    return TryDecrypt(connStr);
                }

                // Fallback to DocumentDBConnection (backward compatibility)
                connStr = _configuration.GetConnectionString("DocumentDBConnection");

                if (!string.IsNullOrEmpty(connStr))
                {
                    return TryDecrypt(connStr);
                }

                // Final fallback to default connection
                return ConnectionString;
            }
        }

        public string GetConnectionString(string name, bool fallbackToNoSql = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Connection string name cannot be null or empty", nameof(name));
            }

            string connStr = _configuration.GetConnectionString(name);

            if (!string.IsNullOrEmpty(connStr))
            {
                return TryDecrypt(connStr);
            }

            // Fallback logic
            return fallbackToNoSql ? NoSQLConnectionString : ConnectionString;
        }

        /// <summary>
        /// Attempts to decrypt a connection string using AES encryption key.
        /// If decryption fails (string not encrypted), returns original string.
        /// </summary>
        private string TryDecrypt(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            try
            {
                return _encryptionService.DecryptString(connectionString, _databaseOptions.EncryptionKey);
            }
            catch
            {
                // Not encrypted or decryption failed - return as-is
                return connectionString;
            }
        }
    }
}
