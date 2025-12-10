using MIT.Fwk.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.Repositories
{
    public interface IMongoRepository
    {
        Task<T> Add<T>(T item) where T : IDocument;

        Task<T> Get<T>(long id) where T : IDocument;

        Task<IEnumerable<T>> GetAll<T>(int tenantId, string size, List<string> guids) where T : IDocument;

        Task<long> CountAll<T>(int tenantId = 0);

        Task<bool> UpdateMeta(long id, string meta);

        Task<bool> Update<T>(long id, T item) where T : IDocument;

        Task<bool> Remove(long id);

        Task<bool> RemoveAll();
    }
}
