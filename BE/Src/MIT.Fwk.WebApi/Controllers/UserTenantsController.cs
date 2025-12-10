using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class UserTenantsController : JsonApiController<UserTenant, String>
    {
        private readonly IResourceService<UserTenant, String> _resourceService;
        private readonly IResourceService<UserRole, String> _userRoleService;
        private readonly IResourceService<Role, String> _roleService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public UserTenantsController(IDocumentService docService, IJsonApiManualService jsonService, IResourceService<UserRole, String> userRoleService, IResourceService<Role, String> roleService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<UserTenant, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _jsonService = jsonService;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] UserTenant associazioneUtenteTenantDiDestinazione, CancellationToken cancellationToken)
        {
            Request.Query.TryGetValue("roleName", out StringValues ruoloDesiderato);

            if (!String.IsNullOrEmpty(ruoloDesiderato))
            {
                IEnumerable<Role> ruoliEsistentiConIlNomePassato = _jsonService.GetAll<Role, String>().Where(x => x.Name == ruoloDesiderato);

                if (ruoliEsistentiConIlNomePassato == null || ruoliEsistentiConIlNomePassato.Count() == 0
                    || ruoliEsistentiConIlNomePassato.Where(ruoloInEsame => ruoloInEsame.TenantId == 1 || ruoloInEsame.TenantId == associazioneUtenteTenantDiDestinazione.TenantId).Count() == 0)
                {
                    return StatusCode(400, "Role not valid.");
                }

                await _jsonService.RegisterUserRole(associazioneUtenteTenantDiDestinazione.UserId, associazioneUtenteTenantDiDestinazione.TenantId, ruoloDesiderato);
            }
            else if (await _jsonService.CheckIsSuperadmin(associazioneUtenteTenantDiDestinazione.UserId))
            {
                await _jsonService.RegisterUserRole(associazioneUtenteTenantDiDestinazione.UserId, associazioneUtenteTenantDiDestinazione.TenantId, "SuperAdmin");
            }
            else
            {
                await _jsonService.RegisterUserRole(associazioneUtenteTenantDiDestinazione.UserId, associazioneUtenteTenantDiDestinazione.TenantId, "User");
            }

            return await base.PostAsync(associazioneUtenteTenantDiDestinazione, cancellationToken);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(String id, CancellationToken cancellationToken)
        {
            UserTenant userTenant = await _resourceService.GetAsync(id, cancellationToken);
            UserTenant otherAssociatedTenant = (await _resourceService.GetAsync(cancellationToken)).FirstOrDefault(x => x.UserId == userTenant.UserId && x.Id != id);
            User user = await _jsonService.GetUserById(userTenant.UserId);

            //se un utente é nel master tenant qui non ci entro
            if (otherAssociatedTenant == null)
            {
                Tenant recoveryTenant = _jsonService.FirstOrDefault<Tenant, int>(x => x.isRecovery);
                if (recoveryTenant.Id == userTenant.TenantId)
                {
                    return StatusCode(406);
                }
                else
                {
                    await _jsonService.RegisterUserTenant(userTenant.UserId, recoveryTenant.Id, "127.0.0.1", "ownerCreated");
                    await _jsonService.RegisterUserRole(userTenant.UserId, recoveryTenant.Id, "User");
                    user.TenantId = recoveryTenant.Id;
                    await _jsonService.UpdateAsync<User, string>(user);
                }
            }

            StringValues bearer;
            HttpContext.Request.Headers.TryGetValue("Authorization", out bearer);
            JwtSecurityToken jwt = new(bearer.ToString().Substring(7));
            string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
            User userSession = await _jsonService.GetUserByEmail(userEmail);
            bool isSuperadmin = await _jsonService.CheckIsSuperadmin(userSession.Id);



            List<UserRole> userRole = await _jsonService.GetAllUserRolesByEmail(user.Email);
            Setup webSetup = await _jsonService.FirstOrDefaultAsync<Setup, int>(x => x.environment == "web");
            List<string> rolesDeletable = webSetup.rolesForEditUsers.Split(',').ToList();
            List<Role> rolesCustomCurrentTenant = _jsonService.RolesFromTenant(userTenant.TenantId);

            List<Role> idRolesDeletable = await _jsonService.GetAllRolesByNames(rolesDeletable);
            idRolesDeletable.AddRange(rolesCustomCurrentTenant);
            IEnumerable<string> userRoleToDeleteIds = userRole.FindAll(x => x.TenantId == userTenant.TenantId && idRolesDeletable.Select(x => x.Id).Contains(x.RoleId)).Select(x => x.Id);
            List<UserRole> userRoles = (await _userRoleService.GetAsync(cancellationToken)).ToList().FindAll(x => x.UserId == userTenant.UserId && x.TenantId == userTenant.TenantId);
            bool canDeleteAssociation = true;
            foreach (UserRole ur in userRoles)
            {

                if (userRoleToDeleteIds.Contains(ur.Id) || isSuperadmin)
                {

                    await _userRoleService.DeleteAsync(ur.Id, cancellationToken);
                }
                else { canDeleteAssociation = false; }
            }

            //gli metto come tenant di default uno diverso da quello da cui lo sto togliendo
            if (user.TenantId == userTenant.TenantId)
            {
                user.TenantId = otherAssociatedTenant.TenantId;
                await _jsonService.UpdateAsync<User, string>(user);
            }


            return canDeleteAssociation ? await base.DeleteAsync(id, cancellationToken) : StatusCode(401);
        }

    }
}

