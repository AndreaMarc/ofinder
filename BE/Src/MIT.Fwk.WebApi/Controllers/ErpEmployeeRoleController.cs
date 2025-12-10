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
    public class ErpEmployeeRoleController : JsonApiController<ErpEmployeeRole, string>
    {
        private readonly IResourceService<ErpEmployeeRole, string> _resourceService;
        private readonly IResourceService<ErpEmployee, string> _employeeService;
        private readonly IResourceService<ErpRole, string> _roleService;
        private readonly IResourceService<Tenant, int> _tenantService;
        private readonly IJsonApiManualService _jsonService;
        private readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public ErpEmployeeRoleController(
            IJsonApiManualService jsonManualService,
            IDocumentService docService,
            IJsonApiOptions options,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<ErpEmployeeRole, string> resourceService,
            IResourceService<ErpEmployee, string> employeeService,
            IResourceService<ErpRole, string> roleService,
            IResourceService<Tenant, int> tenantService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _employeeService = employeeService;
            _roleService = roleService;
            _tenantService = tenantService;
        }
    }
}

