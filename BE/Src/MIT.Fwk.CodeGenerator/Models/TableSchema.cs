using System.Collections.Generic;
using System.Linq;

namespace MIT.Fwk.CodeGenerator.Models
{
    /// <summary>
    /// Represents metadata for a database table.
    /// </summary>
    public class TableSchema
    {
        /// <summary>
        /// Table name in the database (e.g., "customer_orders")
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Entity class name in PascalCase (e.g., "CustomerOrder")
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Database schema name (e.g., "dbo", "public")
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// All columns in the table
        /// </summary>
        public List<ColumnSchema> Columns { get; set; } = new List<ColumnSchema>();

        /// <summary>
        /// Relationships (foreign keys) to other tables
        /// </summary>
        public List<RelationshipSchema> Relationships { get; set; } = new List<RelationshipSchema>();

        /// <summary>
        /// Primary key column(s)
        /// </summary>
        public List<ColumnSchema> PrimaryKeyColumns =>
            Columns.Where(c => c.IsPrimaryKey).ToList();

        /// <summary>
        /// Whether this table has a composite primary key
        /// </summary>
        public bool HasCompositePrimaryKey => PrimaryKeyColumns.Count > 1;

        /// <summary>
        /// Single primary key column (null if composite key)
        /// </summary>
        public ColumnSchema SinglePrimaryKey =>
            HasCompositePrimaryKey ? null : PrimaryKeyColumns.FirstOrDefault();
    }
}
