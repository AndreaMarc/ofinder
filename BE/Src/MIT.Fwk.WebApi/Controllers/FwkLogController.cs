using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    //[EnableCors("AllowSpecificOrigin")]
    [SkipClaimsValidation]
    [SkipRequestLogging]
    public class FwkLogController : ControllerBase
    {
        protected readonly IFwkLogService _fwkLogService;
        protected ILogger _logger;
        private readonly IConnectionStringProvider _connStringProvider;
        private readonly string _logCollection;

        public FwkLogController(
            IFwkLogService FwkLogService,
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IConnectionStringProvider connStringProvider,
            IOptions<LogOptions> logOptions,
            ILoggerFactory loggerFactory)
        {
            _fwkLogService = FwkLogService;
            _logger = loggerFactory.CreateLogger<FwkLogController>();
            _connStringProvider = connStringProvider;
            _logCollection = logOptions.Value.LogCollection;
        }

        public class MyJsonObject
        {
            public string Key { get; set; }
            public List<string> Value { get; set; }
        }

        [HttpPost]
        [Authorize(Policy = "isAudit")]
        [Route("log")]
        public Task<IActionResult> ReadLogAsync([FromBody] FwkLogDTO log)
        {
            try
            {
                string connectionString = _connStringProvider.GetConnectionString("NoSQLConnection");
                MongoClient dbClient = new(connectionString.Substring(0, connectionString.LastIndexOf("/")));
                IMongoDatabase db = dbClient.GetDatabase(connectionString.Substring(connectionString.LastIndexOf("/") + 1));
                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(_logCollection);

                FilterDefinition<BsonDocument> filter = new BsonDocument();

                int pages = 1;

                if (log != null)
                {
                    FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
                    filter = builder.Eq("LogType", "Middleware");

                    if (!string.IsNullOrEmpty(log.Method))
                    {
                        filter &= builder.Eq("RequestMethod", log.Method);
                    }

                    if (!string.IsNullOrEmpty(log.UserEmail))
                    {
                        filter &= builder.Eq("User.Email", log.UserEmail);
                    }

                    if (!string.IsNullOrEmpty(log.PartialRoute))
                    {
                        filter &= builder.Regex("Data", new BsonRegularExpression(log.PartialRoute));
                    }

                    if (!string.IsNullOrEmpty(log.UserId))
                    {
                        filter &= builder.Eq("User.Guid", log.UserId);
                    }

                    if (log.LogDateStart != DateTime.MinValue)
                    {
                        filter &= builder.Gte("Date", log.LogDateStart);
                    }

                    if (log.LogDateEnd != DateTime.MinValue)
                    {
                        filter &= builder.Lte("Date", log.LogDateEnd);
                    }

                    if (log.TenantId != null)
                    {
                        filter &= builder.Eq("Headers", new BsonDocument { { "Key", "tenantId" }, { "Value", new BsonArray { log.TenantId } } });
                    }
                }

                if (log.PageNumber == 0)
                {
                    log.PageNumber = 1;
                }

                if (log.PageSize == 0)
                {
                    log.PageSize = 20;
                }

                long totalNumber = collection.CountDocuments(filter);
                pages = (int)Math.Ceiling((double)totalNumber / log.PageSize);

                List<BsonDocument> filteredResults = collection.Find(filter).Skip((log.PageNumber - 1) * log.PageSize).Limit(log.PageSize).ToList();

                List<FwkLogResult> result = [];

                result = filteredResults.Select(fwkLog => new FwkLogResult
                {
                    Data = fwkLog["Data"].ToString(),
                    Date = fwkLog["Date"].ToUniversalTime(),
                    Id = fwkLog["_id"].ToString(),
                    LogType = fwkLog["LogType"].ToString(),
                    Parameters = fwkLog["Parameters"].ToString(),
                    RequestMethod = fwkLog["RequestMethod"].ToString(),
                    User = new MinimalUserInfo
                    {
                        Email = fwkLog["User"]["Email"].ToString(),
                        Guid = fwkLog["User"]["Guid"].ToString(),
                        LastAccess = fwkLog["User"]["LastAccess"].ToUniversalTime(),
                        LastIp = fwkLog["User"]["LastIp"].ToString()
                    },
                    Headers = Newtonsoft.Json.JsonConvert.DeserializeObject(fwkLog["Headers"].ToString()),
                    Payload = Newtonsoft.Json.JsonConvert.DeserializeObject(fwkLog["Payload"].ToString())
                }).ToList();

                return Task.FromResult<IActionResult>(Ok(new { Pages = pages, Logs = result }));

            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(BadRequest(ex));
            }
        }
    }
}