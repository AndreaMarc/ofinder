using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class SetupController : JsonApiController<Setup, int>
    {
        private readonly IResourceService<Setup, int> _resourceService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;
        readonly IdentityOptions _identityOptions;

        [ActivatorUtilitiesConstructor]
        public SetupController(IOptions<IdentityOptions> identityOptionsAccessor, IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Setup, int> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _identityOptions = identityOptionsAccessor.Value;//httpContextAccessor.HttpContext.RequestServices.GetService<IdentityOptions>();
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(int id, [FromBody] Setup resource, CancellationToken cancellationToken)
        {
            _identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(resource.blockingPeriodDuration);
            _identityOptions.Lockout.MaxFailedAccessAttempts = resource.failedLoginAttempts;

            return await base.PatchAsync(id, resource, cancellationToken);
        }

    }
}

