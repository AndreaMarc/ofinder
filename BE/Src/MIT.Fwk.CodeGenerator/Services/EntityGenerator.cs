using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates entity classes from table schemas.
    /// </summary>
    public class EntityGenerator
    {
        private readonly TemplateEngine _templateEngine;
        private readonly string _entityTemplate;

        public EntityGenerator()
        {
            _templateEngine = new TemplateEngine();
            _entityTemplate = GetEntityTemplate();
        }

        /// <summary>
        /// Generates entity class code for a table.
        /// </summary>
        public string GenerateEntity(TableSchema table, string namespaceName)
        {
            // Handle composite keys
            if (table.HasCompositePrimaryKey)
            {
                return GenerateEntityWithCompositeKey(table, namespaceName);
            }

            // Get primary key type
            var pkColumn = table.SinglePrimaryKey;
            string pkType = pkColumn != null
                ? SqlTypeMappings.GetPrimaryKeyType(pkColumn.DataType, DatabaseEngine.SqlServer)
                : "int";

            // Prepare scalar properties (exclude FK and PK - PK will be handled separately as Id override if needed)
            var scalarProperties = table.Columns
                .Where(c => !c.IsForeignKey && !c.IsPrimaryKey)
                .Select(c => new
                {
                    c.PropertyName,
                    Type = c.FullCSharpType,
                    c.ColumnName,
                    c.RequiresColumnAttribute,
                    c.NeedsSummary,
                    c.SummaryText
                })
                .ToList();

            // Prepare navigation properties
            var navigationProperties = new List<object>();

            foreach (var rel in table.Relationships.Distinct())
            {
                string attribute = rel.Type == RelationshipType.OneToMany ? "HasMany" : "HasOne";
                string type = rel.Type == RelationshipType.OneToMany
                    ? $"ICollection<{rel.ReferencedEntityName}>"
                    : rel.ReferencedEntityName;

                navigationProperties.Add(new
                {
                    Attribute = attribute,
                    Type = type,
                    Name = rel.NavigationPropertyName
                });
            }

            // Render template
            var data = new Dictionary<string, object>
            {
                { "Namespace", namespaceName },
                { "EntityName", table.EntityName },
                { "TableName", table.TableName },
                { "PkType", pkType },
                { "PkColumnName", pkColumn?.ColumnName },
                { "HasCustomPkName", pkColumn?.IsIdOverride ?? false },
                { "Properties", scalarProperties },
                { "NavigationProperties", navigationProperties },
                { "HasNavigationProperties", navigationProperties.Any() }
            };

            return _templateEngine.Render(_entityTemplate, data);
        }

        /// <summary>
        /// Generates entity with composite key.
        /// </summary>
        private string GenerateEntityWithCompositeKey(TableSchema table, string namespaceName)
        {
            var sb = new StringBuilder();

            // Generate composite key class
            string keyClassName = $"{table.EntityName}Key";

            sb.AppendLine("using JsonApiDotNetCore.Resources;");
            sb.AppendLine("using JsonApiDotNetCore.Resources.Annotations;");
            sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Entities");
            sb.AppendLine("{");

            // Key class
            sb.AppendLine($"    public class {keyClassName}");
            sb.AppendLine("    {");
            foreach (var pkCol in table.PrimaryKeyColumns)
            {
                // Add XML summary when [Column] attribute is present
                if (pkCol.RequiresColumnAttribute)
                {
                    sb.AppendLine($"        /// <summary>{pkCol.SummaryText}</summary>");
                }
                // Add [Column] attribute if property name differs from column name
                if (pkCol.RequiresColumnAttribute)
                {
                    sb.AppendLine($"        [Column(\"{pkCol.ColumnName}\")]");
                }
                sb.AppendLine($"        public {pkCol.CSharpType} {pkCol.PropertyName} {{ get; set; }}");
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            // Entity class
            sb.AppendLine("    [Resource]");
            sb.AppendLine($"    [Table(\"{table.TableName}\")]");
            sb.AppendLine($"    public class {table.EntityName} : Identifiable<{keyClassName}>");
            sb.AppendLine("    {");

            // Scalar properties (exclude PKs)
            foreach (var col in table.Columns.Where(c => !c.IsPrimaryKey && !c.IsForeignKey))
            {
                // Add XML summary when [Column] attribute is present
                if (col.RequiresColumnAttribute)
                {
                    sb.AppendLine($"        /// <summary>{col.SummaryText}</summary>");
                }
                sb.AppendLine("        [Attr]");
                // Add [Column] attribute if property name differs from column name
                if (col.RequiresColumnAttribute)
                {
                    sb.AppendLine($"        [Column(\"{col.ColumnName}\")]");
                }
                sb.AppendLine($"        public {col.FullCSharpType} {col.PropertyName} {{ get; set; }}");
                sb.AppendLine();
            }

            // Navigation properties
            foreach (var rel in table.Relationships.Distinct())
            {
                string attribute = rel.Type == RelationshipType.OneToMany ? "HasMany" : "HasOne";
                string type = rel.Type == RelationshipType.OneToMany
                    ? $"ICollection<{rel.ReferencedEntityName}>"
                    : rel.ReferencedEntityName;

                sb.AppendLine($"        [{attribute}]");
                sb.AppendLine($"        public virtual {type} {rel.NavigationPropertyName} {{ get; set; }}");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the entity template.
        /// </summary>
        private string GetEntityTemplate()
        {
            return @"using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace {{Namespace}}.Entities
{
    [Resource]
    [Table(""{{TableName}}"")]
    public class {{EntityName}} : Identifiable<{{PkType}}>
    {
{{#If HasCustomPkName}}
        [Attr]
        [Column(""{{PkColumnName}}"")]
        public override {{PkType}} Id { get; set; }

{{/If}}
{{#Properties}}
{{#If RequiresColumnAttribute}}
        /// <summary>{{SummaryText}}</summary>
{{/If}}
        [Attr]
{{#If RequiresColumnAttribute}}
        [Column(""{{ColumnName}}"")]
{{/If}}
        public {{Type}} {{PropertyName}} { get; set; }

{{/Properties}}
{{#If HasNavigationProperties}}
{{#NavigationProperties}}
        [{{Attribute}}]
        public virtual {{Type}} {{Name}} { get; set; }

{{/NavigationProperties}}
{{/If}}
    }
}";
        }

        /// <summary>
        /// Generates all entity files for selected tables.
        /// </summary>
        public Dictionary<string, string> GenerateAllEntities(List<TableSchema> tables, string namespaceName)
        {
            var entities = new Dictionary<string, string>();

            foreach (var table in tables)
            {
                string fileName = $"{table.EntityName}.cs";
                string code = GenerateEntity(table, namespaceName);
                entities[fileName] = code;
            }

            return entities;
        }

        /// <summary>
        /// Writes entity files to disk.
        /// </summary>
        public void WriteEntitiesToDisk(Dictionary<string, string> entities, string entitiesPath)
        {
            if (!Directory.Exists(entitiesPath))
                Directory.CreateDirectory(entitiesPath);

            foreach (var (fileName, code) in entities)
            {
                string filePath = Path.Combine(entitiesPath, fileName);
                File.WriteAllText(filePath, code);
            }
        }
    }
}
