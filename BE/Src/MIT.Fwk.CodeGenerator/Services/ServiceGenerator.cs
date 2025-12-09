using System.IO;
using System.Text;
using MIT.Fwk.CodeGenerator.Models;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Generates ManualService interface and implementation.
    /// Pattern: I{DbName}ManualService + {DbName}ManualService with generic CRUD methods.
    /// </summary>
    public class ServiceGenerator
    {
        /// <summary>
        /// Generates service interface code.
        /// </summary>
        public string GenerateServiceInterface(string dbName, string namespaceName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using JsonApiDotNetCore.Resources;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Interfaces");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{dbName}ManualService");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets all entities of type T.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        Task<List<T>> GetAllAsync<T, TId>()");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets entity by ID.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        Task<T> GetByIdAsync<T, TId>(TId id)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a new entity.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        Task<T> CreateAsync<T, TId>(T entity)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Updates an existing entity.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        Task<T> UpdateAsync<T, TId>(T entity)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible;");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deletes an entity by ID.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        Task<bool> DeleteAsync<T, TId>(TId id)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates service implementation code.
        /// </summary>
        public string GenerateServiceImplementation(string dbName, string namespaceName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using JsonApiDotNetCore.Resources;");
            sb.AppendLine($"using {namespaceName}.Data;");
            sb.AppendLine($"using {namespaceName}.Interfaces;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}.Services");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {dbName}ManualService : I{dbName}ManualService");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {dbName}DbContext _context;");
            sb.AppendLine();
            sb.AppendLine($"        public {dbName}ManualService({dbName}DbContext context)");
            sb.AppendLine("        {");
            sb.AppendLine("            _context = context;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GetAllAsync
            sb.AppendLine("        public async Task<List<T>> GetAllAsync<T, TId>()");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible");
            sb.AppendLine("        {");
            sb.AppendLine("            return await _context.Set<T>().AsNoTracking().ToListAsync();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // GetByIdAsync
            sb.AppendLine("        public async Task<T> GetByIdAsync<T, TId>(TId id)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible");
            sb.AppendLine("        {");
            sb.AppendLine("            return await _context.Set<T>().FindAsync(id);");
            sb.AppendLine("        }");
            sb.AppendLine();

            // CreateAsync
            sb.AppendLine("        public async Task<T> CreateAsync<T, TId>(T entity)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible");
            sb.AppendLine("        {");
            sb.AppendLine("            _context.Set<T>().Add(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("            return entity;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // UpdateAsync
            sb.AppendLine("        public async Task<T> UpdateAsync<T, TId>(T entity)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible");
            sb.AppendLine("        {");
            sb.AppendLine("            _context.Set<T>().Update(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("            return entity;");
            sb.AppendLine("        }");
            sb.AppendLine();

            // DeleteAsync
            sb.AppendLine("        public async Task<bool> DeleteAsync<T, TId>(TId id)");
            sb.AppendLine("            where T : class, IIdentifiable<TId>");
            sb.AppendLine("            where TId : IConvertible");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = await _context.Set<T>().FindAsync(id);");
            sb.AppendLine("            if (entity == null) return false;");
            sb.AppendLine();
            sb.AppendLine("            _context.Set<T>().Remove(entity);");
            sb.AppendLine("            await _context.SaveChangesAsync();");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Writes service interface to disk.
        /// </summary>
        public void WriteServiceInterfaceToDisk(string code, string interfacesPath, string dbName)
        {
            if (!Directory.Exists(interfacesPath))
                Directory.CreateDirectory(interfacesPath);

            string filePath = Path.Combine(interfacesPath, $"I{dbName}ManualService.cs");
            File.WriteAllText(filePath, code);
        }

        /// <summary>
        /// Writes service implementation to disk.
        /// </summary>
        public void WriteServiceImplementationToDisk(string code, string servicesPath, string dbName)
        {
            if (!Directory.Exists(servicesPath))
                Directory.CreateDirectory(servicesPath);

            string filePath = Path.Combine(servicesPath, $"{dbName}ManualService.cs");
            File.WriteAllText(filePath, code);
        }
    }
}
