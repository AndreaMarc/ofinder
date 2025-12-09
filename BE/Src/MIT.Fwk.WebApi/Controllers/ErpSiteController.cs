using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class ErpSiteController : JsonApiController<ErpSite, string>
    {
        private readonly IResourceService<ErpSite, string> _resourceService;
        private readonly IResourceService<Tenant, int> _tenantService;
        private readonly IJsonApiManualService _jsonService;
        private readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public ErpSiteController(
            IJsonApiManualService jsonManualService,
            IDocumentService docService,
            IJsonApiOptions options,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<ErpSite, string> resourceService,
            IResourceService<Tenant, int> tenantService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _tenantService = tenantService;
        }
    }
}

