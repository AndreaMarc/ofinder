using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Data.NoSql.Document
{
    public class DocumentManager
    {
        // FASE 7: Helper method to get configuration for static context
        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

        #region Mapper

        // Legacy method - IEntityTypeConfiguration removed
        // Returns hardcoded DatabaseInformations for Document entity
        private static DatabaseInformations GetDocumentDatabaseInformations()
        {
            IConfiguration config = GetConfiguration();
            bool enableSQLBC = config.GetValue<bool>("SQLBackwardCompatibility", false);

            return new DatabaseInformations()
            {
                IsTable = true,
                ObjectName = enableSQLBC ? "MITDocumentsBC" : "MITDocuments",
                EntityName = "Document",

                EntityDbMappings = new Dictionary<string, ColumnDetail>() {
                    {
                        "Id"
                    ,   new ColumnDetail() {
                        Name = "ID", TypeName = "bigint", IsKey = true, IsRequired = true, MaxLength = 8, IsIdentity = true
                        }   },
                    {
                        "Title"
                    ,   new ColumnDetail() {
                        Name = "Title", TypeName = "varchar", IsKey = false, IsRequired = false, MaxLength = 50
                        }   },
                    {
                        "Description"
                    ,   new ColumnDetail() {
                        Name = "Description", TypeName = "varchar", IsKey = false, IsRequired = false, MaxLength = 500
                        }   },
                    {
                        "FileName"
                    ,   new ColumnDetail() {
                        Name = "FileName", TypeName = "varchar", IsKey = false, IsRequired = true, MaxLength = 200
                        }   },
                    {
                        "Extension"
                    ,   new ColumnDetail() {
                        Name = "Extension", TypeName = "varchar", IsKey = false, IsRequired = true, MaxLength = 5
                        }   },
                    {
                        "BinaryData"
                    ,   new ColumnDetail() {
                        Name = "BinaryData", TypeName = enableSQLBC? "varbinary": "image", IsKey = false, IsRequired = true, MaxLength = enableSQLBC? -1: 16
                        }   },
                    {
                        "Meta"
                    ,   new ColumnDetail() {
                        Name = "Meta", TypeName = "varchar", IsKey = false, IsRequired = false, MaxLength = 250
                        }   },
                },
            };
        }

        #endregion

        #region CRUD

        public static async Task<IEnumerable<T>> ListAsync<T>(T template, IDictionary<string, object> filter) where T : IDocument
        {
            IList<IDictionary<string, object>> newFilter = null;

            if (filter != null)
            {
                newFilter = [filter];
            }

            return await ListAsync(template, newFilter);
        }

        public static async Task<IEnumerable<T>> ListAsync<T>(T template = default, IEnumerable<IDictionary<string, object>> filter = null) where T : IDocument
        {
            Type tp = template != null ? template.GetType() : typeof(T);

            IConfiguration config = GetConfiguration();
            using SqlConnection connection = new(config.GetConnectionString("DocumentDB"));
            connection.Open();

            string sql = FormatListQuery(filter, out IDictionary<string, object> dynamicFilter, out DatabaseInformations dbInfo);

            dynamic dynFilter = dynamicFilter;

            IEnumerable<dynamic> rows = await connection.QueryAsync(sql, (object)dynFilter);

            List<T> list = [];
            foreach (dynamic row in rows)
            {
                object obj = ReflectionHelper.CreateInstance(tp);

                IDictionary<string, object> data = (IDictionary<string, object>)row;

                foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
                {
                    obj.SetPropertyValue(mapInfo.Key, data[mapInfo.Value.Name]);
                }

                list.Add((T)obj);
            }

            return list;
        }

        public static async Task<T> ReadAsync<T>(long id, T template = default, SqlConnection conn = null, SqlTransaction trans = null) where T : IDocument
        {
            Type tp = template != null ? template.GetType() : typeof(T);

            string sql = FormatReadQuery(id, out IDictionary<string, object> dynamicFilter, out DatabaseInformations dbInfo);

            dynamic filter = dynamicFilter;


            if (trans != null && conn != null)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                IEnumerable<dynamic> rows = await conn.QueryAsync(sql, (object)filter, trans);
                if (!rows.Any())
                {
                    return default;
                }

                dynamic row = rows.Single();

                object obj = ReflectionHelper.CreateInstance(tp);

                IDictionary<string, object> data = (IDictionary<string, object>)row;

                foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
                {
                    obj.SetPropertyValue(mapInfo.Key, data[mapInfo.Value.Name]);
                }

                return (T)obj;
            }
            else
            {
                IConfiguration config = GetConfiguration();
            using SqlConnection connection = new(config.GetConnectionString("DocumentDB"));
                connection.Open();

                IEnumerable<dynamic> rows = await connection.QueryAsync(sql, (object)filter);
                if (!rows.Any())
                {
                    return default;
                }

                dynamic row = rows.Single();

                object obj = ReflectionHelper.CreateInstance(tp);

                IDictionary<string, object> data = (IDictionary<string, object>)row;

                foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
                {
                    obj.SetPropertyValue(mapInfo.Key, data[mapInfo.Value.Name]);
                }

                return (T)obj;
            }
        }

        public static async Task<T> CreateAsync<T>(T entity, SqlConnection conn = null, SqlTransaction trans = null) where T : IDocument
        {
            string sql = FormatCreateQuery(entity, out IDictionary<string, object> dynamicOptions, out bool returnIdentity, out _);

            dynamic param = dynamicOptions;
            if (trans != null && conn != null)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (returnIdentity)
                {
                    IEnumerable<int> result = await conn.QueryAsync<int>(sql, (object)param, trans);

                    long generatedKey = result.FirstOrDefault();
                    return await ReadAsync(generatedKey, entity, conn, trans);
                }
                else
                {
                    _ = conn.ExecuteAsync(sql, (object)param, trans);
                    return await ReadAsync(entity.Id, entity, conn, trans);
                }

            }
            else
            {
                IConfiguration config = GetConfiguration();
            using SqlConnection connection = new(config.GetConnectionString("DocumentDB"));
                connection.Open();
                if (returnIdentity)
                {
                    long generatedKey = 0;
                    try
                    {

                        IEnumerable<int> result = await connection.QueryAsync<int>(sql, (object)param);

                        generatedKey = result.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] DocumentManager.CreateAsync: {ex.Message}");
                    }
                    return await ReadAsync(generatedKey, entity);
                }
                else
                {
                    _ = await connection.ExecuteAsync(sql, (object)param);

                    return await ReadAsync(entity.Id, entity);
                }
            }
        }

        public static async Task<bool> UpdateAsync<T>(T entity) where T : IDocument
        {

            IConfiguration config = GetConfiguration();
            using SqlConnection connection = new(config.GetConnectionString("DocumentDB"));
            connection.Open();

            string sql = FormatUpdateQuery(entity, out IDictionary<string, object> dynamicOptions);

            dynamic param = dynamicOptions;

            int result = await connection.ExecuteAsync(sql, (object)param);
            return result > 0;
        }

        public static async Task<bool> DeleteAsync<T>(T entity) where T : IDocument
        {

            IConfiguration config = GetConfiguration();
            using SqlConnection connection = new(config.GetConnectionString("DocumentDB"));
            connection.Open();

            string sql = FormatDeleteQuery(entity, out IDictionary<string, object> dynamicOptions);

            dynamic param = dynamicOptions;

            int result = await connection.ExecuteAsync(sql, (object)param);
            return result > 0;
        }

        #endregion

        #region SQL

        protected static string FormatListQuery(IEnumerable<IDictionary<string, object>> filter, out IDictionary<string, object> dynamicFilter, out DatabaseInformations dbInfo)
        {
            StringBuilder query = new();

            // Legacy: IEntityTypeConfiguration removed - use hardcoded DatabaseInformations
            dbInfo = GetDocumentDatabaseInformations();

            query.Append(string.Format(" select '{0}'", DateTime.UtcNow.ToUniversalTime()));

            foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
            {
                if (mapInfo.Value.TypeName != "image"
                    && mapInfo.Value.TypeName != "varbinary")
                {
                    query.Append(string.Format(", {0}", mapInfo.Value.Name));
                }
            }

            query.Append(" from ");
            query.Append(dbInfo.ObjectName);

            query.Append(" where 1=1 ");

            dynamicFilter = new ExpandoObject();

            int i = 0;

            if (filter != null)
            {
                //query.Append(" AND ( 1=1 ");
                query.Append(" AND ( ");

                foreach (IDictionary<string, object> row in filter)
                {

                    query.Append(" ( 1=1 "); //AND

                    foreach (KeyValuePair<string, object> field in row)
                    {

                        if (field.Value != null && field.Value.GetType().IsArray)
                        {
                            query.Append(string.Format(" AND {0} IN @{1}", PropertyToDb(dbInfo, field.Key), i));// string.Format("{0}{1}",field.Key,i)));
                        }
                        else
                        {
                            if (field.Value == null)
                            {
                                query.Append(string.Format(" AND {0} IS NULL", PropertyToDb(dbInfo, field.Key)));
                            }
                            else
                            {
                                query.Append(string.Format(" AND {0}=@{1}", PropertyToDb(dbInfo, field.Key), i));
                            }

                        }

                        dynamicFilter[i.ToString()] = field.Value;
                        i++;
                    }

                    query.Append(") OR ");

                }

                query.Append(" ( 1=0 ))");
            }

            return query.ToString();
        }

        protected static string FormatCreateQuery<T>(T entity, out IDictionary<string, object> dynamicOptions, out bool returnIdentity, out string identityKey) where T : IDocument
        {
            StringBuilder query = new();
            StringBuilder fields = new();
            StringBuilder values = new();
            identityKey = "ID";

            DatabaseInformations dbInfo = GetDocumentDatabaseInformations();

            KeyValuePair<string, ColumnDetail> identityCol = dbInfo.EntityDbMappings.FirstOrDefault(col => col.Value.IsIdentity);

            returnIdentity = identityCol.Key != null;

            if (returnIdentity)
            {
                identityKey = identityCol.Key;
            }

            dynamicOptions = new ExpandoObject();

            foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
            {
                object val = ReflectionHelper.GetPropertyValue(entity, mapInfo.Key);
                if (val != null)
                {
                    if (returnIdentity && !mapInfo.Value.IsIdentity || !returnIdentity)
                    {
                        if (!mapInfo.Value.TypeName.Equals("datetime") || mapInfo.Value.TypeName.Equals("datetime") && !DateTime.MinValue.Equals(val))
                        {
                            fields.Append(string.Format(" {0},", mapInfo.Value.Name));
                            values.Append(string.Format(" @{0},", mapInfo.Key));
                            dynamicOptions[mapInfo.Key] = val;
                        }
                    }
                }
            }

            string fieldsStr = fields.ToString();
            string valuesStr = values.ToString();

            if (fieldsStr.EndsWith(','))
            {
                fieldsStr = fieldsStr[..^1];
            }

            if (valuesStr.EndsWith(','))
            {
                valuesStr = valuesStr[..^1];
            }

            query.Append(string.Format("INSERT INTO {0} ({1}) VALUES ({2}){3}", dbInfo.ObjectName, fieldsStr, valuesStr, returnIdentity ? "; SELECT CAST(SCOPE_IDENTITY() AS INT)" : ""));

            return query.ToString();
        }

        protected static string FormatReadQuery(long id, out IDictionary<string, object> dynamicFilter, out DatabaseInformations dbInfo)
        {
            StringBuilder query = new();

            dbInfo = GetDocumentDatabaseInformations();

            query.Append(string.Format(" select '{0}'", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff z")));

            foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
            {
                query.Append(string.Format(", {0}", mapInfo.Value.Name));
            }

            query.Append(" from ");
            query.Append(dbInfo.ObjectName);

            query.Append(" where ID=@ID ");

            dynamicFilter = new ExpandoObject();
            dynamicFilter["ID"] = id;

            return query.ToString();
        }

        protected static string FormatUpdateQuery<T>(T entity, out IDictionary<string, object> dynamicOptions)
        {
            StringBuilder query = new();
            StringBuilder fields = new();
            StringBuilder keys = new();

            //Type tp = typeof(T);
            DatabaseInformations dbInfo = GetDocumentDatabaseInformations();

            dynamicOptions = new ExpandoObject();

            foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
            {
                object val = ReflectionHelper.GetPropertyValue(entity, mapInfo.Key);
                if (val != null)
                {
                    if (mapInfo.Value.IsKey)
                    {
                        keys.Append(string.Format(" AND {0}=@{1}", mapInfo.Value.Name, mapInfo.Key));
                    }
                    else if (!mapInfo.Value.IsIdentity)
                    {
                        fields.Append(string.Format(" {0}=@{1},", mapInfo.Value.Name, mapInfo.Key));
                    }

                    dynamicOptions[mapInfo.Key] = val;
                }
            }

            string fieldsStr = fields.ToString();

            if (fieldsStr.EndsWith(','))
            {
                fieldsStr = fieldsStr[..^1];
            }

            string keysStr = keys.ToString();

            query.Append(string.Format("UPDATE {0} SET {1} WHERE 1=1 {2}", dbInfo.ObjectName, fieldsStr, keysStr));

            return query.ToString();
        }

        protected static string FormatDeleteQuery<T>(T entity, out IDictionary<string, object> dynamicOptions)
        {
            StringBuilder query = new();
            StringBuilder keys = new();

            DatabaseInformations dbInfo = GetDocumentDatabaseInformations();

            dynamicOptions = new ExpandoObject();

            foreach (KeyValuePair<string, ColumnDetail> mapInfo in dbInfo.EntityDbMappings)
            {
                object val = ReflectionHelper.GetPropertyValue(entity, mapInfo.Key);
                if (val != null)
                {
                    if (mapInfo.Value.IsKey)
                    {
                        keys.Append(string.Format(" AND {0}=@{1}", mapInfo.Value.Name, mapInfo.Key));
                    }

                    dynamicOptions[mapInfo.Key] = val;
                }
            }

            string keysStr = keys.ToString();

            query.Append(string.Format("DELETE {0} WHERE 1=1 {1}", dbInfo.ObjectName, keysStr));


            return query.ToString();
        }

        #endregion

        #region Private

        private static string PropertyToDb(DatabaseInformations dbInfo, string propertyName)
        {
            KeyValuePair<string, ColumnDetail> colInfo = dbInfo.EntityDbMappings.FirstOrDefault(col => col.Key == propertyName);

            if (colInfo.Key != null)
            {
                return colInfo.Value.Name; // nome colonna da nome proprietà
            }

            colInfo = dbInfo.EntityDbMappings.FirstOrDefault(col => col.Value.Name == propertyName);

            if (colInfo.Key != null)
            {
                return propertyName; // già nome colonna
            }

            return string.Empty;
        }

        private static string ValueToDb(DatabaseInformations dbInfo, string propertyName, object propertyValue)
        {
            KeyValuePair<string, ColumnDetail> colInfo = dbInfo.EntityDbMappings.FirstOrDefault(col => col.Key == propertyName);

            if (colInfo.Key == null)
            {
                colInfo = dbInfo.EntityDbMappings.FirstOrDefault(col => col.Value.Name == propertyName);
            }

            if (colInfo.Key != null)
            {
                // verifico il tipo dato e applico formattazione
                switch (colInfo.Value.TypeName)
                {
                    case "varchar":
                    case "varchar2":
                    case "nvarchar":
                        return string.Format("'{0}'", propertyValue);

                    case "datetime":
                        return string.Format("'{0}'", DateTime.Parse(propertyValue.ToString()).ToString("yyyy-MM-dd hh:mm:ss"));

                }

            }

            return propertyValue.ToString();
        }

        #endregion

    }
}
