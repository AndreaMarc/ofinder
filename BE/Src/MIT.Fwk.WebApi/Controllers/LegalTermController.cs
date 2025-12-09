using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using MIT.Fwk.Core.Attributes;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class LegalTermController : JsonApiController<LegalTerm, String>
    {
        private readonly IResourceService<LegalTerm, String> _resourceService;
        private readonly IResourceService<RoleClaim, int> _claimService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public LegalTermController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<LegalTerm, String> resourceService, IResourceService<RoleClaim, int> claimService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _claimService = claimService;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync(LegalTerm legalTerms, CancellationToken cancellationToken)
        {
            if (legalTerms.Active)
            {
                legalTerms.Active = false;
            }

            await _resourceService.CreateAsync(legalTerms, cancellationToken);

            return StatusCode(201);
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(String id, [FromBody] LegalTerm resource, CancellationToken cancellationToken)
        {
            LegalTerm existing = await _resourceService.GetAsync(id, cancellationToken);

            if (existing.Active != resource.Active)
            {
                resource.Active = existing.Active;
            }

            return await base.PatchAsync(id, resource, cancellationToken);
        }
    }
}

