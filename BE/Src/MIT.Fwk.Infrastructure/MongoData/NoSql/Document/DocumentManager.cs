using Microsoft.Extensions.Configuration;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.NoSql.Document
{
    /// <summary>
    /// DocumentManager refactored to use MongoDB instead of SQL Server.
    /// Provides CRUD operations for IDocument entities (DocumentFile, etc.)
    /// </summary>
    public class DocumentManager
    {
        private static IMongoDatabase _database;
        private static long _idCounter = 0;
        private static readonly object _lockObject = new object();

        // Helper method to get MongoDB database instance
        private static IMongoDatabase GetDatabase()
        {
            if (_database == null)
            {
                var config = GetConfiguration();
                var connectionString = config.GetConnectionString("NoSQLConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("NoSQLConnection not found in configuration");
                }

                var client = new MongoClient(connectionString);
                var dbName = connectionString.Substring(connectionString.LastIndexOf("/") + 1);
                _database = client.GetDatabase(dbName);
            }

            return _database;
        }

        // Helper method to get configuration for static context
        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

        // Get MongoDB collection for document type
        private static IMongoCollection<T> GetCollection<T>() where T : IDocument
        {
            var db = GetDatabase();
            return db.GetCollection<T>("Document");
        }

        // Generate sequential ID (MongoDB-style but as long for compatibility)
        private static long GenerateId<T>() where T : IDocument
        {
            lock (_lockObject)
            {
                var collection = GetCollection<T>();

                // Try to get the maximum ID from existing documents
                var maxDoc = collection.Find(FilterDefinition<T>.Empty)
                    .SortByDescending(d => d.Id)
                    .Limit(1)
                    .FirstOrDefault();

                if (maxDoc != null && maxDoc.Id > _idCounter)
                {
                    _idCounter = maxDoc.Id;
                }

                return ++_idCounter;
            }
        }

        #region CRUD Operations

        /// <summary>
        /// Create a new document in MongoDB
        /// </summary>
        public static async Task<T> CreateAsync<T>(T entity) where T : IDocument
        {
            try
            {
                var collection = GetCollection<T>();

                // Generate ID if not set
                if (entity.Id == 0)
                {
                    entity.Id = GenerateId<T>();
                }

                await collection.InsertOneAsync(entity);

                Console.WriteLine($"[DocumentManager] Created document with ID: {entity.Id}");

                return entity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DocumentManager.CreateAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Read a document by ID
        /// </summary>
        public static async Task<T> ReadAsync<T>(long id, T template = default) where T : IDocument
        {
            try
            {
                var collection = GetCollection<T>();
                var filter = Builders<T>.Filter.Eq(d => d.Id, id);

                var result = await collection.Find(filter).FirstOrDefaultAsync();

                if (result == null)
                {
                    Console.WriteLine($"[DocumentManager] Document with ID {id} not found");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DocumentManager.ReadAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// List all documents
        /// </summary>
        public static async Task<IEnumerable<T>> ListAsync<T>(T template = default, IEnumerable<IDictionary<string, object>> filter = null) where T : IDocument
        {
            try
            {
                var collection = GetCollection<T>();

                FilterDefinition<T> mongoFilter = FilterDefinition<T>.Empty;

                // Build filter if provided
                if (filter != null && filter.Any())
                {
                    var filterBuilder = Builders<T>.Filter;
                    var orFilters = new List<FilterDefinition<T>>();

                    foreach (var filterDict in filter)
                    {
                        var andFilters = new List<FilterDefinition<T>>();

                        foreach (var kvp in filterDict)
                        {
                            if (kvp.Value != null)
                            {
                                // Build filter based on property name
                                var propertyFilter = BuildPropertyFilter<T>(kvp.Key, kvp.Value);
                                if (propertyFilter != null)
                                {
                                    andFilters.Add(propertyFilter);
                                }
                            }
                        }

                        if (andFilters.Any())
                        {
                            orFilters.Add(filterBuilder.And(andFilters));
                        }
                    }

                    if (orFilters.Any())
                    {
                        mongoFilter = filterBuilder.Or(orFilters);
                    }
                }

                var results = await collection.Find(mongoFilter).ToListAsync();

                Console.WriteLine($"[DocumentManager] Listed {results.Count} documents");

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DocumentManager.ListAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// List documents with single filter dictionary
        /// </summary>
        public static async Task<IEnumerable<T>> ListAsync<T>(T template, IDictionary<string, object> filter) where T : IDocument
        {
            IList<IDictionary<string, object>> newFilter = null;

            if (filter != null)
            {
                newFilter = new List<IDictionary<string, object>> { filter };
            }

            return await ListAsync(template, newFilter);
        }

        /// <summary>
        /// Update an existing document
        /// </summary>
        public static async Task<bool> UpdateAsync<T>(T entity) where T : IDocument
        {
            try
            {
                var collection = GetCollection<T>();
                var filter = Builders<T>.Filter.Eq(d => d.Id, entity.Id);

                var result = await collection.ReplaceOneAsync(filter, entity);

                if (result.ModifiedCount > 0)
                {
                    Console.WriteLine($"[DocumentManager] Updated document with ID: {entity.Id}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[DocumentManager] No document found with ID: {entity.Id}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DocumentManager.UpdateAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        public static async Task<bool> DeleteAsync<T>(T entity) where T : IDocument
        {
            try
            {
                var collection = GetCollection<T>();
                var filter = Builders<T>.Filter.Eq(d => d.Id, entity.Id);

                var result = await collection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    Console.WriteLine($"[DocumentManager] Deleted document with ID: {entity.Id}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[DocumentManager] No document found with ID: {entity.Id}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DocumentManager.DeleteAsync: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Build MongoDB filter for a property
        /// </summary>
        private static FilterDefinition<T> BuildPropertyFilter<T>(string propertyName, object value) where T : IDocument
        {
            var filterBuilder = Builders<T>.Filter;

            // Handle common IDocument properties
            switch (propertyName.ToLower())
            {
                case "id":
                    if (long.TryParse(value.ToString(), out long id))
                        return filterBuilder.Eq(d => d.Id, id);
                    break;

                case "tenantid":
                    if (int.TryParse(value.ToString(), out int tenantId))
                        return filterBuilder.Eq(d => d.TenantId, tenantId);
                    break;

                case "title":
                    return filterBuilder.Eq(d => d.Title, value.ToString());

                case "description":
                    return filterBuilder.Eq(d => d.Description, value.ToString());

                case "filename":
                    return filterBuilder.Eq(d => d.FileName, value.ToString());

                case "extension":
                    return filterBuilder.Eq(d => d.Extension, value.ToString());

                case "fileguid":
                    return filterBuilder.Eq(d => d.FileGuid, value.ToString());

                case "meta":
                    return filterBuilder.Eq(d => d.Meta, value.ToString());
            }

            // If property not recognized, return null
            return null;
        }

        #endregion
    }
}
