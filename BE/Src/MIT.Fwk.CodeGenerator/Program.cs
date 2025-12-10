using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MIT.Fwk.CodeGenerator.Models;
using MIT.Fwk.CodeGenerator.Services;

namespace MIT.Fwk.CodeGenerator
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                Console.Clear();
                PrintBanner();

                // Step 1: Get connection string
                Console.WriteLine("Enter database connection string:");
                Console.Write("> ");
                string connectionString = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Console.WriteLine("❌ Connection string cannot be empty.");
                    return 1;
                }

                // Step 2: Get database engine
                Console.WriteLine("\nSelect database engine:");
                Console.WriteLine("  [1] SQL Server");
                Console.WriteLine("  [2] MySQL");
                Console.Write("> ");
                string engineInput = Console.ReadLine()?.Trim();

                DatabaseEngine engine;
                if (engineInput == "1")
                    engine = DatabaseEngine.SqlServer;
                else if (engineInput == "2")
                    engine = DatabaseEngine.MySql;
                else
                {
                    Console.WriteLine("❌ Invalid selection. Please enter 1 or 2.");
                    return 1;
                }

                // Step 3: Get database name
                Console.WriteLine("\nEnter database name (for naming convention, e.g., 'Northwind'):");
                Console.Write("> ");
                string dbName = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(dbName) || !IsValidIdentifier(dbName))
                {
                    Console.WriteLine("❌ Invalid database name. Use only alphanumeric characters.");
                    return 1;
                }

                // Ensure PascalCase
                dbName = ToPascalCase(dbName);

                // Step 4: Test connection and analyze schema
                Console.WriteLine("\n🔌 Testing connection...");
                var analyzer = DatabaseAnalyzerFactory.CreateAnalyzer(engine);

                bool connected = await analyzer.TestConnectionAsync(connectionString);
                if (!connected)
                {
                    Console.WriteLine("❌ Connection failed. Please check your connection string.");
                    return 1;
                }
                Console.WriteLine("✅ Connection successful!");

                Console.WriteLine("\n📊 Analyzing database schema...");
                var allTables = await analyzer.AnalyzeSchemaAsync(connectionString);

                if (!allTables.Any())
                {
                    Console.WriteLine("❌ No tables found in database.");
                    return 1;
                }

                Console.WriteLine($"✅ Found {allTables.Count} tables");

                // Step 5: Table selection
                Console.WriteLine("\n📋 Select tables to generate (enter numbers separated by commas, or 'all' for all tables):");
                for (int i = 0; i < allTables.Count; i++)
                {
                    var table = allTables[i];
                    string pkInfo = table.HasCompositePrimaryKey
                        ? " [Composite PK]"
                        : table.SinglePrimaryKey != null
                            ? $" [PK: {table.SinglePrimaryKey.PropertyName}]"
                            : " [No PK]";

                    Console.WriteLine($"  [{i + 1}] {table.TableName} → {table.EntityName}{pkInfo}");
                }

                Console.Write("\n> ");
                string selection = Console.ReadLine()?.Trim().ToLowerInvariant();

                List<TableSchema> selectedTables;
                if (selection == "all")
                {
                    selectedTables = allTables;
                }
                else
                {
                    var indices = ParseSelectionIndices(selection, allTables.Count);
                    if (!indices.Any())
                    {
                        Console.WriteLine("❌ Invalid selection.");
                        return 1;
                    }

                    selectedTables = indices.Select(i => allTables[i]).ToList();
                }

                Console.WriteLine($"\n✅ Selected {selectedTables.Count} tables for generation");

                // Step 6: Prepare options
                string basePath = @"C:\MaeFWK\maefwk8";
                string outputPath = System.IO.Path.Combine(basePath, "Src", $"MIT.Fwk.{dbName}");
                string solutionPath = System.IO.Path.Combine(basePath, "MIT.Fwk.sln");
                string webApiProjectPath = System.IO.Path.Combine(basePath, "Src", "MIT.Fwk.WebApi", "MIT.Fwk.WebApi.csproj");

                var options = new GeneratorOptions
                {
                    ConnectionString = connectionString,
                    Engine = engine,
                    DatabaseName = dbName,
                    OutputPath = outputPath,
                    SolutionPath = solutionPath,
                    WebApiProjectPath = webApiProjectPath,
                    SelectedTables = selectedTables
                };

                // Step 7: Confirm generation
                Console.WriteLine("\n📝 Generation Summary:");
                Console.WriteLine($"  Database: {dbName}");
                Console.WriteLine($"  Engine: {engine}");
                Console.WriteLine($"  Tables: {selectedTables.Count}");
                Console.WriteLine($"  Output: {outputPath}");
                Console.WriteLine($"\nProceed with generation? [Y/N]");
                Console.Write("> ");
                string confirm = Console.ReadLine()?.Trim().ToUpperInvariant();

                if (confirm != "Y" && confirm != "YES")
                {
                    Console.WriteLine("❌ Generation cancelled.");
                    return 0;
                }

                // Step 8: Generate
                var orchestrator = new CodeGeneratorOrchestrator();
                bool success = await orchestrator.GenerateModuleAsync(options);

                return success ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Fatal error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }

        /// <summary>
        /// Prints the banner.
        /// </summary>
        static void PrintBanner()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          MIT Framework - Code Generator v9.0                  ║");
            Console.WriteLine("║          Generate modules from existing databases             ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        /// <summary>
        /// Validates identifier (alphanumeric only).
        /// </summary>
        static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        /// <summary>
        /// Converts string to PascalCase.
        /// </summary>
        static string ToPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Simple implementation: capitalize first letter
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// Parses selection indices from comma-separated string.
        /// </summary>
        static List<int> ParseSelectionIndices(string input, int maxCount)
        {
            var indices = new List<int>();

            if (string.IsNullOrWhiteSpace(input))
                return indices;

            var parts = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int index))
                {
                    // Convert to 0-based index
                    int zeroBasedIndex = index - 1;

                    if (zeroBasedIndex >= 0 && zeroBasedIndex < maxCount)
                    {
                        indices.Add(zeroBasedIndex);
                    }
                }
            }

            return indices.Distinct().OrderBy(i => i).ToList();
        }
    }
}
