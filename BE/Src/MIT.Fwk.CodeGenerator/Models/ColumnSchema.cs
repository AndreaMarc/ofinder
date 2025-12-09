namespace MIT.Fwk.CodeGenerator.Models
{
    /// <summary>
    /// Represents metadata for a database column.
    /// </summary>
    public class ColumnSchema
    {
        /// <summary>
        /// Column name in the database (e.g., "customer_name")
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// SQL data type (e.g., "VARCHAR", "INT", "DATETIME")
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Mapped C# type (e.g., "string", "int", "DateTime")
        /// </summary>
        public string CSharpType { get; set; }

        /// <summary>
        /// Property name in PascalCase (e.g., "CustomerName")
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Whether the column allows NULL values
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Whether this column is part of the primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Whether this column is a foreign key
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Maximum length for string columns (NULL if not applicable)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Returns the full C# type including nullable marker if needed
        /// </summary>
        public string FullCSharpType =>
            IsNullable && !CSharpType.EndsWith("?") && CSharpType != "string" && CSharpType != "byte[]"
                ? $"{CSharpType}?"
                : CSharpType;

        /// <summary>
        /// Whether the PropertyName differs from ColumnName (requires [Column] attribute).
        /// Returns true if sanitization changed the name (e.g., "User-Id" -> "UserId")
        /// </summary>
        public bool RequiresColumnAttribute
        {
            get
            {
                // If names are identical, no attribute needed
                return PropertyName != ColumnName;
            }
        }

        /// <summary>
        /// Whether this column is the primary key but NOT named "Id".
        /// When true, generate "public override TType Id { get; set; }" instead of property with column name.
        /// Examples: CGX1CODICE, ProductCode, user_profile_id should all override Id.
        /// </summary>
        public bool IsIdOverride
        {
            get
            {
                // Is primary key AND property name is not "Id" (case-insensitive)
                return IsPrimaryKey && !PropertyName.Equals("Id", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Whether this property needs an XML summary comment.
        /// Returns true when PropertyName differs from ColumnName (mapping changed).
        /// </summary>
        public bool NeedsSummary
        {
            get
            {
                return PropertyName != ColumnName;
            }
        }

        /// <summary>
        /// Gets the XML summary text explaining the database column mapping.
        /// Example: "Database column: customer_id"
        /// </summary>
        public string SummaryText
        {
            get
            {
                return $"Database column: {ColumnName}";
            }
        }
    }
}
