using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates DbContext class from table schemas.
    /// </summary>
    public class DbContextGenerator
    {
        private readonly TemplateEngine _templateEngine;

        public DbContextGenerator()
        {
            _templateEngine = new TemplateEngine();
        }

        /// <summary>
        /// Generates DbContext class code.
        /// </summary>
        public string GenerateDbContext(List<TableSchema> tables, GeneratorOptions options)
        {
            string dbName = options.DatabaseName;
            string namespaceName = options.RootNamespace;
            bool useSqlServer = options.Engine == DatabaseEngine.SqlServer;

            // Prepare DbSet properties
            var dbSets = tables.Select(t => new
            {
                EntityName = t.EntityName,
                PluralName = t.EntityName.Pluralize()
            }).ToList();

            var sb = new StringBuilder();

            // Using statements
            sb.AppendLine("using JetBrains.Annotations;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.Extensions.Configuration;");
            sb.AppendLine("using MIT.Fwk.Core.Domain.Interfaces;");
            sb.AppendLine($"using {namespaceName}.Entities;");
            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {namespaceName}.Data");
            sb.AppendLine("{");

            // Class declaration with IMPORTANT comment about IJsonApiDbContext only
            sb.AppendLine("    [UsedImplicitly(ImplicitUseTargetFlags.Members)]");
            sb.AppendLine("    // Implements IJsonApiDbContext for auto-discovery by JsonAPI");
            sb.AppendLine("    // Does NOT implement IMigrationDbContext - no migrations will be applied");
            sb.AppendLine($"    public partial class {dbName}DbContext : DbContext, IJsonApiDbContext");
            sb.AppendLine("    {");

            // Static provider flag
            sb.AppendLine($"        public static bool _UseSqlServer = {useSqlServer.ToString().ToLowerInvariant()};");
            sb.AppendLine();

            // Helper method for configuration
            sb.AppendLine("        // FASE 7: Helper method to get configuration for design-time scenarios (EF migrations)");
            sb.AppendLine("        private static IConfiguration GetConfiguration()");
            sb.AppendLine("        {");
            sb.AppendLine("            var builder = new ConfigurationBuilder()");
            sb.AppendLine("                .AddJsonFile(\"dbconnections.json\", optional: true, reloadOnChange: true)");
            sb.AppendLine("                .AddJsonFile(\"customsettings.json\", optional: true, reloadOnChange: true)");
            sb.AppendLine("                .AddJsonFile(\"appsettings.json\", optional: true, reloadOnChange: true)");
            sb.AppendLine("                .AddEnvironmentVariables();");
            sb.AppendLine("            return builder.Build();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Constructor with options
            sb.AppendLine($"        public {dbName}DbContext(DbContextOptions<{dbName}DbContext> options)");
            sb.AppendLine("            : base(options)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Parameterless constructor for design-time
            sb.AppendLine($"        public {dbName}DbContext() : this(_UseSqlServer");
            sb.AppendLine($"            ? new DbContextOptionsBuilder<{dbName}DbContext>()");
            sb.AppendLine($"                .UseSqlServer(GetConfiguration().GetConnectionString(nameof({dbName}DbContext)))");
            sb.AppendLine("                .Options");
            sb.AppendLine($"            : new DbContextOptionsBuilder<{dbName}DbContext>()");
            sb.AppendLine($"                .UseMySQL(GetConfiguration().GetConnectionString(nameof({dbName}DbContext)))");
            sb.AppendLine("                .Options)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();

            // OnConfiguring method
            sb.AppendLine("        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (!optionsBuilder.IsConfigured)");
            sb.AppendLine("            {");
            sb.AppendLine("                if (_UseSqlServer)");
            sb.AppendLine("                {");
            sb.AppendLine("                    optionsBuilder.UseSqlServer(");
            sb.AppendLine($"                        GetConfiguration().GetConnectionString(nameof({dbName}DbContext)));");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                {");
            sb.AppendLine("                    optionsBuilder.UseMySQL(");
            sb.AppendLine($"                        GetConfiguration().GetConnectionString(nameof({dbName}DbContext)));");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // DbSet properties
            foreach (var dbSet in dbSets)
            {
                sb.AppendLine($"        public DbSet<{dbSet.EntityName}> {dbSet.PluralName} => Set<{dbSet.EntityName}>();");
            }

            // Close class and namespace
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Writes DbContext to disk.
        /// </summary>
        public void WriteDbContextToDisk(string code, string dataPath, string dbName)
        {
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            string filePath = Path.Combine(dataPath, $"{dbName}DbContext.cs");
            File.WriteAllText(filePath, code);
        }
    }
}
