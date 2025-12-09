using Microsoft.Extensions.Configuration;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Services;
using MongoDB.Driver;

namespace MIT.Fwk.Infrastructure.Data.NoSql
{
    public class MongoContext
    {
        protected readonly IMongoDatabase _database = null;
        private readonly IConfiguration _configuration;

        public IMongoDatabase Database => _database;

        public MongoContext(IConnectionStringProvider connectionStringProvider, IConfiguration configuration)
        {
            _configuration = configuration;

            MongoClient client = new(connectionStringProvider.NoSQLConnectionString);
            string dbName = connectionStringProvider.NoSQLConnectionString.Substring(connectionStringProvider.NoSQLConnectionString.LastIndexOf("/") + 1);

            if (client != null)
            {
                _database = client.GetDatabase(dbName);
            }
        }

        public IMongoCollection<DocumentFile> Documents
        {
            get
            {
                return _database.GetCollection<DocumentFile>("Document");
            }
        }

        public IMongoCollection<FwkLog> FwkLogs
        {
            get
            {
                return _database.GetCollection<FwkLog>(_configuration["LogCollection"]);
            }
        }
    }

}
