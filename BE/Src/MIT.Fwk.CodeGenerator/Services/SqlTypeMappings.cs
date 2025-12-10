using System;
using System.Collections.Generic;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Helper class for mapping SQL data types to C# types.
    /// </summary>
    public static class SqlTypeMappings
    {
        /// <summary>
        /// Maps SQL Server data types to C# types.
        /// </summary>
        private static readonly Dictionary<string, string> SqlServerToCSharp = new(StringComparer.OrdinalIgnoreCase)
        {
            // Integer types
            { "tinyint", "byte" },
            { "smallint", "short" },
            { "int", "int" },
            { "bigint", "long" },

            // Decimal types
            { "decimal", "decimal" },
            { "numeric", "decimal" },
            { "money", "decimal" },
            { "smallmoney", "decimal" },
            { "float", "double" },
            { "real", "float" },

            // String types
            { "char", "string" },
            { "varchar", "string" },
            { "text", "string" },
            { "nchar", "string" },
            { "nvarchar", "string" },
            { "ntext", "string" },

            // Date/Time types
            { "datetime", "DateTime" },
            { "datetime2", "DateTime" },
            { "date", "DateTime" },
            { "time", "TimeSpan" },
            { "datetimeoffset", "DateTimeOffset" },
            { "smalldatetime", "DateTime" },
            { "timestamp", "byte[]" },

            // Boolean
            { "bit", "bool" },

            // Binary types
            { "binary", "byte[]" },
            { "varbinary", "byte[]" },
            { "image", "byte[]" },

            // GUID
            { "uniqueidentifier", "Guid" },

            // XML
            { "xml", "string" },

            // JSON (SQL Server 2016+)
            { "json", "string" }
        };

        /// <summary>
        /// Maps MySQL data types to C# types.
        /// </summary>
        private static readonly Dictionary<string, string> MySqlToCSharp = new(StringComparer.OrdinalIgnoreCase)
        {
            // Integer types
            { "tinyint", "byte" },
            { "smallint", "short" },
            { "mediumint", "int" },
            { "int", "int" },
            { "integer", "int" },
            { "bigint", "long" },

            // Decimal types
            { "decimal", "decimal" },
            { "numeric", "decimal" },
            { "float", "float" },
            { "double", "double" },
            { "real", "double" },

            // String types
            { "char", "string" },
            { "varchar", "string" },
            { "text", "string" },
            { "tinytext", "string" },
            { "mediumtext", "string" },
            { "longtext", "string" },

            // Date/Time types
            { "datetime", "DateTime" },
            { "date", "DateTime" },
            { "time", "TimeSpan" },
            { "timestamp", "DateTime" },
            { "year", "int" },

            // Boolean (MySQL uses TINYINT(1))
            { "boolean", "bool" },
            { "bool", "bool" },

            // Binary types
            { "binary", "byte[]" },
            { "varbinary", "byte[]" },
            { "blob", "byte[]" },
            { "tinyblob", "byte[]" },
            { "mediumblob", "byte[]" },
            { "longblob", "byte[]" },

            // GUID
            { "uuid", "Guid" },

            // JSON (MySQL 5.7+)
            { "json", "string" },

            // Enum/Set (treated as string)
            { "enum", "string" },
            { "set", "string" }
        };

        /// <summary>
        /// Maps a SQL data type to a C# type.
        /// </summary>
        /// <param name="sqlType">SQL data type (e.g., "VARCHAR", "INT")</param>
        /// <param name="engine">Database engine</param>
        /// <returns>C# type (e.g., "string", "int")</returns>
        public static string MapToCSharp(string sqlType, Models.DatabaseEngine engine)
        {
            if (string.IsNullOrWhiteSpace(sqlType))
                return "object";

            // Remove size/precision info (e.g., "VARCHAR(50)" -> "VARCHAR")
            string cleanType = sqlType.Split('(')[0].Trim().ToLowerInvariant();

            // Special case: TINYINT(1) in MySQL is typically boolean
            if (engine == Models.DatabaseEngine.MySql && sqlType.ToLowerInvariant().StartsWith("tinyint(1)"))
                return "bool";

            var mappings = engine == Models.DatabaseEngine.SqlServer
                ? SqlServerToCSharp
                : MySqlToCSharp;

            return mappings.TryGetValue(cleanType, out string csharpType)
                ? csharpType
                : "object"; // Fallback for unknown types
        }

        /// <summary>
        /// Gets the default primary key type for Identifiable<T>.
        /// </summary>
        /// <param name="sqlType">SQL data type</param>
        /// <param name="engine">Database engine</param>
        /// <returns>Primary key type (e.g., "int", "long", "Guid")</returns>
        public static string GetPrimaryKeyType(string sqlType, Models.DatabaseEngine engine)
        {
            string csharpType = MapToCSharp(sqlType, engine);

            // Primary keys should not be nullable
            return csharpType.TrimEnd('?');
        }
    }
}
