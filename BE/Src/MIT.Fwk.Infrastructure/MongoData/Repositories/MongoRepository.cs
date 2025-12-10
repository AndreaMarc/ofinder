using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Data.NoSql;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.Repositories
{
    public class MongoRepository : IMongoRepository
    {
        public class PageQueue
        {
            public long Total { get; set; }
            public List<DocumentFile> Items { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        private readonly MongoContext _context = null;

        public MongoRepository(MongoContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateMeta(long id, string meta)
        {
            FilterDefinition<DocumentFile> filter = Builders<DocumentFile>.Filter.Eq(s => s.Id, id);
            UpdateDefinition<DocumentFile> update = Builders<DocumentFile>.Update
                            .Set(s => s.Meta, meta)
                            .CurrentDate(s => s.Meta);

            UpdateResult actionResult = await _context.Documents.UpdateOneAsync(filter, update);

            return actionResult.IsAcknowledged
                && actionResult.ModifiedCount > 0;
        }

        public async Task<bool> Remove(long id)
        {
            DeleteResult actionResult = await _context.Documents.DeleteOneAsync(
                 Builders<DocumentFile>.Filter.Eq("Id", id));

            return actionResult.IsAcknowledged
                && actionResult.DeletedCount > 0;
        }

        public async Task<bool> RemoveAll()
        {
            DeleteResult actionResult = await _context.Documents.DeleteManyAsync(new BsonDocument());

            return actionResult.IsAcknowledged
                && actionResult.DeletedCount > 0;
        }

        // FASE 8A: Search() method removed - LookupFilter eliminated

        public async Task<T> Add<T>(T item) where T : IDocument
        {
            _context.Documents.InsertOne(item as DocumentFile);
            return (T)await Get(item.Id);
        }

        public async Task<T> Get<T>(long id) where T : IDocument
        {
            return (T)await Get(id);
        }

        public async Task<IEnumerable<T>> GetAll<T>(int tenantId, string size = "", List<string> guids = null) where T : IDocument
        {
            return (IEnumerable<T>)await GetAll(tenantId, size, guids);
        }

        public async Task<long> CountAll<T>(int tenantId = 0)
        {
            return tenantId > 0 ? (await GetAll(tenantId)).Count() : _context.Documents.CountDocuments(_ => true);
        }

        public async Task<bool> Update<T>(long id, T item) where T : IDocument
        {
            ReplaceOneResult actionResult = await _context.Documents
                                            .ReplaceOneAsync(n => n.Id.Equals(id)
                                                            , item as DocumentFile
                                                            , new ReplaceOptions { IsUpsert = true });
            return actionResult.IsAcknowledged
                && actionResult.ModifiedCount > 0;
        }

        private async Task<IDocument> Get(long id)
        {
            FilterDefinition<DocumentFile> filter = Builders<DocumentFile>.Filter.Eq("Id", id);

            return await _context.Documents
                            .Find(filter)
                            .FirstOrDefaultAsync();
        }

        private async Task<IEnumerable<IDocument>> GetAll(int tenantId, string size = "", List<string> guids = null)
        {
            if (guids == null)
            {
                return await _context.Documents.Find(doc => doc.TenantId == tenantId).ToListAsync();
            }

            switch (size)
            {
                case "sm":
                    ProjectionDefinition<DocumentFile, DocumentFile> smallProjection = Builders<DocumentFile>.Projection.Expression(x => new DocumentFile { SmallFormat = x.SmallFormat, FileGuid = x.FileGuid, FileBase64 = x.FileBase64 });
                    return await _context.Documents.Find(doc => doc.TenantId == tenantId && guids.Contains(doc.FileGuid)).Project(smallProjection).ToListAsync();
                case "md":
                    ProjectionDefinition<DocumentFile, DocumentFile> mediumProjection = Builders<DocumentFile>.Projection.Expression(x => new DocumentFile { MediumFormat = x.MediumFormat, FileGuid = x.FileGuid, FileBase64 = x.FileBase64 });
                    return await _context.Documents.Find(doc => doc.TenantId == tenantId && guids.Contains(doc.FileGuid)).Project(mediumProjection).ToListAsync();
                case "lg":
                    ProjectionDefinition<DocumentFile, DocumentFile> bigProjection = Builders<DocumentFile>.Projection.Expression(x => new DocumentFile { BigFormat = x.BigFormat, FileGuid = x.FileGuid, FileBase64 = x.FileBase64 });
                    return await _context.Documents.Find(doc => doc.TenantId == tenantId && guids.Contains(doc.FileGuid)).Project(bigProjection).ToListAsync();
                default:
                    return await _context.Documents.Find(doc => doc.TenantId == tenantId && guids.Contains(doc.FileGuid)).ToListAsync();
            }

        }

    }
}
