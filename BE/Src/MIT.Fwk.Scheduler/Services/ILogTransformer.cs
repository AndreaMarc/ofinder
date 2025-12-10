using MIT.Fwk.Infrastructure.Entities;
using MongoDB.Bson;

namespace MIT.Fwk.Scheduler.Services
{
    /// <summary>
    /// Service for transforming LogToMongo objects to BsonDocuments
    /// </summary>
    public interface ILogTransformer
    {
        /// <summary>
        /// Transforms LogToMongo to BsonDocument for MongoDB insertion
        /// </summary>
        /// <param name="logToMongo">Source log object</param>
        /// <returns>BsonDocument ready for MongoDB insertion</returns>
        BsonDocument Transform(LogToMongo logToMongo);
    }
}
