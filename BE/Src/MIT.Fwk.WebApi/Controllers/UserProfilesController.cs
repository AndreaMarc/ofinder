using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class UserProfilesController : JsonApiController<UserProfile, String>
    {
        private readonly IResourceService<UserProfile, String> _resourceService;
        private readonly IResourceService<User, String> _userService;
        private readonly IResourceService<UserTenant, String> _userTenantService;
        private readonly IResourceService<Setup, int> _setupService;
        private readonly IJsonApiManualService _jsonApiManualService;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly IDocumentService _docService;
        private readonly IConfiguration _configuration;

        [ActivatorUtilitiesConstructor]
        public UserProfilesController(IDocumentService docService
            , IJsonApiManualService jsonApiManualService
            , IResourceService<User, String> userService
            , IResourceService<UserTenant, String> userTenantService
            , IResourceService<Setup, int> setupService
            , UserManager<MITApplicationUser> userManager
            , IConfiguration configuration
            , IJsonApiOptions options, IResourceGraph resourceGraph
            , ILoggerFactory loggerFactory
            , IResourceService<UserProfile, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _userTenantService = userTenantService;
            _userService = userService;
            _setupService = setupService;
            _userManager = userManager;
            _jsonApiManualService = jsonApiManualService;
            _configuration = configuration;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] UserProfile resource, CancellationToken cancellationToken)
        {
            StringValues testNoLog;

            StringValues tenant;
            HttpContext.Request.Headers.TryGetValue("TenantId", out tenant);
            int tenantId = Int32.Parse(tenant);
            String tenantDestinationName = "";
            StringValues tenantDestination = "";
            HttpContext.Request.Headers.TryGetValue("TenantDestinationId", out tenantDestination);
            if (!String.IsNullOrEmpty(tenantDestination) && tenantDestination != "" && tenantDestination != tenant)
            {
                try
                {
                    int tenantDestinationId = Int32.Parse(tenantDestination);
                    List<Tenant> childrenTenants = _jsonApiManualService.GetAllChildrenTenants(tenantId).Result;
                    if (!_jsonApiManualService.CheckIfTenantIsChild(tenantId, tenantDestinationId, true).Result)
                    {
                        return StatusCode(406);
                    }
                    else
                    {
                        tenantId = tenantDestinationId;
                    }

                    try { tenantDestinationName = _jsonApiManualService.FirstOrDefault<Tenant, int>(x => x.Id == tenantDestinationId).Name; }
                    catch (Exception) { }

                }
                catch (Exception) { }

            }



            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            StringValues rolesValues;
            HttpContext.Request.Headers.TryGetValue("roles", out rolesValues);

            List<Role> roles = [];
            if (!String.IsNullOrEmpty(rolesValues))
            {
                roles = _jsonApiManualService.GetAllRolesByRoleIdList(rolesValues);
            }

            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());

            User existingUser = _jsonApiManualService.FirstOrDefault<User, string>(x => x.Email == resource.ContactEmail);


            string roleByPassOtpCustomSettings = _configuration["RolesByPassOtp"];
            List<string> roleByPassOtp = [];

            if (!roleByPassOtpCustomSettings.IsNullOrEmpty())
            {
                roleByPassOtp = roleByPassOtpCustomSettings.Split(",").ToList();
            }

            if (existingUser != null)
            {
                List<UserRole> userRoles = await _jsonApiManualService.GetAllUserRolesByEmail(existingUser.Email);

                List<Role> checkRole = [];
                foreach (UserRole userRole in userRoles)
                {
                    Role infoRole = _jsonApiManualService.GetById<Role, string>(userRole.RoleId);
                    checkRole.Add(infoRole);
                }
                //controllo se quell'utente esiste gia sullo stesso tenant
                UserTenant existingUserInTenat = _jsonApiManualService.FirstOrDefault<UserTenant, string>(x => x.UserId == existingUser.Id && x.TenantId == tenantId);
                if (existingUserInTenat != null)
                {
                    //controllo se é associato ma in pending
                    if (existingUserInTenat.AcceptedAt == null)
                    {

                        if (checkRole.Any(role => roleByPassOtp.Contains(role.Name)))
                        {
                            existingUserInTenat.State = "ownerCreated";
                            existingUserInTenat.AcceptedAt = DateTime.Now;
                            return StatusCode(200);
                        }

                        return StatusCode(412);

                    }
                    //altrimenti do errore
                    return StatusCode(409);
                }
                else
                {
                    if (setup.needRequestAssociation && !checkRole.Any(role => roleByPassOtp.Contains(role.Name)))
                    {
                        UserTenant ut = await _jsonApiManualService.RegisterUserTenantPending(existingUser.Id, tenantId);

                        Otp otp = await _jsonApiManualService.GenerateNewOtp(existingUser.Id, ut.Id, tenantId);

                        if (String.IsNullOrEmpty(rolesValues))
                        {
                            await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenantId, "User");
                        }
                        else if (roles.Count > 0)
                        {
                            foreach (Role role in roles)
                            {
                                await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenantId, role.Name);
                            }
                        }
                        else
                        {
                            return StatusCode(403);
                        }

                        HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                        return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                            ? StatusCode(200)
                            : !await _jsonApiManualService.SendOtpEmail(otp.OtpSended, setupType.ToString(), baseEndpoint)
                            ? StatusCode(428)
                            : StatusCode(451);
                    }
                    else
                    {
                        await _jsonApiManualService.RegisterUserTenant(existingUser.Id, tenantId, HttpContext.Connection.RemoteIpAddress.ToString(), "ownerCreated");

                        if (String.IsNullOrEmpty(rolesValues) || roles.Count == 0)
                        {
                            await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenantId, "User");
                        }
                        else if (roles.Count > 0)
                        {
                            foreach (Role role in roles)
                            {
                                await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenantId, role.Name);
                            }
                        }

                        Dictionary<string, string> customContent = new()     {
                            {"{tenantName}", tenantDestinationName }
                        };

                        await _jsonApiManualService.SendGenericEmail(existingUser.Email, "eb3c00b6-ed30-414c-b908-dd542b8db86c", tenantId, baseEndpoint, customContent);


                        return StatusCode(201);
                    }
                }
            }

            string password = await _jsonApiManualService.CalculateMD5Hash(setup.defaultUserPassword);

            MITApplicationUser user = new() { UserName = resource.ContactEmail, Email = resource.ContactEmail, PasswordLastChange = new DateTime(1975, 12, 10), TenantId = tenantId, EmailConfirmed = true };
            IdentityResult result = await _userManager.CreateAsync(user, password);

            resource.UserId = user.Id;
            resource.Id = Guid.NewGuid().ToString();

            await _resourceService.CreateAsync(resource, cancellationToken);

            User userJsonapi = await _jsonApiManualService.GetUserByEmail(resource.ContactEmail);
            userJsonapi.IsPasswordMd5 = true;
            userJsonapi.PasswordLastChange = new DateTime(1975, 12, 10);
            userJsonapi.Deleted = false;
            await _jsonApiManualService.UpdateAsync<User, string>(userJsonapi);


            await _jsonApiManualService.RegisterUserTenant(user.Id, tenantId, HttpContext.Connection.RemoteIpAddress.ToString(), "ownerCreated");

            if (String.IsNullOrEmpty(rolesValues))
            {
                await _jsonApiManualService.RegisterUserRole(user.Id, tenantId, "User");
            }
            else if (roles.Count > 0)
            {
                foreach (Role role in roles)
                {
                    await _jsonApiManualService.RegisterUserRole(user.Id, tenantId, role.Name);
                }
            }
            else
            {
                return StatusCode(403);
            }

            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                ? StatusCode(201)
                : !await _jsonApiManualService.SendGenericEmail(user.UserName, "f4646e12-f847-4ba9-bbb3-d60bac1f473d", tenantId, baseEndpoint)
                ? StatusCode(428)
                : StatusCode(201);
        }

    }
}

