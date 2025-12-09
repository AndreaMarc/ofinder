using System.IO;
using System.Text;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates generic repository class for JsonAPI.
    /// Pattern: OtherDbContextRepository (generic for all entities).
    /// </summary>
    public class RepositoryGenerator
    {
        /// <summary>
        /// Generates generic repository class code.
        /// </summary>
        public string GenerateRepository(string dbName, string namespaceName)
        {
            var sb = new StringBuilder();

            // Using statements
            sb.AppendLine("using JetBrains.Annotations;");
            sb.AppendLine("using JsonApiDotNetCore.Configuration;");
            sb.AppendLine("using JsonApiDotNetCore.Queries;");
            sb.AppendLine("using JsonApiDotNetCore.Repositories;");
            sb.AppendLine("using JsonApiDotNetCore.Resources;");
            sb.AppendLine("using Microsoft.Extensions.Logging;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {namespaceName}.Data;");
            sb.AppendLine();

            // Class declaration with primary constructor (C# 12)
            sb.AppendLine("[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]");
            sb.AppendLine($"public sealed class {dbName}DbContextRepository<TResource, TId>(");
            sb.AppendLine("    ITargetedFields targetedFields,");
            sb.AppendLine($"    DbContextResolver<{dbName}DbContext> dbContextResolver,");
            sb.AppendLine("    IResourceGraph resourceGraph,");
            sb.AppendLine("    IResourceFactory resourceFactory,");
            sb.AppendLine("    IEnumerable<IQueryConstraintProvider> constraintProviders,");
            sb.AppendLine("    ILoggerFactory loggerFactory,");
            sb.AppendLine("    IResourceDefinitionAccessor resourceDefinitionAccessor)");
            sb.AppendLine("    : EntityFrameworkCoreRepository<TResource, TId>(");
            sb.AppendLine("        targetedFields,");
            sb.AppendLine("        dbContextResolver,");
            sb.AppendLine("        resourceGraph,");
            sb.AppendLine("        resourceFactory,");
            sb.AppendLine("        constraintProviders,");
            sb.AppendLine("        loggerFactory,");
            sb.AppendLine("        resourceDefinitionAccessor)");
            sb.AppendLine("    where TResource : class, IIdentifiable<TId>;");

            return sb.ToString();
        }

        /// <summary>
        /// Writes repository to disk.
        /// </summary>
        public void WriteRepositoryToDisk(string code, string dataPath, string dbName)
        {
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            string filePath = Path.Combine(dataPath, $"{dbName}DbContextRepository.cs");
            File.WriteAllText(filePath, code);
        }
    }
}
