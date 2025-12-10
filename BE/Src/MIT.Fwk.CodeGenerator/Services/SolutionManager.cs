using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Manages solution (.sln) and project reference updates.
    /// </summary>
    public class SolutionManager
    {
        private const string SolutionGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"; // C# project type GUID

        /// <summary>
        /// Adds a new project to the solution file under the "Extensions" solution folder.
        /// </summary>
        public bool AddProjectToSolution(string solutionPath, string projectPath, string projectName)
        {
            try
            {
                if (!File.Exists(solutionPath))
                {
                    Console.WriteLine($"Error: Solution file not found: {solutionPath}");
                    return false;
                }

                // Read solution content
                string solutionContent = File.ReadAllText(solutionPath);

                // Generate project GUID
                string projectGuid = Guid.NewGuid().ToString().ToUpper();

                // Calculate relative path from solution to project
                string solutionDir = Path.GetDirectoryName(solutionPath);
                string relativePath = GetRelativePath(solutionDir, projectPath);

                // Check if project already exists in solution
                if (solutionContent.Contains($"\"{projectName}\""))
                {
                    Console.WriteLine($"  ⚠️  Project '{projectName}' already exists in solution. Skipping.");
                    return true;
                }

                // Find or create "Extensions" solution folder
                string extensionsFolderGuid = GetOrCreateExtensionsFolder(ref solutionContent);

                // Create project entry
                var projectEntry = new StringBuilder();
                projectEntry.AppendLine($"Project(\"{SolutionGuid}\") = \"{projectName}\", \"{relativePath}\", \"{{{projectGuid}}}\"");
                projectEntry.AppendLine("EndProject");

                // Find insertion point (before "Global" section)
                int globalIndex = solutionContent.IndexOf("Global");
                if (globalIndex == -1)
                {
                    Console.WriteLine("Error: 'Global' section not found in solution file");
                    return false;
                }

                // Insert project entry
                solutionContent = solutionContent.Insert(globalIndex, projectEntry.ToString());

                // Add project to Extensions folder in solution configuration
                solutionContent = AddProjectToSolutionFolder(solutionContent, projectGuid, extensionsFolderGuid);

                // Write updated solution
                File.WriteAllText(solutionPath, solutionContent);
                Console.WriteLine($"  ✓ Added project to solution: {projectName}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding project to solution: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Adds a project reference to MIT.Fwk.WebApi.csproj.
        /// </summary>
        public bool AddProjectReferenceToWebApi(string webApiProjectPath, string newProjectPath, string projectName)
        {
            try
            {
                if (!File.Exists(webApiProjectPath))
                {
                    Console.WriteLine($"Error: WebApi project file not found: {webApiProjectPath}");
                    return false;
                }

                // Load project XML
                XDocument doc = XDocument.Load(webApiProjectPath);

                // Calculate relative path
                string webApiDir = Path.GetDirectoryName(webApiProjectPath);
                string relativePath = GetRelativePath(webApiDir, newProjectPath);

                // Check if reference already exists
                var existingRef = doc.Descendants("ProjectReference")
                    .FirstOrDefault(pr => pr.Attribute("Include")?.Value == relativePath);

                if (existingRef != null)
                {
                    Console.WriteLine($"  ⚠️  Project reference already exists in WebApi. Skipping.");
                    return true;
                }

                // Find or create ItemGroup for ProjectReference
                var itemGroup = doc.Descendants("ItemGroup")
                    .FirstOrDefault(ig => ig.Elements("ProjectReference").Any());

                if (itemGroup == null)
                {
                    // Create new ItemGroup
                    itemGroup = new XElement("ItemGroup");
                    doc.Root?.Add(itemGroup);
                }

                // Add ProjectReference
                var projectReference = new XElement("ProjectReference",
                    new XAttribute("Include", relativePath));

                itemGroup.Add(projectReference);

                // Save with formatting
                doc.Save(webApiProjectPath);
                Console.WriteLine($"  ✓ Added project reference to WebApi: {projectName}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding project reference: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets or creates the "Extensions" solution folder.
        /// </summary>
        private string GetOrCreateExtensionsFolder(ref string solutionContent)
        {
            // Check if Extensions folder already exists
            string pattern = "Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"Extensions\"";
            int folderIndex = solutionContent.IndexOf(pattern);

            if (folderIndex != -1)
            {
                // Extract existing GUID
                int guidStart = solutionContent.IndexOf("{", folderIndex + pattern.Length);
                int guidEnd = solutionContent.IndexOf("}", guidStart);
                return solutionContent.Substring(guidStart + 1, guidEnd - guidStart - 1);
            }

            // Create new Extensions folder
            string folderGuid = Guid.NewGuid().ToString().ToUpper();
            var folderEntry = new StringBuilder();
            folderEntry.AppendLine($"Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"Extensions\", \"Extensions\", \"{{{folderGuid}}}\"");
            folderEntry.AppendLine("EndProject");

            // Insert before Global
            int globalIndex = solutionContent.IndexOf("Global");
            solutionContent = solutionContent.Insert(globalIndex, folderEntry.ToString());

            return folderGuid;
        }

        /// <summary>
        /// Adds project to solution folder in the NestedProjects section.
        /// </summary>
        private string AddProjectToSolutionFolder(string solutionContent, string projectGuid, string folderGuid)
        {
            // Find NestedProjects section
            string nestedPattern = "GlobalSection(NestedProjects) = preSolution";
            int nestedIndex = solutionContent.IndexOf(nestedPattern);

            if (nestedIndex == -1)
            {
                // Create NestedProjects section if it doesn't exist
                string endGlobalPattern = "EndGlobal";
                int endGlobalIndex = solutionContent.IndexOf(endGlobalPattern);

                if (endGlobalIndex != -1)
                {
                    var nestedSection = new StringBuilder();
                    nestedSection.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
                    nestedSection.AppendLine($"\t\t{{{projectGuid}}} = {{{folderGuid}}}");
                    nestedSection.AppendLine("\tEndGlobalSection");

                    solutionContent = solutionContent.Insert(endGlobalIndex, nestedSection.ToString());
                }
            }
            else
            {
                // Add to existing NestedProjects section
                int endSectionIndex = solutionContent.IndexOf("EndGlobalSection", nestedIndex);
                string entry = $"\t\t{{{projectGuid}}} = {{{folderGuid}}}\n";
                solutionContent = solutionContent.Insert(endSectionIndex, entry);
            }

            return solutionContent;
        }

        /// <summary>
        /// Calculates relative path from one directory to another.
        /// </summary>
        private string GetRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // Convert forward slashes to backslashes for Windows
            return relativePath.Replace('/', '\\');
        }

        /// <summary>
        /// Updates both solution and WebApi project references.
        /// </summary>
        public bool UpdateSolutionAndReferences(string solutionPath, string webApiProjectPath,
            string newProjectPath, string projectName)
        {
            Console.WriteLine("\n📋 Updating solution and project references...");

            bool solutionUpdated = AddProjectToSolution(solutionPath, newProjectPath, projectName);
            bool referenceAdded = AddProjectReferenceToWebApi(webApiProjectPath, newProjectPath, projectName);

            if (solutionUpdated && referenceAdded)
            {
                Console.WriteLine("  ✅ Solution and references updated successfully");
                return true;
            }
            else
            {
                Console.WriteLine("  ⚠️  Some updates failed");
                return false;
            }
        }
    }
}
