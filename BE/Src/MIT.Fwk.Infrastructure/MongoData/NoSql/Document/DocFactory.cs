using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.NoSql.Document
{
    public class DocFactory : IDocFactory
    {

        #region CRUD

        public T CreateDocument<T>(T model) where T : IDocument
        {
            return DocumentManager.CreateAsync(model).Result;
        }

        public async Task<T> CreateDocumentAsync<T>(T model) where T : IDocument
        {
            return await DocumentManager.CreateAsync(model);
        }

        public T ReadDocument<T>(T model) where T : IDocument
        {
            return DocumentManager.ReadAsync(model.Id, model).Result;
        }

        public async Task<T> ReadDocumentAsync<T>(T model) where T : IDocument
        {
            return await DocumentManager.ReadAsync(model.Id, model);
        }

        public T ReadDocument<T>(long Id) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            Task<T> res = DocumentManager.ReadAsync(Id, template);
            return res.Result;
        }

        public async Task<T> ReadDocumentAsync<T>(long Id) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            return await DocumentManager.ReadAsync(Id, template);

        }

        public long CountAll<T>(int tenantId = 0) where T : IDocument
        {
            Task<IEnumerable<T>> res = DocumentManager.ListAsync<T>();
            return res.Result.Count();
        }

        public async Task<long> CountAllAsync<T>(int tenantId = 0) where T : IDocument
        {
            return (await DocumentManager.ListAsync<T>()).Count();
        }

        public IEnumerable<T> ListDocuments<T>(int tenantId) where T : IDocument
        {
            Task<IEnumerable<T>> res = DocumentManager.ListAsync<T>();
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId) where T : IDocument
        {
            return await DocumentManager.ListAsync<T>();
        }

        public IEnumerable<T> ListDocuments<T>(T template) where T : IDocument
        {
            Task<IEnumerable<T>> res = DocumentManager.ListAsync(template);
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(T template) where T : IDocument
        {
            return await DocumentManager.ListAsync(template);
        }

        public IEnumerable<T> ListDocuments<T>(int tenantId, Dictionary<string, object> filter) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            Task<IEnumerable<T>> res = DocumentManager.ListAsync(template, filter);
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId, Dictionary<string, object> filter) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            return await DocumentManager.ListAsync(template, filter);
        }

        public IEnumerable<T> ListDocuments<T>(T template, Dictionary<string, object> filter) where T : IDocument
        {
            Task<IEnumerable<T>> res = DocumentManager.ListAsync(template, filter);
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(T template, Dictionary<string, object> filter) where T : IDocument
        {
            return await DocumentManager.ListAsync(template, filter);
        }

        public IEnumerable<T> ListDocuments<T>(int tenantId, List<Dictionary<string, object>> filter) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            Task<IEnumerable<T>> res = DocumentManager.ListAsync(template, filter);
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId, List<Dictionary<string, object>> filter) where T : IDocument
        {
            T template = (T)ReflectionHelper.Resolve<T>();
            return await DocumentManager.ListAsync(template, filter);
        }

        public IEnumerable<T> ListDocuments<T>(T template, List<Dictionary<string, object>> filter) where T : IDocument
        {
            Task<IEnumerable<T>> res = DocumentManager.ListAsync(template, filter);
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListDocumentsAsync<T>(T template, List<Dictionary<string, object>> filter) where T : IDocument
        {
            return await DocumentManager.ListAsync(template, filter);
        }

        public bool UpdateDocument<T>(T model) where T : IDocument
        {
            return DocumentManager.UpdateAsync(model).Result;
        }

        public async Task<bool> UpdateDocumentAsync<T>(T model) where T : IDocument
        {
            return await DocumentManager.UpdateAsync(model);
        }

        public bool DeleteDocument<T>(T model) where T : IDocument
        {
            return DocumentManager.DeleteAsync(model).Result;
        }

        public async Task<bool> DeleteDocumentAsync<T>(T model) where T : IDocument
        {
            return await DocumentManager.DeleteAsync(model);
        }

        #endregion
    }
}
