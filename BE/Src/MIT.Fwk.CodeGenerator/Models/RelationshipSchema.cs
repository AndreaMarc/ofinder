namespace MIT.Fwk.CodeGenerator.Models
{
    /// <summary>
    /// Type of relationship between tables.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// One-to-many relationship (e.g., Customer -> Orders)
        /// </summary>
        OneToMany,

        /// <summary>
        /// Many-to-one relationship (e.g., Order -> Customer)
        /// </summary>
        ManyToOne
    }

    /// <summary>
    /// Represents a foreign key relationship between two tables.
    /// </summary>
    public class RelationshipSchema
    {
        /// <summary>
        /// Foreign key column in the current table
        /// </summary>
        public string ForeignKeyColumn { get; set; }

        /// <summary>
        /// Referenced table name
        /// </summary>
        public string ReferencedTable { get; set; }

        /// <summary>
        /// Referenced column (usually primary key)
        /// </summary>
        public string ReferencedColumn { get; set; }

        /// <summary>
        /// Type of relationship
        /// </summary>
        public RelationshipType Type { get; set; }

        /// <summary>
        /// Navigation property name (e.g., "Customer", "Orders")
        /// </summary>
        public string NavigationPropertyName { get; set; }

        /// <summary>
        /// Referenced entity class name (e.g., "Customer", "Order")
        /// </summary>
        public string ReferencedEntityName { get; set; }

        /// <summary>
        /// Determines equality based on navigation property name, type, and referenced entity.
        /// This allows .Distinct() to work correctly and prevents duplicate relationships.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is RelationshipSchema other)
            {
                return NavigationPropertyName == other.NavigationPropertyName &&
                       Type == other.Type &&
                       ReferencedEntityName == other.ReferencedEntityName;
            }
            return false;
        }

        /// <summary>
        /// Returns hash code based on navigation property name, type, and referenced entity.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(NavigationPropertyName, Type, ReferencedEntityName);
        }
    }
}
