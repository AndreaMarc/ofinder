using System;
using System.Collections.Generic;
using System.Data;

namespace MIT.Fwk.Core.Data
{
    // FASE 6: DBParameters removed - use EF Core parameters or Dapper DynamicParameters
    // FASE 8A: LookupFilter, LookupFilterCriteria, LookupBodyData removed - use LINQ Where() or EF Core query filters

    /// <summary>
    /// Pagination information for lookup queries.
    /// </summary>
    public class LookupInfo
    {
        public int PageNumber { get; set; }
        public int RowsPerPage { get; set; }
        public long TotalRows { get; set; }
        public int TotalPages { get; set; }
        public bool WithMetadata { get; set; }
    }

    /// <summary>
    /// Sorting configuration for lookup queries.
    /// </summary>
    public class LookupSorting
    {
        public string PropertyName { get; set; }
        public LookupSortingDirection Direction { get; set; }
    }

    /// <summary>
    /// Sort direction enumeration.
    /// </summary>
    public enum LookupSortingDirection
    {
        ASC,
        DESC
    }

    // FASE 6: FilterCondition and FilterConditions enum removed - use LINQ Where() expressions

    /// <summary>
    /// Database schema information for entity-to-table mapping.
    /// INTERNAL USE: Used by DocumentManager for SQL bridge queries. Not intended for external use.
    /// For new code, prefer EF Core metadata API (IModel, IEntityType).
    /// </summary>
    public class DatabaseInformations
    {
        public bool IsTable { get; set; }
        public string ObjectName { get; set; }
        public string EntityName { get; set; }
        public Dictionary<string, ColumnDetail> EntityDbMappings { get; set; }
    }

    /// <summary>
    /// Database column metadata.
    /// </summary>
    public class ColumnDetail
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsRequired { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsUnique { get; set; }
        public bool IsKey { get; set; }
        public object DefaultValue { get; set; }
        public decimal MaxLength { get; set; }
    }
}
