using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Orchestrates the entire code generation process.
    /// </summary>
    public class CodeGeneratorOrchestrator
    {
        private readonly EntityGenerator _entityGenerator;
        private readonly DbContextGenerator _dbContextGenerator;
        private readonly RepositoryGenerator _repositoryGenerator;
        private readonly ServiceGenerator _serviceGenerator;
        private readonly ProjectFileGenerator _projectFileGenerator;
        private readonly ConfigurationUpdater _configurationUpdater;
        private readonly SolutionManager _solutionManager;
        private readonly StandardEntityTestsGenerator _testGenerator;

        public CodeGeneratorOrchestrator()
        {
            _entityGenerator = new EntityGenerator();
            _dbContextGenerator = new DbContextGenerator();
            _repositoryGenerator = new RepositoryGenerator();
            _serviceGenerator = new ServiceGenerator();
            _projectFileGenerator = new ProjectFileGenerator();
            _configurationUpdater = new ConfigurationUpdater();
            _solutionManager = new SolutionManager();
            _testGenerator = new StandardEntityTestsGenerator();
        }

        /// <summary>
        /// Generates a complete module from database schema.
        /// </summary>
        public async Task<bool> GenerateModuleAsync(GeneratorOptions options)
        {
            try
            {
                Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║          MIT Framework - Code Generator v9.0                  ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

                // Step 1: Test connection
                Console.WriteLine("🔌 Step 1: Testing database connection...");
                var analyzer = DatabaseAnalyzerFactory.CreateAnalyzer(options.Engine);

                bool connected = await analyzer.TestConnectionAsync(options.ConnectionString);
                if (!connected)
                {
                    Console.WriteLine("  ❌ Connection failed. Please check your connection string.");
                    return false;
                }
                Console.WriteLine("  ✅ Connection successful!");

                // Step 2: Analyze schema
                Console.WriteLine("\n📊 Step 2: Analyzing database schema...");
                var allTables = await analyzer.AnalyzeSchemaAsync(options.ConnectionString);

                if (!allTables.Any())
                {
                    Console.WriteLine("  ❌ No tables found in database.");
                    return false;
                }

                Console.WriteLine($"  ✅ Found {allTables.Count} tables");
                Console.WriteLine($"  ✅ Found {allTables.Sum(t => t.Columns.Count)} columns");
                Console.WriteLine($"  ✅ Found {allTables.Sum(t => t.Relationships.Count)} relationships");

                // Use selected tables from options (already filtered by user)
                var tables = options.SelectedTables;

                // Step 3: Check if output directory exists
                Console.WriteLine($"\n📁 Step 3: Checking output directory...");
                if (Directory.Exists(options.OutputPath))
                {
                    Console.WriteLine($"  ❌ Error: Directory already exists: {options.OutputPath}");
                    Console.WriteLine("  Please delete it manually before running the generator.");
                    return false;
                }
                Console.WriteLine($"  ✅ Output path is available");

                // Step 4: Create directory structure
                Console.WriteLine("\n🗂️  Step 4: Creating directory structure...");
                CreateDirectoryStructure(options.OutputPath);
                Console.WriteLine($"  ✅ Created: {options.OutputPath}");

                // Step 5: Generate entities
                Console.WriteLine($"\n🔨 Step 5: Generating {tables.Count} entities...");
                string entitiesPath = Path.Combine(options.OutputPath, "Entities");
                var entities = _entityGenerator.GenerateAllEntities(tables, options.RootNamespace);
                _entityGenerator.WriteEntitiesToDisk(entities, entitiesPath);
                Console.WriteLine($"  ✅ Generated {entities.Count} entity classes");

                // Step 6: Generate DbContext
                Console.WriteLine("\n🔨 Step 6: Generating DbContext...");
                string dataPath = Path.Combine(options.OutputPath, "Data");
                string dbContextCode = _dbContextGenerator.GenerateDbContext(tables, options);
                _dbContextGenerator.WriteDbContextToDisk(dbContextCode, dataPath, options.DatabaseName);
                Console.WriteLine($"  ✅ Generated {options.DatabaseName}DbContext");

                // Step 7: Generate Repository
                Console.WriteLine("\n🔨 Step 7: Generating Repository...");
                string repositoryCode = _repositoryGenerator.GenerateRepository(options.DatabaseName, options.RootNamespace);
                _repositoryGenerator.WriteRepositoryToDisk(repositoryCode, dataPath, options.DatabaseName);
                Console.WriteLine($"  ✅ Generated {options.DatabaseName}DbContextRepository");

                // Step 8: Generate ManualService
                Console.WriteLine("\n🔨 Step 8: Generating ManualService...");
                string interfacesPath = Path.Combine(options.OutputPath, "Interfaces");
                string servicesPath = Path.Combine(options.OutputPath, "Services");

                string serviceInterfaceCode = _serviceGenerator.GenerateServiceInterface(options.DatabaseName, options.RootNamespace);
                _serviceGenerator.WriteServiceInterfaceToDisk(serviceInterfaceCode, interfacesPath, options.DatabaseName);

                string serviceImplCode = _serviceGenerator.GenerateServiceImplementation(options.DatabaseName, options.RootNamespace);
                _serviceGenerator.WriteServiceImplementationToDisk(serviceImplCode, servicesPath, options.DatabaseName);
                Console.WriteLine($"  ✅ Generated I{options.DatabaseName}ManualService");
                Console.WriteLine($"  ✅ Generated {options.DatabaseName}ManualService");

                // Step 9: Generate .csproj
                Console.WriteLine("\n🔨 Step 9: Generating project file...");
                string projectFileCode = _projectFileGenerator.GenerateProjectFile(options.ProjectName);
                _projectFileGenerator.WriteProjectFileToDisk(projectFileCode, options.OutputPath, options.ProjectName);
                Console.WriteLine($"  ✅ Generated {options.ProjectName}.csproj");

                // Step 11: Update configurations
                _configurationUpdater.UpdateConfigurations(
                    options.DatabaseName,
                    options.Engine,
                    options.ConnectionString);

                // Step 12: Update solution and references
                string projectPath = Path.Combine(options.OutputPath, $"{options.ProjectName}.csproj");
                _solutionManager.UpdateSolutionAndReferences(
                    options.SolutionPath,
                    options.WebApiProjectPath,
                    projectPath,
                    options.ProjectName);

                // Step 13: Generate unit tests
                Console.WriteLine("\n🔨 Step 13: Generating unit tests...");
                string testMethod = _testGenerator.GenerateTestMethod(options.DatabaseName, options.RootNamespace);
                string usingNamespace = $"{options.RootNamespace}.Data";
                _testGenerator.AppendTestToStandardEntityTests(testMethod, usingNamespace, options.TestFilePath);
                Console.WriteLine($"  ✅ Generated test method for {options.DatabaseName}DbContext");

                // Final summary
                PrintSuccessSummary(options, tables);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error during code generation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Creates the directory structure for the module.
        /// </summary>
        private void CreateDirectoryStructure(string basePath)
        {
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "Data"));
            Directory.CreateDirectory(Path.Combine(basePath, "Entities"));
            Directory.CreateDirectory(Path.Combine(basePath, "Interfaces"));
            Directory.CreateDirectory(Path.Combine(basePath, "Services"));
        }

        /// <summary>
        /// Prints success summary.
        /// </summary>
        private void PrintSuccessSummary(GeneratorOptions options, List<TableSchema> tables)
        {
            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    GENERATION COMPLETE!                       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"📁 Module location: {options.OutputPath}\n");
            Console.WriteLine("Generated files:");
            Console.WriteLine($"  ✅ {tables.Count} entity classes");
            Console.WriteLine($"  ✅ {options.DatabaseName}DbContext");
            Console.WriteLine($"  ✅ {options.DatabaseName}DbContextRepository");
            Console.WriteLine($"  ✅ I{options.DatabaseName}ManualService");
            Console.WriteLine($"  ✅ {options.DatabaseName}ManualService");
            Console.WriteLine($"  ✅ {options.ProjectName}.csproj");
            Console.WriteLine($"  ✅ Updated appsettings.json");
            Console.WriteLine($"  ✅ Updated dbconnections.json");
            Console.WriteLine($"  ✅ Added to solution");
            Console.WriteLine($"  ✅ Added reference to WebApi");
            Console.WriteLine($"  ✅ Generated unit test for {options.DatabaseName}DbContext");
            Console.WriteLine();
            Console.WriteLine("⚠️  NOTE: This module does NOT include migrations (read-only access).");
            Console.WriteLine("   If you need to modify the database schema, edit the external DB directly.");
        }
    }
}
