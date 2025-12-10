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
using System.Linq;

using System.Threading;
using System.Threading.Tasks;
using MIT.Fwk.Core.Attributes;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class TenantController : JsonApiController<Tenant, int>
    {
        private readonly IResourceService<Tenant, int> _resourceService;
        private readonly IResourceService<User, String> _userService;
        private readonly IResourceService<Category, int> _categoryService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public TenantController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Tenant, int> resourceService, IResourceService<Category, int> categoryService, IResourceService<User, String> userService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _userService = userService;
            _categoryService = categoryService;
        }



        [HttpPost]
        public override async Task<IActionResult> PostAsync(Tenant tenant, CancellationToken cancellationToken)
        {
            Tenant newTenant = await _resourceService.CreateAsync(tenant, cancellationToken);
            //copiamo dal tenant padre se esiste, senno prendiamo il tenant 1 che e'quello base
            Tenant parentTenant = await _resourceService.GetAsync(tenant.ParentTenant < 1 ? 1 : tenant.ParentTenant, cancellationToken);

            if (parentTenant.Id != 1)
            {
                List<Role> rolesFromParent = await _jsonService.RolesFromParent(parentTenant);

                List<RoleClaim> allClaims = _jsonService.GetAllRoleClaimsInRoles(rolesFromParent);

                //copio tutti i ruoli di sistema e relativi claims
                foreach (Role role in rolesFromParent)
                {
                    if (role.CopyInNewTenants == false)
                    {
                        continue;
                    }

                    Role newRole = new() { Level = role.Level, Name = role.Name, CopyInNewTenants = role.CopyInNewTenants, Needful = role.Needful, TenantId = newTenant.Id, NormalizedName = role.NormalizedName, Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() };

                    await _jsonService.CreateAsync<Role, string>(newRole);

                    List<RoleClaim> roleClaims = allClaims.FindAll(x => x.RoleId == role.Id);

                    foreach (RoleClaim roleClaim in roleClaims)
                    {
                        RoleClaim newRoleClaim = new() { ClaimType = roleClaim.ClaimType, ClaimValue = roleClaim.ClaimValue, RoleId = newRole.Id };
                        await _jsonService.CreateAsync<RoleClaim, int>(newRoleClaim);
                    }
                }
            }

            //copio tutte le categorie del padre
            IEnumerable<Category> categoriesToCopy = (await _jsonService.GetAllCategoriesByTenantId(parentTenant.Id)).Where(x => x.CopyInNewTenants && x.ParentCategory == 0);

            await _jsonService.CopyCategoryTreeRecursive(categoriesToCopy, 0, newTenant.Id, parentTenant.Id);

            List<MediaCategory> mediaCategoriesToCopy = (await _jsonService.GetAllMediaCategoriesByTenantId(parentTenant.Id)).ToList().FindAll(x => x.copyInNewTenant && x.ParentMediaCategory == "");

            await _jsonService.CopyMediaCategoryTreeRecursive(mediaCategoriesToCopy, "", newTenant.Id, parentTenant.Id);

            return Ok(newTenant);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            Tenant tenant = _jsonService.FirstOrDefault<Tenant, int>(x => x.Id == id);
            if (tenant == null || !tenant.isErasable)
            {
                return NotFound();
            }

            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;

            if (!queryString.ContainsKey("alternativeTenant"))
            {
                await _jsonService.DeleteTenantReferiments(id);
            }
            else
            {
                await _jsonService.DeleteTenantReferiments(id, int.Parse(queryString["alternativeTenant"]));
            }

            return await base.DeleteAsync(id, cancellationToken);
        }

    }
}

