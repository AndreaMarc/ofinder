using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MySql.Data.MySqlClient;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Analyzes MySQL database schema.
    /// </summary>
    public class MySqlAnalyzer : IDatabaseAnalyzer
    {
        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public string GetDatabaseName(string connectionString)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                return builder.Database ?? "Database";
            }
            catch
            {
                return "Database";
            }
        }

        public async Task<List<TableSchema>> AnalyzeSchemaAsync(string connectionString)
        {
            var tables = new List<TableSchema>();
            var usedEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string databaseName = GetDatabaseName(connectionString);

            // Get all tables
            var tableNames = await GetTablesAsync(connection, databaseName);

            foreach (var tableName in tableNames)
            {
                // Generate base entity name
                string baseEntityName = ConvertToEntityName(tableName);
                string finalEntityName = baseEntityName;

                // If EntityName already used, add random suffix until unique
                while (usedEntityNames.Contains(finalEntityName))
                {
                    finalEntityName = $"{baseEntityName}_{GenerateRandomSuffix()}";
                }

                usedEntityNames.Add(finalEntityName);

                var table = new TableSchema
                {
                    TableName = tableName,
                    Schema = databaseName,
                    EntityName = finalEntityName
                };

                // Get columns (pass entity name for conflict detection)
                table.Columns = await GetColumnsAsync(connection, databaseName, tableName, table.EntityName);

                tables.Add(table);
            }

            // Get relationships (foreign keys)
            await PopulateRelationshipsAsync(connection, databaseName, tables);

            return tables;
        }

        private async Task<List<string>> GetTablesAsync(MySqlConnection connection, string databaseName)
        {
            var tables = new List<string>();

            string query = @"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = @DatabaseName
                    AND TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_NAME";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        private async Task<List<ColumnSchema>> GetColumnsAsync(MySqlConnection connection, string databaseName, string tableName, string entityName)
        {
            var columns = new List<ColumnSchema>();

            string query = @"
                SELECT
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.IS_NULLABLE,
                    c.CHARACTER_MAXIMUM_LENGTH,
                    c.COLUMN_TYPE,
                    CASE WHEN c.COLUMN_KEY = 'PRI' THEN 1 ELSE 0 END AS IS_PRIMARY_KEY
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_SCHEMA = @DatabaseName
                    AND c.TABLE_NAME = @TableName
                ORDER BY c.ORDINAL_POSITION";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            command.Parameters.AddWithValue("@TableName", tableName);

            using var reader = await command.ExecuteReaderAsync();

            // PASS 1: Read all columns and generate base PropertyName
            while (await reader.ReadAsync())
            {
                string columnName = reader.GetString(0);
                string dataType = reader.GetString(1);
                bool isNullable = reader.GetString(2) == "YES";
                int? maxLength = reader.IsDBNull(3) ? null : Convert.ToInt32(reader.GetValue(3));
                string columnType = reader.GetString(4); // Full type with size (e.g., "tinyint(1)")
                bool isPrimaryKey = reader.GetInt32(5) == 1;

                var column = new ColumnSchema
                {
                    ColumnName = columnName,
                    DataType = columnType, // Use full column type for better mapping
                    CSharpType = SqlTypeMappings.MapToCSharp(columnType, DatabaseEngine.MySql),
                    PropertyName = ConvertToPropertyName(columnName, entityName, isPrimaryKey),
                    IsNullable = isNullable,
                    IsPrimaryKey = isPrimaryKey,
                    MaxLength = maxLength
                };

                columns.Add(column);
            }

            // PASS 2: Detect and resolve duplicate PropertyNames
            var usedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in columns)
            {
                string originalName = col.PropertyName;

                // If PropertyName already used, add random suffix until unique
                while (usedPropertyNames.Contains(col.PropertyName))
                {
                    col.PropertyName = $"{originalName}_{GenerateRandomSuffix()}";
                }

                usedPropertyNames.Add(col.PropertyName);
            }

            return columns;
        }

        private async Task PopulateRelationshipsAsync(MySqlConnection connection, string databaseName, List<TableSchema> tables)
        {
            string query = @"
                SELECT
                    kcu.TABLE_NAME,
                    kcu.COLUMN_NAME,
                    kcu.REFERENCED_TABLE_NAME,
                    kcu.REFERENCED_COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                WHERE kcu.TABLE_SCHEMA = @DatabaseName
                    AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
                ORDER BY kcu.TABLE_NAME, kcu.COLUMN_NAME";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string tableName = reader.GetString(0);
                string columnName = reader.GetString(1);
                string referencedTable = reader.GetString(2);
                string referencedColumn = reader.GetString(3);

                // Find source table
                var sourceTable = tables.FirstOrDefault(t => t.TableName == tableName);

                if (sourceTable != null)
                {
                    // Mark column as FK
                    var fkColumn = sourceTable.Columns.FirstOrDefault(c => c.ColumnName == columnName);
                    if (fkColumn != null)
                        fkColumn.IsForeignKey = true;

                    // Skip self-references (table references itself)
                    if (IsSelfReference(tableName, referencedTable))
                    {
                        continue;
                    }

                    // Add many-to-one relationship (check for duplicates first)
                    string referencedEntityName = ConvertToEntityName(referencedTable);

                    var manyToOneRel = new RelationshipSchema
                    {
                        ForeignKeyColumn = columnName,
                        ReferencedTable = referencedTable,
                        ReferencedColumn = referencedColumn,
                        Type = RelationshipType.ManyToOne,
                        NavigationPropertyName = referencedEntityName,
                        ReferencedEntityName = referencedEntityName
                    };

                    // Check if relationship already exists (avoid duplicates)
                    if (!sourceTable.Relationships.Any(r =>
                        r.NavigationPropertyName == manyToOneRel.NavigationPropertyName &&
                        r.Type == manyToOneRel.Type &&
                        r.ReferencedEntityName == manyToOneRel.ReferencedEntityName))
                    {
                        sourceTable.Relationships.Add(manyToOneRel);
                    }
                }

                // Find referenced table and add one-to-many relationship
                var referencedTableObj = tables.FirstOrDefault(t => t.TableName == referencedTable);

                if (referencedTableObj != null)
                {
                    // Skip self-references (table references itself)
                    if (IsSelfReference(tableName, referencedTable))
                    {
                        continue;
                    }

                    string sourceEntityName = ConvertToEntityName(tableName);
                    string pluralName = sourceEntityName.Pluralize();

                    var oneToManyRel = new RelationshipSchema
                    {
                        ForeignKeyColumn = columnName,
                        ReferencedTable = tableName,
                        ReferencedColumn = referencedColumn,
                        Type = RelationshipType.OneToMany,
                        NavigationPropertyName = pluralName,
                        ReferencedEntityName = sourceEntityName
                    };

                    // Check if relationship already exists (avoid duplicates)
                    if (!referencedTableObj.Relationships.Any(r =>
                        r.NavigationPropertyName == oneToManyRel.NavigationPropertyName &&
                        r.Type == oneToManyRel.Type &&
                        r.ReferencedEntityName == oneToManyRel.ReferencedEntityName))
                    {
                        referencedTableObj.Relationships.Add(oneToManyRel);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a relationship is a self-reference (table references itself).
        /// Example: Employee.ManagerId -> Employee.Id
        /// </summary>
        private bool IsSelfReference(string sourceTable, string referencedTable)
        {
            // Compare entity names after conversion (sanitization + pascalize + singularize)
            string sourceEntity = ConvertToEntityName(sourceTable);
            string referencedEntity = ConvertToEntityName(referencedTable);

            return sourceEntity.Equals(referencedEntity, StringComparison.OrdinalIgnoreCase);
        }

        private string ConvertToEntityName(string tableName)
        {
            // Remove common prefixes
            string cleaned = tableName;
            if (cleaned.StartsWith("tbl_", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned.Substring(4);

            // Sanitize and convert to PascalCase
            return SanitizeIdentifier(cleaned).Singularize();
        }

        private string ConvertToPropertyName(string columnName, string entityName, bool isPrimaryKey)
        {
            // Sanitize and convert to PascalCase
            string baseName = SanitizeIdentifier(columnName);

            // Conflict 1: Column named "Id" that is NOT a primary key
            // This would conflict with Identifiable<T>.Id property
            if (baseName.Equals("Id", StringComparison.OrdinalIgnoreCase) && !isPrimaryKey)
            {
                return $"Id_{GenerateRandomSuffix()}";
            }

            // Conflict 2: Property name equals entity class name
            // This is invalid in C# (e.g., "public string Product { get; set; }" in class Product)
            if (baseName.Equals(entityName, StringComparison.OrdinalIgnoreCase))
            {
                return $"{baseName}_{GenerateRandomSuffix()}";
            }

            return baseName;
        }

        /// <summary>
        /// Sanitizes identifier by removing special characters, keeping only alphanumeric and underscores.
        /// Then converts to PascalCase.
        /// Examples:
        /// - "MAE-09-Perc%" -> "Mae09Perc"
        /// - "User-Profile" -> "UserProfile"
        /// - "Cost%Total" -> "CostTotal"
        /// - "Invoice{2024}" -> "Invoice2024"
        /// </summary>
        private string SanitizeIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Remove special characters, keeping only alphanumeric and underscores
            var sanitized = new System.Text.StringBuilder();

            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sanitized.Append(c);
            }

            // Convert to PascalCase using Humanizer
            return sanitized.ToString().Pascalize();
        }

        /// <summary>
        /// Generates a random 4-character alphanumeric suffix (lowercase a-z and 0-9).
        /// Used to resolve naming conflicts (e.g., property name = class name, or "Id" collision).
        /// Example: "x7k9", "a1b2", "m9c4"
        /// </summary>
        private string GenerateRandomSuffix()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 4)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}
