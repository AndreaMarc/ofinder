using System.IO;
using System.Text;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates .csproj file for the generated module.
    /// </summary>
    public class ProjectFileGenerator
    {
        /// <summary>
        /// Generates project file (.csproj) content.
        /// </summary>
        public string GenerateProjectFile(string projectName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine();
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine("    <Nullable>disable</Nullable>");
            sb.AppendLine("    <LangVersion>latest</LangVersion>");
            sb.AppendLine($"    <AssemblyName>{projectName}</AssemblyName>");
            sb.AppendLine($"    <RootNamespace>{projectName}</RootNamespace>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <!-- Framework dependencies -->");
            sb.AppendLine("    <ProjectReference Include=\"..\\MIT.Fwk.Infrastructure\\MIT.Fwk.Infrastructure.csproj\" />");
            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine();
            sb.AppendLine("</Project>");

            return sb.ToString();
        }

        /// <summary>
        /// Writes project file to disk.
        /// </summary>
        public void WriteProjectFileToDisk(string code, string projectPath, string projectName)
        {
            if (!Directory.Exists(projectPath))
                Directory.CreateDirectory(projectPath);

            string filePath = Path.Combine(projectPath, $"{projectName}.csproj");
            File.WriteAllText(filePath, code);
        }
    }
}
