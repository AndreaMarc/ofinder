using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;

namespace MIT.Fwk.WebApi.Controllers
{
    public class ErpExternalWorkerDetailsController : JsonApiController<ErpExternalWorkerDetails, string>
    {
        private readonly IResourceService<ErpExternalWorkerDetails, string> _resourceService;
        private readonly IResourceService<ErpEmployee, string> _employeeService;
        private readonly IResourceService<Tenant, int> _tenantService;
        private readonly IJsonApiManualService _jsonService;
        private readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public ErpExternalWorkerDetailsController(
            IJsonApiManualService jsonManualService,
            IDocumentService docService,
            IJsonApiOptions options,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<ErpExternalWorkerDetails, string> resourceService,
            IResourceService<ErpEmployee, string> employeeService,
            IResourceService<Tenant, int> tenantService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _employeeService = employeeService;
            _tenantService = tenantService;
        }
    }
}