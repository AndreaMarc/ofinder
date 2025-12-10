using System;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Factory for creating database analyzers based on engine type.
    /// </summary>
    public static class DatabaseAnalyzerFactory
    {
        /// <summary>
        /// Creates a database analyzer for the specified engine.
        /// </summary>
        /// <param name="engine">Database engine</param>
        /// <returns>Database analyzer instance</returns>
        public static IDatabaseAnalyzer CreateAnalyzer(DatabaseEngine engine)
        {
            return engine switch
            {
                DatabaseEngine.SqlServer => new SqlServerAnalyzer(),
                DatabaseEngine.MySql => new MySqlAnalyzer(),
                _ => throw new NotSupportedException($"Database engine '{engine}' is not supported.")
            };
        }
    }
}
