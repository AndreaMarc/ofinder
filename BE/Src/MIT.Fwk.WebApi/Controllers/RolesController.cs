using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class RolesController : JsonApiController<Role, String>
    {
        private readonly IResourceService<Role, String> _resourceService;
        private readonly IResourceService<RoleClaim, int> _claimService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public RolesController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Role, String> resourceService, IResourceService<RoleClaim, int> claimService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _claimService = claimService;
        }

        [HttpGet]
        [HttpHead]
        public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            IActionResult actionResult = (await base.GetAsync(cancellationToken));

            List<Role> roles = [];

            if (actionResult is OkObjectResult ok)
            {
                try
                {
                    Role[] value = (Role[])ok.Value;
                    roles = value.ToList();
                }
                catch
                {
                    var rolesReadOnly = (ReadOnlyCollection<Role>)ok.Value;
                    roles = [.. rolesReadOnly];
                }
            }

            if (roles.FirstOrDefault(x => x.TenantId == 1) == null)
            {
                roles.AddRange(_jsonService.GetAllRolesByTenant(1));
            }

            return Ok(roles);
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync(Role role, CancellationToken cancellationToken)
        {
            if (role.TenantId != 1 && _jsonService.GetAllRolesByTenant(1).Select(x => x.Name).Contains(role.Name))
            {
                return Forbid();
            }

            if (role.TenantId == 1)
            {
                role.Needful = true;
            }

            await _resourceService.CreateAsync(role, cancellationToken);

            RoleClaim newRoleClaim = new() { ClaimType = "custom", ClaimValue = "is" + role.Name.Pascalize(), RoleId = role.Id };
            await _jsonService.CreateAsync<RoleClaim, int>(newRoleClaim);

            return StatusCode(201);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(String id, CancellationToken cancellationToken)
        {
            List<RoleClaim> claims = await _jsonService.GetAllClaimsByRoleId(id);

            foreach (RoleClaim item in claims)
            {
                await _claimService.DeleteAsync(item.Id, cancellationToken);
            }

            return await base.DeleteAsync(id, cancellationToken);
        }
    }
}

