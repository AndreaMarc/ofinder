using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Examples.Entities;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;

namespace MIT.Fwk.Examples.Controllers
{
    [method: ActivatorUtilitiesConstructor]
    public class ExampleCategoriesController(IJsonApiManualService jsonApiManualService
            , IJsonApiOptions options, IResourceGraph resourceGraph
            , ILoggerFactory loggerFactory
            , IResourceService<ExampleCategory, int> resourceService
            , JsonApiDbContext context
            , IEvaluatedIncludeCache evaluatedIncludeCache
            , IncludeParser includeParser) : JsonApiController<ExampleCategory, int>(options, resourceGraph, loggerFactory, resourceService)
    {
        private readonly IJsonApiManualService _jsonApiManualService = jsonApiManualService;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly IDocumentService _docService;
        private readonly JsonApiDbContext _context = context;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache = evaluatedIncludeCache;
        private readonly IncludeParser _includeParser = includeParser;
        private readonly IResourceGraph _resourceGraph = resourceGraph;
    }
}

