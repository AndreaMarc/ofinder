using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Implementation of IMongoLogRepository for inserting logs into MongoDB
    /// </summary>
    public class MongoLogRepository : IMongoLogRepository
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly ILogger<MongoLogRepository> _logger;

        public MongoLogRepository(
            IConfiguration configuration,
            IOptions<MongoLogOptions> options,
            ILogger<MongoLogRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            string connectionString = configuration.GetConnectionString("NoSQLConnection")
                ?? throw new InvalidOperationException("NoSQLConnection not found in configuration");

            string collectionName = options?.Value?.LogCollectionName
                ?? configuration["Logging:LogCollection"]
                ?? "FwkLog";

            // Parse connection string
            int lastSlashIndex = connectionString.LastIndexOf("/");
            string serverUrl = connectionString.Substring(0, lastSlashIndex);
            string databaseName = connectionString.Substring(lastSlashIndex + 1);

            MongoClient dbClient = new(serverUrl);
            IMongoDatabase db = dbClient.GetDatabase(databaseName);
            _collection = db.GetCollection<BsonDocument>(collectionName);

            _logger.LogInformation("MongoLogRepository initialized with collection: {CollectionName}", collectionName);
        }

        public async Task InsertManyAsync(List<BsonDocument> documents)
        {
            if (documents == null || documents.Count == 0)
            {
                return;
            }

            try
            {
                await _collection.InsertManyAsync(documents, new InsertManyOptions { IsOrdered = false });
                _logger.LogInformation("Inserted {Count} log documents into MongoDB", documents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert {Count} documents into MongoDB", documents.Count);
                throw;
            }
        }
    }
}
