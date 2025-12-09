using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace MIT.Fwk.Scheduler.Services
{
    # nullable enable
    /// <summary>
    /// Implementation of ILogTransformer for converting LogToMongo to BsonDocument
    /// </summary>
    public class LogTransformer : ILogTransformer
    {
        public BsonDocument Transform(LogToMongo logToMongo)
        {
            string parameters = ExtractModelParameters(logToMongo.Model);
            MinimalUserInfo userLog = CreateUserInfo(logToMongo.CurrentUser);

            BsonDocument document = new()
            {
                { "User", userLog.ToBsonDocument() },
                { "Date", DateTime.UtcNow },
                { "Data", logToMongo.RoutePath },
                { "Parameters", parameters },
                { "RequestMethod", logToMongo.RequestMethod },
                { "Headers", BsonSerializer.Deserialize<BsonArray>(logToMongo.Headers) },
                { "Payload", ShouldExcludePayload(logToMongo.RoutePath) ? "" : BsonSerializer.Deserialize<BsonArray>(logToMongo.PayLoad) },
                { "LogType", logToMongo.LogType }
            };

            return document;
        }

        private string ExtractModelParameters(object? model)
        {
            if (model == null)
            {
                return string.Empty;
            }

            PropertyInfo[] props = model.GetType().GetProperties();
            return string.Join(",\n", props.Select(prop => $"{prop.Name} -> {prop.GetValue(model)}"));
        }

        private MinimalUserInfo CreateUserInfo(object? currentUser)
        {
            if (currentUser == null)
            {
                return new MinimalUserInfo
                {
                    Guid = "notLoggedUser",
                    Email = "notLoggedUser",
                    LastAccess = DateTime.MinValue,
                    LastIp = ""
                };
            }

            // Use reflection to extract user properties dynamically
            Type userType = currentUser.GetType();
            PropertyInfo? idProp = userType.GetProperty("Id");
            PropertyInfo? emailProp = userType.GetProperty("Email");
            PropertyInfo? lastAccessProp = userType.GetProperty("LastAccess");
            PropertyInfo? lastIpProp = userType.GetProperty("LastIp");

            return new MinimalUserInfo
            {
                Guid = idProp?.GetValue(currentUser)?.ToString() ?? "unknown",
                Email = emailProp?.GetValue(currentUser)?.ToString() ?? "unknown",
                LastAccess = (lastAccessProp?.GetValue(currentUser) as DateTime?) ?? DateTime.MinValue,
                LastIp = lastIpProp?.GetValue(currentUser)?.ToString() ?? ""
            };
        }

        private bool ShouldExcludePayload(string routePath)
        {
            // Exclude file upload payloads (too large)
            return routePath == "/api/v6/fileUpload";
        }
    }
}
