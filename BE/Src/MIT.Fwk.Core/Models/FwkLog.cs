using MIT.Fwk.Core.Domain.Interfaces;
using MongoDB.Bson;
using System;

namespace MIT.Fwk.Core.Models
{
    public class FwkLog : IFwkLog
    {
        public long Id { get; set; }

        public string LogType { get; set; }
        public string RequestMethod { get; set; }
        public string Parameters { get; set; }

        public MinimalUserInfo User { get; set; }

        public DateTime Date { get; set; }

        public string Data { get; set; }
        public BsonArray Headers { get; set; }
        public BsonArray Payload { get; set; }

    }

    public class FwkLogResult
    {
        public string Id { get; set; }

        public string LogType { get; set; }
        public string RequestMethod { get; set; }
        public string Parameters { get; set; }

        public MinimalUserInfo User { get; set; }

        public DateTime Date { get; set; }

        public string Data { get; set; }
        public object Headers { get; set; }
        public object Payload { get; set; }

    }

    public class FwkLogDTO
    {
        public string Method { get; set; }
        public DateTime LogDateStart { get; set; }
        public DateTime LogDateEnd { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string PartialRoute { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public string TenantId { get; set; }

    }

    public class MinimalUserInfo
    {
        public string Guid { get; set; }
        public string Email { get; set; }
        public DateTime LastAccess { get; set; }
        public string LastIp { get; set; }
    }

}
