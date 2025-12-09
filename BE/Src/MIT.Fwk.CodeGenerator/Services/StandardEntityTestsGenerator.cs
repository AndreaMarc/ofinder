using System;
using System.IO;
using System.Text;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates unit test methods for StandardEntityTests.cs.
    /// Appends a new test method for each generated DbContext.
    /// </summary>
    public class StandardEntityTestsGenerator
    {
        /// <summary>
        /// Generates a test method for a specific DbContext.
        /// Pattern: TestAllStandardEntities_{DbName}DbContext_ShouldSucceed()
        /// </summary>
        public string GenerateTestMethod(string dbName, string dbNamespace)
        {
            string camelDbName = ToCamelCase(dbName);
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Tests CRUD operations for all [Resource] entities in {dbName}DbContext.");
            sb.AppendLine($"        /// Uses reflection to discover entities and test them automatically.");
            sb.AppendLine($"        /// Executes all tests in a single transaction with proper dependency ordering.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        [Fact]");
            sb.AppendLine($"        public async Task TestAllStandardEntities_{dbName}DbContext_ShouldSucceed()");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            WriteSectionHeader(\"{dbName}DbContext Entity Tests (Transactional + Dependency-Aware)\");");
            sb.AppendLine();
            sb.AppendLine($"            // Get {dbName}DbContext from DI");
            sb.AppendLine($"            var {camelDbName}Context = GetService<{dbName}DbContext>();");
            sb.AppendLine();
            sb.AppendLine($"            // Discover all [Resource] entities from {dbName}DbContext");
            sb.AppendLine($"            var entityTypes = EntityReflectionHelper.DiscoverResourceEntities(typeof({dbName}DbContext));");
            sb.AppendLine($"            WriteLine($\"Found {{entityTypes.Count}} [Resource] entities in {dbName}DbContext\");");
            sb.AppendLine($"            WriteLine(\"\");");
            sb.AppendLine();
            sb.AppendLine($"            // Create runner and execute tests");
            sb.AppendLine($"            var runner = new TransactionalEntityTestRunner({camelDbName}Context, WriteLine);");
            sb.AppendLine($"            var report = await runner.RunTransactionalCrudTestAsync(entityTypes);");
            sb.AppendLine();
            sb.AppendLine($"            // Output summary");
            sb.AppendLine($"            WriteLine(report.GetSummary());");
            sb.AppendLine();
            sb.AppendLine($"            // Assert all tests passed");
            sb.AppendLine($"            if (!report.AllTestsPassed())");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                var detailedErrors = report.GetDetailedErrors();");
            sb.AppendLine($"                Assert.Fail($\"Some {dbName}DbContext entity tests failed:\\n{{detailedErrors}}\");");
            sb.AppendLine($"            }}");
            sb.AppendLine($"        }}");

            return sb.ToString();
        }

        /// <summary>
        /// Appends a test method to StandardEntityTests.cs file.
        /// Also adds the necessary using statement for the generated module.
        /// </summary>
        public bool AppendTestToStandardEntityTests(string testMethod, string usingNamespace, string testFilePath)
        {
            try
            {
                if (!File.Exists(testFilePath))
                {
                    Console.WriteLine($"Error: Test file not found: {testFilePath}");
                    return false;
                }

                // Read file content
                string content = File.ReadAllText(testFilePath);

                // Check if using statement already exists
                string usingStatement = $"using {usingNamespace};";
                if (!content.Contains(usingStatement))
                {
                    // Find insertion point for using statement (after last using)
                    int lastUsingIndex = content.LastIndexOf("using ");
                    if (lastUsingIndex != -1)
                    {
                        int endOfLineIndex = content.IndexOf('\n', lastUsingIndex);
                        if (endOfLineIndex != -1)
                        {
                            content = content.Insert(endOfLineIndex + 1, usingStatement + "\n");
                            Console.WriteLine($"  ✓ Added using statement: {usingStatement}");
                        }
                    }
                }

                // Find insertion point for test method (before last closing brace)
                int lastBraceIndex = content.LastIndexOf('}');
                if (lastBraceIndex == -1)
                {
                    Console.WriteLine("Error: Could not find closing brace in test file");
                    return false;
                }

                // Find the second-to-last closing brace (end of class, not namespace)
                int secondLastBraceIndex = content.LastIndexOf('}', lastBraceIndex - 1);
                if (secondLastBraceIndex == -1)
                {
                    Console.WriteLine("Error: Could not find class closing brace in test file");
                    return false;
                }

                // Insert test method before class closing brace
                content = content.Insert(secondLastBraceIndex, testMethod + "\n");

                // Write back to file
                File.WriteAllText(testFilePath, content);
                Console.WriteLine($"  ✓ Added test method to StandardEntityTests.cs");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending test to StandardEntityTests.cs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts string to camelCase (first letter lowercase).
        /// </summary>
        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
    }
}
