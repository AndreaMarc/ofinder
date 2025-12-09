using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Infrastructure.Data.NoSql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.Repositories
{
    public class FwkLogFactory : IFwkLogFactory
    {
        readonly FwkMongoLogRepository _noSqlRepository;

        public FwkLogFactory(MongoContext context)
        {
            _noSqlRepository = new FwkMongoLogRepository(context);
        }

        #region CRUD

        public T CreateFwkLog<T>(T model) where T : IFwkLog
        {
            T doc = default;
            Task<T> task = _noSqlRepository.Add(model);
            return !task.IsFaulted ? task.Result : doc;

        }

        public async Task<T> CreateFwkLogAsync<T>(T model) where T : IFwkLog
        {
            return await _noSqlRepository.Add(model);
        }

        public T ReadFwkLog<T>(T model) where T : IFwkLog
        {
            return _noSqlRepository.Get<T>(model.Id).Result;
        }

        public async Task<T> ReadFwkLogAsync<T>(T model) where T : IFwkLog
        {
            return await _noSqlRepository.Get<T>(model.Id);
        }

        public T ReadFwkLog<T>(long Id) where T : IFwkLog
        {
            Task<T> res = _noSqlRepository.Get<T>(Id);
            return res.Result;
        }

        public async Task<T> ReadFwkLogAsync<T>(long Id) where T : IFwkLog
        {
            return await _noSqlRepository.Get<T>(Id);
        }

        public IEnumerable<T> ListFwkLogs<T>() where T : IFwkLog
        {
            Task<IEnumerable<T>> res = _noSqlRepository.GetAll<T>();
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>() where T : IFwkLog
        {
            return await _noSqlRepository.GetAll<T>();
        }

        public IEnumerable<T> ListFwkLogs<T>(T template) where T : IFwkLog
        {
            Task<IEnumerable<T>> res = _noSqlRepository.GetAll<T>();
            return res.Result;
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template) where T : IFwkLog
        {
            return await _noSqlRepository.GetAll<T>();
        }

        public IEnumerable<T> ListFwkLogs<T>(Dictionary<string, object> filter) where T : IFwkLog
        {
            List<T> docs = [];
            if (filter != null && filter.Keys.Count > 0)
            {
                if (filter.ContainsKey("logType"))
                {
                    docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filter["logType"].ToString())));
                }

                return docs;
            }
            else
            {
                return _noSqlRepository.GetAll<T>().Result;
            }
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>(Dictionary<string, object> filter) where T : IFwkLog
        {
            List<T> docs = [];
            if (filter != null && filter.Keys.Count > 0)
            {
                if (filter.ContainsKey("logType"))
                {
                    docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filter["logType"].ToString())));
                }

                return docs;
            }
            else
            {
                return await _noSqlRepository.GetAll<T>();
            }
        }

        public IEnumerable<T> ListFwkLogs<T>(T template, Dictionary<string, object> filter) where T : IFwkLog
        {
            List<T> docs = [];
            if (filter != null && filter.Keys.Count > 0)
            {
                if (filter.ContainsKey("logType"))
                {
                    docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filter["logType"].ToString())));
                }

                return docs;
            }
            else
            {
                return _noSqlRepository.GetAll<T>().Result;
            }
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template, Dictionary<string, object> filter) where T : IFwkLog
        {
            List<T> docs = [];
            if (filter != null && filter.Keys.Count > 0)
            {
                if (filter.ContainsKey("logType"))
                {
                    docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filter["logType"].ToString())));
                }

                return docs;
            }
            else
            {
                return await _noSqlRepository.GetAll<T>();
            }
        }

        public IEnumerable<T> ListFwkLogs<T>(List<Dictionary<string, object>> filter) where T : IFwkLog
        {
            List<T> docs = [];

            if (filter != null && filter.Count > 0)
            {
                foreach (Dictionary<string, object> filterVal in filter)
                {
                    if (filterVal.Keys.Count > 0)
                    {
                        if (filterVal.ContainsKey("logType"))
                        {
                            docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filterVal["logType"].ToString())));
                        }
                    }
                }

                return docs;
            }
            else
            {
                return _noSqlRepository.GetAll<T>().Result;
            }
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>(List<Dictionary<string, object>> filter) where T : IFwkLog
        {
            List<T> docs = [];

            if (filter != null && filter.Count > 0)
            {
                foreach (Dictionary<string, object> filterVal in filter)
                {
                    if (filterVal.Keys.Count > 0)
                    {
                        if (filterVal.ContainsKey("logType"))
                        {
                            docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filterVal["logType"].ToString())));
                        }
                    }
                }

                return docs;
            }
            else
            {
                return await _noSqlRepository.GetAll<T>();
            }
        }

        public IEnumerable<T> ListFwkLogs<T>(T template, List<Dictionary<string, object>> filter) where T : IFwkLog
        {
            List<T> docs = [];

            if (filter != null && filter.Count > 0)
            {
                foreach (Dictionary<string, object> filterVal in filter)
                {
                    if (filterVal.Keys.Count > 0)
                    {
                        if (filterVal.ContainsKey("logType"))
                        {
                            docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filterVal["logType"].ToString())));
                        }
                    }
                }

                return docs;
            }
            else
            {
                return _noSqlRepository.GetAll<T>().Result;
            }
        }

        public async Task<IEnumerable<T>> ListFwkLogsAsync<T>(T template, List<Dictionary<string, object>> filter) where T : IFwkLog
        {
            List<T> docs = [];

            if (filter != null && filter.Count > 0)
            {
                foreach (Dictionary<string, object> filterVal in filter)
                {
                    if (filterVal.Keys.Count > 0)
                    {
                        if (filterVal.ContainsKey("logType"))
                        {
                            docs.AddRange(_noSqlRepository.GetAll<T>().Result.Where(doc => doc.LogType.Contains(filterVal["logType"].ToString())));
                        }
                    }
                }

                return docs;
            }
            else
            {
                return await _noSqlRepository.GetAll<T>();
            }
        }


        public bool DeleteFwkLog<T>(T model) where T : IFwkLog
        {
            return _noSqlRepository.Remove(model.Id).Result;
        }

        public async Task<bool> DeleteFwkLogAsync<T>(T model) where T : IFwkLog
        {
            return await _noSqlRepository.Remove(model.Id);
        }

        #endregion
    }
}
