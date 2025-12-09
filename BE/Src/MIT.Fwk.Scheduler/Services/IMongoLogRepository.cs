using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Repository for inserting logs into MongoDB
    /// </summary>
    public interface IMongoLogRepository
    {
        /// <summary>
        /// Bulk inserts log documents into MongoDB
        /// </summary>
        /// <param name="documents">List of BsonDocuments to insert</param>
        /// <returns>Task representing the async operation</returns>
        Task InsertManyAsync(List<BsonDocument> documents);
    }
}
