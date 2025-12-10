using System.Collections.Generic;
using System.Threading.Tasks;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Interface for analyzing database schema.
    /// </summary>
    public interface IDatabaseAnalyzer
    {
        /// <summary>
        /// Tests the database connection.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> TestConnectionAsync(string connectionString);

        /// <summary>
        /// Retrieves all table schemas from the database.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>List of table schemas</returns>
        Task<List<TableSchema>> AnalyzeSchemaAsync(string connectionString);

        /// <summary>
        /// Gets the database name from the connection string.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Database name</returns>
        string GetDatabaseName(string connectionString);
    }
}
