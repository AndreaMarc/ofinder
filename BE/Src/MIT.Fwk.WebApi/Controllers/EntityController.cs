
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using MIT.Fwk.Core.Attributes;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Route("entity")]
    [Authorize]
    public class EntityController : ControllerBase
    {

        private readonly JsonApiDbContext _context;

        public EntityController(
        JsonApiDbContext context,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _context = context; ;
        }



        [HttpGet]
        [Route("getAll")]
        public IActionResult GetAllEntities()
        {
            List<object> customDbContexts = ReflectionHelper.ResolveAll<IJsonApiDbContext>();

            List<IEntityType> ets = [];

            if (customDbContexts != null && customDbContexts.Count > 0)
            {
                foreach (object customDbContext in customDbContexts)
                {
                    System.Reflection.PropertyInfo modelProperty = customDbContext.GetType().GetProperty("Model");
                    object modelValue = modelProperty.GetValue(customDbContext);
                    System.Reflection.MethodInfo getEntityTypesMethod = modelValue.GetType().GetInterface("IModel").GetMethod("GetEntityTypes");
                    ets.AddRange(((IEnumerable<IEntityType>)getEntityTypesMethod.Invoke(modelValue, null)).ToList());
                }
            }

            ets.AddRange(_context.Model.GetEntityTypes().ToList());

            List<string> result = [];
            foreach (IEntityType entityType in ets)
            {
                if (!result.Contains(entityType.DisplayName()))
                {
                    result.Add(entityType.DisplayName());
                }
            }

            result = result.OrderBy(x => x).ToList();

            return Ok(new { success = true, data = result });
        }







    }
}

