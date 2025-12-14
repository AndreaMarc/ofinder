using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using String = System.String;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    [Route("roleClaims")]
    public class RoleClaimsCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;
        protected readonly UserManager<MITApplicationUser> _userManager;

        public RoleClaimsCustomController(
        IJsonApiManualService jsonService,
        IResourceService<Translation, int> translationService,
        IResourceService<Setup, int> setupService,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
            _userManager = userManager;
        }
        public class CrudModel
        {
            public string roleId { get; set; }
            public List<Dictionary<String, EntityModel>> claims { get; set; }
        }

        public class EntityModel
        {
            public bool read { get; set; }
            public bool create { get; set; }
            public bool update { get; set; }
            public bool delete { get; set; }

        }

        [HttpPost]
        [Route("crud")]
        public async Task<IActionResult> Crud([FromBody] CrudModel model, CancellationToken cancellationToken)
        {

            List<RoleClaim> listClaimsRoles = [];

            foreach (Dictionary<string, EntityModel> e in model.claims)
            {


                if (e.Values.First().create)
                {
                    RoleClaim entity = new()
                    {
                        RoleId = model.roleId,
                        ClaimType = "crud",
                        ClaimValue = e.Keys.First() + ".create"
                    };
                    listClaimsRoles.Add(entity);

                }
                if (e.Values.First().update)
                {
                    RoleClaim entity = new()
                    {
                        RoleId = model.roleId,
                        ClaimType = "crud",
                        ClaimValue = e.Keys.First() + ".update"
                    };
                    listClaimsRoles.Add(entity);

                }
                if (e.Values.First().read)
                {
                    RoleClaim entity = new()
                    {
                        RoleId = model.roleId,
                        ClaimType = "crud",
                        ClaimValue = e.Keys.First() + ".read"
                    };
                    listClaimsRoles.Add(entity);

                }
                if (e.Values.First().delete)
                {
                    RoleClaim entity = new()
                    {
                        RoleId = model.roleId,
                        ClaimType = "crud",
                        ClaimValue = e.Keys.First() + ".delete"
                    };
                    listClaimsRoles.Add(entity);

                }

            }

            return Ok(new { success = true, data = await _jsonService.SetRoleClaims(listClaimsRoles, model.roleId, true) });

        }

        public class RoutesRoleModel
        {
            public string roleId { get; set; }
            public string claimsType { get; set; }
            public List<String> claims { get; set; }
        }

        [HttpPost]
        [Route("route")]
        public async Task<IActionResult> RouteCrud([FromBody] RoutesRoleModel model, CancellationToken cancellationToken)
        {


            List<RoleClaim> listClaimsRoles = [];
            foreach (string claim in model.claims)
            {

                RoleClaim entity = new() { RoleId = model.roleId, ClaimType = model.claimsType, ClaimValue = claim };
                listClaimsRoles.Add(entity);

            }


            return Ok(new { success = true, data = await _jsonService.UpdateRoutesRole(listClaimsRoles) });

        }

        [HttpGet]
        [Route("getAssignableClaims")]
        public async Task<IActionResult> getAssignableClaims()
        {
            // Get current user from request
            string username = Request.HttpContext.User.Claims.ToList().Find(x => x.Type == "username").Value;
            MITApplicationUser currentUser = await _userManager.FindByEmailAsync(username);
            if (currentUser == null)
            {
                currentUser = await _userManager.FindByNameAsync(username);
            }

            List<string> claimsPool = (await _jsonService.GetClaimsByUsername(currentUser.UserName, currentUser.TenantId.ToString())).Split(",").ToList();

            List<Role> roles = _jsonService.GetAllRolesByTenant(currentUser.TenantId);

            if (claimsPool.Contains("isSuperAdmin"))
            {
                return Ok(new { success = true, data = "{ \"roles\": \"" + String.Join(",", roles.Select(x => x.Id)) + "\" }" });
            }
            else
            {
                List<string> possibleRoles = [];

                List<RoleClaim> allClaims = _jsonService.GetAllRoleClaimsInRoles(roles);

                foreach (Role role in roles)
                {
                    if (role == null) { continue; }

                    List<string> roleClaims = allClaims.FindAll(x => x.RoleId == role.Id).Select(x => x.ClaimValue).ToList();
                    roleClaims.RemoveAll(x => x.StartsWith("is"));

                    if (roleClaims.Count == 0 || roleClaims.All(x => claimsPool.Contains(x)))
                    {
                        possibleRoles.Add(role.Id);
                    }
                }

                if (possibleRoles.Count == 0)
                {
                    possibleRoles.Add(roles.First(x => x.Name == "User").Id);
                }

                return Ok(new { success = true, data = "{ \"roles\": \"" + String.Join(",", possibleRoles) + "\" }" });
            }
        }



    }
}

