using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Infrastructure.Services;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;

namespace MIT.Fwk.WebApi.Controllers
{
    [Route("benchmark")]
    public class BenchmarkController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonApiManualService;
        protected ILogger _logger;
        public BenchmarkController(
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IJsonApiManualService jsonApiManualService,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AccountController>();
            _jsonApiManualService = jsonApiManualService;
        }

        [HttpGet]
        [Route("takeTwoHundred/{entityName}")]
        [AllowAnonymous]
        public IActionResult GetAudits(string entityName)
        {
            try
            {
                Tuple<Type, Type> entityTypes = _jsonApiManualService.GetEntityTypeAndIdType(entityName);

                if (entityTypes == null)
                {
                    return BadRequest();
                }

                //IEnumerable
                Stopwatch iEnumerableStopwatch = new();
                iEnumerableStopwatch.Start();
                System.Reflection.MethodInfo method = typeof(JsonApiManualService).GetMethod("GetAll");
                System.Reflection.MethodInfo genericMethod = method.MakeGenericMethod(entityTypes.Item1, entityTypes.Item2);
                object iEnumerable = genericMethod.Invoke(_jsonApiManualService, Array.Empty<object>());
                iEnumerable = typeof(EntityFrameworkQueryableExtensions).GetMethod("AsNoTracking")
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { iEnumerable });
                iEnumerable = typeof(Enumerable).GetMethod("AsEnumerable")
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { iEnumerable });
                iEnumerable = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Take" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { iEnumerable, 200 });
                object resultList = typeof(Enumerable).GetMethod("ToList")
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { iEnumerable });
                iEnumerableStopwatch.Stop();

                //IQueryable
                Stopwatch iQueryableStopwatch = new();
                iQueryableStopwatch.Start();
                method = typeof(JsonApiManualService).GetMethod("GetAllQueryable");
                genericMethod = method.MakeGenericMethod(entityTypes.Item1, entityTypes.Item2);
                object auditsQuerable = genericMethod.Invoke(_jsonApiManualService, Array.Empty<object>());
                //auditsQuerable = typeof(EntityFrameworkQueryableExtensions).GetMethod("AsNoTracking")
                //    .MakeGenericMethod(entityTypes.Item1)
                //    .Invoke(null, new object[] { auditsQuerable });
                auditsQuerable = typeof(Queryable).GetMethods()
                    .First(m => m.Name == "Take" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { auditsQuerable, 200 });
                resultList = typeof(Enumerable).GetMethod("ToList")
                    .MakeGenericMethod(entityTypes.Item1)
                    .Invoke(null, new object[] { auditsQuerable });
                iQueryableStopwatch.Stop();

                return Ok(JsonConvert.SerializeObject(new { iEnumerablePerformance = $"{iEnumerableStopwatch.ElapsedMilliseconds} ms", iQueryablePerformance = $"{iQueryableStopwatch.ElapsedMilliseconds} ms" }));
            }
            catch
            {
                return Forbid();
            }
        }
    }
}
