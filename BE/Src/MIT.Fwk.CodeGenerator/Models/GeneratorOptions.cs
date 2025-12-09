using System.Collections.Generic;

namespace MIT.Fwk.CodeGenerator.Models
{
    /// <summary>
    /// Options for the code generator.
    /// </summary>
    public class GeneratorOptions
    {
        /// <summary>
        /// Database connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Database engine (SQL Server or MySQL)
        /// </summary>
        public DatabaseEngine Engine { get; set; }

        /// <summary>
        /// Database name (used for naming convention, e.g., "Northwind")
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Output directory path (e.g., "C:\MaeFWK\maefwk8\Src\MIT.Fwk.Northwind")
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// Solution file path (e.g., "C:\MaeFWK\maefwk8\MIT.Fwk.sln")
        /// </summary>
        public string SolutionPath { get; set; }

        /// <summary>
        /// WebApi project file path (e.g., "C:\MaeFWK\maefwk8\Src\MIT.Fwk.WebApi\MIT.Fwk.WebApi.csproj")
        /// </summary>
        public string WebApiProjectPath { get; set; }

        /// <summary>
        /// List of tables selected by the user for generation
        /// </summary>
        public List<TableSchema> SelectedTables { get; set; } = new List<TableSchema>();

        /// <summary>
        /// Path to StandardEntityTests.cs file for test generation
        /// </summary>
        public string TestFilePath { get; set; } = @"C:\MaeFWK\maefwk8\Tests\MIT.Fwk.Tests.WebApi\Tests\Entities\StandardEntityTests.cs";

        /// <summary>
        /// Root namespace for generated code (e.g., "MIT.Fwk.Northwind")
        /// </summary>
        public string RootNamespace => $"MIT.Fwk.{DatabaseName}";

        /// <summary>
        /// Project name (e.g., "MIT.Fwk.Northwind")
        /// </summary>
        public string ProjectName => RootNamespace;
    }
}
