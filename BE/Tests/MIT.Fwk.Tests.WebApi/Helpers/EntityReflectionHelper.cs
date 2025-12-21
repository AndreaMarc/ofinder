using Humanizer;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MIT.Fwk.Tests.WebApi.Helpers
{
    /// <summary>
    /// Helper for reflection-based entity operations.
    /// Discovers [Resource] entities, creates instances with default values, and extracts IDs.
    /// </summary>
    public static class EntityReflectionHelper
    {
        /// <summary>
        /// Discovers all types with [Resource] attribute from JsonApiDbContext only.
        /// Scans all MIT.* assemblies for DbContext types implementing IJsonApiDbContext,
        /// then extracts entity types from their DbSet properties.
        /// Excludes MITApplicationDbContext (Identity context) and custom DbContexts like OtherDbContext.
        /// </summary>
        public static List<Type> DiscoverResourceEntities()
        {
            var entityTypes = new HashSet<Type>();

            // Find all assemblies in the current domain that start with "MIT."
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("MIT.") == true)
                .ToList();

            foreach (var assembly in assemblies)
            {
                // Find all DbContext types that implement IJsonApiDbContext
                // ONLY include JsonApiDbContext - exclude custom contexts like OtherDbContext
                var dbContextTypes = assembly.GetTypes()
                    .Where(t => t.IsClass &&
                               !t.IsAbstract &&
                               typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(t) &&
                               typeof(MIT.Fwk.Core.Domain.Interfaces.IJsonApiDbContext).IsAssignableFrom(t) &&
                               t.Name == "JsonApiDbContext")  // Only JsonApiDbContext, not OtherDbContext or MITApplicationDbContext
                    .ToList();

                foreach (var dbContextType in dbContextTypes)
                {
                    // Get all DbSet<T> properties from this context
                    var dbSetProperties = dbContextType.GetProperties()
                        .Where(p => p.PropertyType.IsGenericType &&
                                   p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>))
                        .ToList();

                    foreach (var property in dbSetProperties)
                    {
                        // Get the entity type from DbSet<T>
                        var entityType = property.PropertyType.GetGenericArguments()[0];

                        // Check if entity has [Resource] attribute
                        if (entityType.GetCustomAttribute<ResourceAttribute>() != null)
                        {
                            entityTypes.Add(entityType);
                        }
                    }
                }
            }

            return entityTypes.ToList();
        }

        /// <summary>
        /// Discovers all types with [Resource] attribute from a specific DbContext type.
        /// Scans all MIT.* assemblies for the specified DbContext type,
        /// then extracts entity types from its DbSet properties.
        /// </summary>
        public static List<Type> DiscoverResourceEntities(Type targetDbContextType)
        {
            var entityTypes = new HashSet<Type>();

            // Validate input
            if (targetDbContextType == null || !typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(targetDbContextType))
            {
                throw new ArgumentException($"Type {targetDbContextType?.Name} is not a DbContext", nameof(targetDbContextType));
            }

            // Find all assemblies in the current domain that start with "MIT."
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("MIT.") == true)
                .ToList();

            foreach (var assembly in assemblies)
            {
                // Find the specific DbContext type
                var dbContextType = assembly.GetTypes()
                    .FirstOrDefault(t => t.IsClass &&
                                        !t.IsAbstract &&
                                        typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(t) &&
                                        t.Name == targetDbContextType.Name);

                if (dbContextType != null)
                {
                    // Get all DbSet<T> properties from this context
                    var dbSetProperties = dbContextType.GetProperties()
                        .Where(p => p.PropertyType.IsGenericType &&
                                   p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>))
                        .ToList();

                    foreach (var property in dbSetProperties)
                    {
                        // Get the entity type from DbSet<T>
                        var entityType = property.PropertyType.GetGenericArguments()[0];

                        // Check if entity has [Resource] attribute
                        if (entityType.GetCustomAttribute<NotMappedAttribute>() != null)
                        {
                            continue;
                        }

                        entityTypes.Add(entityType);
                    }

                    break; // Found the target DbContext, no need to continue
                }
            }

            return entityTypes.ToList();
        }

        /// <summary>
        /// Discovers all DbContext types that implement IJsonApiDbContext.
        /// Returns types like JsonApiDbContext, OtherDbContext, etc.
        /// Excludes MITApplicationDbContext (Identity context - not IJsonApiDbContext).
        /// </summary>
        public static List<Type> DiscoverAllJsonApiDbContexts()
        {
            var dbContextTypes = new HashSet<Type>();

            // Find all assemblies in the current domain that start with "MIT."
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.StartsWith("MIT.") == true)
                .ToList();

            foreach (var assembly in assemblies)
            {
                // Find all DbContext types that implement IJsonApiDbContext
                var contexts = assembly.GetTypes()
                    .Where(t => t.IsClass &&
                               !t.IsAbstract &&
                               typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(t) &&
                               typeof(MIT.Fwk.Core.Domain.Interfaces.IJsonApiDbContext).IsAssignableFrom(t))
                    .ToList();

                foreach (var context in contexts)
                {
                    dbContextTypes.Add(context);
                }
            }

            return dbContextTypes.ToList();
        }

        /// <summary>
        /// Gets all [Resource] entity names from the entity/getAll endpoint simulation.
        /// For now, uses reflection discovery instead of HTTP call.
        /// </summary>
        public static List<string> GetAllEntityNames()
        {
            var resourceTypes = DiscoverResourceEntities();
            return resourceTypes.Select(t => t.Name).ToList();
        }

        /// <summary>
        /// Creates an instance of an entity with default values for all [Attr] properties.
        /// Skips foreign key properties - they will be set later based on relationships.
        /// </summary>
        public static object CreateEntityInstance(Type entityType, Microsoft.EntityFrameworkCore.DbContext context)
        {
            var entity = Activator.CreateInstance(entityType)
                ?? throw new InvalidOperationException($"Cannot create instance of {entityType.Name}");

            // Get FK property names from EF Core model metadata
            var fkPropertyNames = GetForeignKeyPropertyNames(entityType, context);

            // Populate [Attr] properties with default values
            foreach (var property in entityType.GetProperties())
            {
                // Skip properties marked with [IdentityDB] (auto-generated by DB)
                if (property.GetCustomAttribute<IdentityDBAttribute>() != null)
                    continue;

                // Skip FK properties - will be set later based on relationships
                if (fkPropertyNames.Contains(property.Name))
                    continue;

                // Handle [Attr] properties
                if (property.GetCustomAttribute<AttrAttribute>() != null)
                {
                    var defaultValue = GetDefaultValueForType(property.PropertyType);
                    if (defaultValue != null)
                    {
                        property.SetValue(entity, defaultValue);
                    }
                }

                // Note: [HasOne] relationships are handled separately via SetRelationships
            }

            return entity;
        }

        /// <summary>
        /// Gets all foreign key property names for an entity using EF Core model metadata.
        /// Returns an empty set if the entity is not found in the model (e.g., from a different DbContext).
        /// </summary>
        private static HashSet<string> GetForeignKeyPropertyNames(Type entityType, Microsoft.EntityFrameworkCore.DbContext context)
        {
            var fkNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Get EF Core metadata for this entity type
                var efEntityType = context.Model.FindEntityType(entityType);
                if (efEntityType == null)
                {
                    // Entity not in this DbContext model - fallback to convention-based detection
                    return GetForeignKeyPropertyNamesByConvention(entityType);
                }

                // Get all foreign keys from EF Core metadata
                foreach (var foreignKey in efEntityType.GetForeignKeys())
                {
                    foreach (var property in foreignKey.Properties)
                    {
                        fkNames.Add(property.Name);
                    }
                }
            }
            catch
            {
                // If metadata access fails, fallback to convention-based detection
                return GetForeignKeyPropertyNamesByConvention(entityType);
            }

            return fkNames;
        }

        /// <summary>
        /// Fallback method: detects FK properties by naming convention (RelationshipName + "Id").
        /// Used when EF Core metadata is not available.
        /// </summary>
        private static HashSet<string> GetForeignKeyPropertyNamesByConvention(Type entityType)
        {
            var fkNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in entityType.GetProperties())
            {
                if (property.GetCustomAttribute<HasOneAttribute>() != null)
                {
                    // Convention: FK property = RelationshipName + "Id"
                    fkNames.Add(property.Name + "Id");
                }
            }

            return fkNames;
        }

        /// <summary>
        /// Gets a reasonable default value for a property type.
        /// </summary>
        private static object? GetDefaultValueForType(Type propertyType)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlyingType == typeof(string))
                return "test";
            else if (underlyingType == typeof(int) || underlyingType == typeof(decimal) ||
                     underlyingType == typeof(float) || underlyingType == typeof(double) ||
                     underlyingType == typeof(short) || underlyingType == typeof(long))
                return Convert.ChangeType(0, underlyingType);
            else if (underlyingType == typeof(DateTime))
                return DateTime.UtcNow;
            else if (underlyingType == typeof(DateTimeOffset))
                return DateTimeOffset.UtcNow;
            else if (underlyingType == typeof(DateOnly))
                return DateOnly.FromDateTime(DateTime.UtcNow);
            else if (underlyingType == typeof(bool))
                return true;
            else if (underlyingType == typeof(Guid))
                return Guid.NewGuid();
            else if (underlyingType.IsEnum)
            {
                // Handle enum types - return first enum value
                var enumValues = Enum.GetValues(underlyingType);
                if (enumValues.Length > 0)
                    return enumValues.GetValue(0);
            }
            else if (underlyingType == typeof(TimeSpan))
                return TimeSpan.FromHours(1); // Default 1 hour

            // For nullable types, return null if we don't have a default
            if (Nullable.GetUnderlyingType(propertyType) != null)
                return null;

            return null;
        }

        /// <summary>
        /// Extracts the ID value from an entity.
        /// Handles Identifiable&lt;int&gt;, Identifiable&lt;string&gt;, etc.
        /// </summary>
        public static object? GetEntityId(object entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            return idProperty?.GetValue(entity);
        }

        /// <summary>
        /// Sets the ID value for an entity.
        /// Useful for entities with string IDs (Guid) that need explicit ID setting.
        /// </summary>
        public static void SetEntityId(object entity, object id)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(entity, id);
            }
        }

        /// <summary>
        /// Gets the JSON:API endpoint name for an entity type.
        /// Example: "Category" -> "categories"
        /// </summary>
        public static string GetEndpointName(Type entityType)
        {
            var pluralName = entityType.Name.Pluralize();
            return Decapitalize(pluralName);
        }

        /// <summary>
        /// Gets the JSON:API type name for an entity.
        /// Example: "Category" -> "categories"
        /// </summary>
        public static string GetJsonApiTypeName(Type entityType)
        {
            return GetEndpointName(entityType);
        }

        /// <summary>
        /// Checks if an entity has a string-based ID (like Guid).
        /// </summary>
        public static bool HasStringId(Type entityType)
        {
            return entityType.IsSubclassOf(typeof(Identifiable<string>)) ||
                   entityType.IsSubclassOf(typeof(Identifiable<String>));
        }

        /// <summary>
        /// Gets all [HasOne] relationship properties for an entity type.
        /// Returns a dictionary of property name -> related entity type.
        /// </summary>
        public static Dictionary<string, Type> GetHasOneRelationships(Type entityType)
        {
            var relationships = new Dictionary<string, Type>();
            var properties = entityType.GetProperties();

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<HasOneAttribute>() != null && properties.FirstOrDefault(x => x.Name == property.Name + "Id") != null)
                {
                    relationships[Decapitalize(property.Name)] = property.PropertyType;
                }
            }

            return relationships;
        }

        /// <summary>
        /// Finds the first available entity of a given type in the database for relationship setup.
        /// Returns null if no entities found.
        /// </summary>
        public static async Task<object?> FindFirstEntityForRelationship(
            Type relatedEntityType,
            Microsoft.EntityFrameworkCore.DbContext context)
        {
            // Use reflection to get DbSet<T> from context
            var dbSetProperty = context.GetType().GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.DbSet<>) &&
                    p.PropertyType.GetGenericArguments()[0] == relatedEntityType);

            if (dbSetProperty == null)
                return null;

            // Get the DbSet
            var dbSet = dbSetProperty.GetValue(context);
            if (dbSet == null)
                return null;

            // Call FirstOrDefaultAsync using reflection
            var firstOrDefaultMethod = typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                .GetMethods()
                .FirstOrDefault(m => m.Name == "FirstOrDefaultAsync" &&
                                    m.GetParameters().Length == 2); // DbSet + CancellationToken

            if (firstOrDefaultMethod != null)
            {
                var genericMethod = firstOrDefaultMethod.MakeGenericMethod(relatedEntityType);
                var task = genericMethod.Invoke(null, new[] { dbSet, CancellationToken.None }) as Task;

                if (task != null)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
            }

            return null;
        }

        /// <summary>
        /// Decapitalizes the first letter of a string.
        /// Example: "Categories" -> "categories"
        /// </summary>
        private static string Decapitalize(string input)
        {
            return !string.IsNullOrEmpty(input) ? char.ToLower(input[0]) + input.Substring(1) : input;
        }
    }
}
