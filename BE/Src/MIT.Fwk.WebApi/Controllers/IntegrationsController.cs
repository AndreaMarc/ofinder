using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class IntegrationsController : JsonApiController<Integration, String>
    {
        private readonly IResourceService<Integration, String> _resourceService;
        private readonly IJsonApiManualService _jsonApiManualService;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public IntegrationsController(IDocumentService docService
            , IJsonApiManualService jsonApiManualService
            , UserManager<MITApplicationUser> userManager
            , IJsonApiOptions options, IResourceGraph resourceGraph
            , ILoggerFactory loggerFactory
            , IResourceService<Integration, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _userManager = userManager;
            _jsonApiManualService = jsonApiManualService;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] Integration resource, CancellationToken cancellationToken)
        {
            resource.EncryptionKey = await _jsonApiManualService.CalculateMD5Hash(resource.Name + resource.Code);

            return await base.PostAsync(resource, cancellationToken);
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(String id, [FromBody] Integration resource, CancellationToken cancellationToken)
        {
            resource.EncryptionKey = await _jsonApiManualService.CalculateMD5Hash(resource.Name + resource.Code);

            return await base.PatchAsync(id, resource, cancellationToken);
        }
    }
}

