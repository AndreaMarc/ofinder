using MIT.Fwk.Core.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Core.Data
{
    /// <summary>
    /// Factory interface for document CRUD operations.
    /// Provides abstraction over document repository for MongoDB storage.
    /// </summary>
    public interface IDocFactory
    {
        T CreateDocument<T>(T model) where T : IDocument;
        Task<T> CreateDocumentAsync<T>(T model) where T : IDocument;

        T ReadDocument<T>(T model) where T : IDocument;
        Task<T> ReadDocumentAsync<T>(T model) where T : IDocument;
        T ReadDocument<T>(long Id) where T : IDocument;
        Task<T> ReadDocumentAsync<T>(long Id) where T : IDocument;

        long CountAll<T>(int tenantId = 0) where T : IDocument;
        Task<long> CountAllAsync<T>(int tenantId = 0) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(int tenantId) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(T template) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(T template) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(int tenantId, Dictionary<string, object> filter) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId, Dictionary<string, object> filter) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(T template, Dictionary<string, object> filter) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(T template, Dictionary<string, object> filter) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(int tenantId, List<Dictionary<string, object>> filter) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(int tenantId, List<Dictionary<string, object>> filter) where T : IDocument;

        IEnumerable<T> ListDocuments<T>(T template, List<Dictionary<string, object>> filter) where T : IDocument;
        Task<IEnumerable<T>> ListDocumentsAsync<T>(T template, List<Dictionary<string, object>> filter) where T : IDocument;

        bool UpdateDocument<T>(T model) where T : IDocument;
        Task<bool> UpdateDocumentAsync<T>(T model) where T : IDocument;

        bool DeleteDocument<T>(T model) where T : IDocument;
        Task<bool> DeleteDocumentAsync<T>(T model) where T : IDocument;
    }

    /// <summary>
    /// Factory interface for framework log CRUD operations.
    /// Provides abstraction over log repository for MongoDB storage.
    /// </summary>
    public interface IFwkLogFactory
    {
        T CreateFwkLog<T>(T model) where T : IFwkLog;
        Task<T> CreateFwkLogAsync<T>(T model) where T : IFwkLog;

        T ReadFwkLog<T>(T model) where T : IFwkLog;
        Task<T> ReadFwkLogAsync<T>(T model) where T : IFwkLog;
        T ReadFwkLog<T>(long Id) where T : IFwkLog;
        Task<T> ReadFwkLogAsync<T>(long Id) where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>() where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>() where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>(T template) where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template) where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>(Dictionary<string, object> filter) where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>(Dictionary<string, object> filter) where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>(T template, Dictionary<string, object> filter) where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template, Dictionary<string, object> filter) where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>(List<Dictionary<string, object>> filter) where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>(List<Dictionary<string, object>> filter) where T : IFwkLog;

        IEnumerable<T> ListFwkLogs<T>(T template, List<Dictionary<string, object>> filter) where T : IFwkLog;
        Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template, List<Dictionary<string, object>> filter) where T : IFwkLog;

        bool DeleteFwkLog<T>(T model) where T : IFwkLog;
        Task<bool> DeleteFwkLogAsync<T>(T model) where T : IFwkLog;
    }
}
