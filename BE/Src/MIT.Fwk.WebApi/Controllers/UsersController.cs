using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    public class UsersController : JsonApiController<User, String>
    {
        private readonly IJsonApiManualService _jsonApiManualService;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly IDocumentService _docService;
        private readonly JsonApiDbContext _context;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly IncludeParser _includeParser;
        private readonly IResourceGraph _resourceGraph;

        [ActivatorUtilitiesConstructor]
        public UsersController(IJsonApiManualService jsonApiManualService
            , IJsonApiOptions options, IResourceGraph resourceGraph
            , ILoggerFactory loggerFactory
            , IResourceService<User, String> resourceService
            , JsonApiDbContext context
            , IEvaluatedIncludeCache evaluatedIncludeCache
            , IncludeParser includeParser) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _jsonApiManualService = jsonApiManualService;
            _context = context;
            _evaluatedIncludeCache = evaluatedIncludeCache;
            _includeParser = includeParser;
            _resourceGraph = resourceGraph;
        }


        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(String id, CancellationToken cancellationToken)
        {
            try
            {
                User ut = await _jsonApiManualService.DeleteUser(id);

                return ut == null ? StatusCode(404) : StatusCode(204);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }




        [HttpGet]
        [Route("statistics")]

        public Task<IActionResult> Statistics()
        {
            ResourceType rsUser = _resourceGraph.FindResourceType("users");
            _evaluatedIncludeCache.Set(_includeParser.Parse("userTenants,userTenants.tenant,userProfile", rsUser));
            IQueryable<User> query = _context.Users.Include(u => u.UserProfile).Include(u => u.UserTenants).ThenInclude(u => u.Tenant).Where(u => u.UserTenants.Any(ut => ut.Tenant.Id == 999));



            dynamic users = query.ToList();
            return Task.FromResult<IActionResult>(Ok(users));
        }
    }
}

