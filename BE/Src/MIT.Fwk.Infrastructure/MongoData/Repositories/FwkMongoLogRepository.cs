using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Data.NoSql;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MIT.Fwk.Infrastructure.Data.Repositories
{
    // FASE 8A: IMongoFwkLogRepository removed - internal implementation only
    public class FwkMongoLogRepository
    {
        public class PageQueue
        {
            public long Total { get; set; }
            public List<FwkLog> Items { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        private readonly MongoContext _context = null;

        public FwkMongoLogRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<bool> Remove(long id)
        {
            DeleteResult actionResult = await _context.FwkLogs.DeleteOneAsync(
                 Builders<FwkLog>.Filter.Eq("Id", id));

            return actionResult.IsAcknowledged
                && actionResult.DeletedCount > 0;
        }

        public async Task<bool> RemoveAll()
        {
            DeleteResult actionResult = await _context.FwkLogs.DeleteManyAsync(new BsonDocument());

            return actionResult.IsAcknowledged
                && actionResult.DeletedCount > 0;
        }

        // FASE 8A: Search() method removed - LookupFilter eliminated

        public async Task<T> Add<T>(T item) where T : IFwkLog
        {
            _context.FwkLogs.InsertOne(item as FwkLog);
            return (T)await Get(item.Id);

        }

        public async Task<T> Get<T>(long id) where T : IFwkLog
        {
            return (T)await Get(id);
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : IFwkLog
        {
            return (IEnumerable<T>)await GetAll();
        }



        private async Task<IFwkLog> Get(long id)
        {
            FilterDefinition<FwkLog> filter = Builders<FwkLog>.Filter.Eq("Id", id);

            return await _context.FwkLogs
                            .Find(filter)
                            .FirstOrDefaultAsync();
        }

        private async Task<IEnumerable<IFwkLog>> GetAll()
        {
            return await _context.FwkLogs.Find(_ => true).ToListAsync();
        }

    }
}
