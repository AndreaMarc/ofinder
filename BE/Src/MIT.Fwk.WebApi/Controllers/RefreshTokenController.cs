using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Entities.AccountViewModels;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.WebApi.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("refreshtoken")]
    public class RefreshTokenController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonApiManualService;
        private readonly UserManager<MITApplicationUser> _userManager;
        private readonly JwtOptions _jwtOptions;
        private readonly SmtpOptions _smtpOptions;
        private readonly IConfiguration _configuration;

        public RefreshTokenController(
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IJsonApiManualService jsonApiManualService,
            IOptions<JwtOptions> jwtOptions,
            IOptions<SmtpOptions> smtpOptions,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _jsonApiManualService = jsonApiManualService;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
            _smtpOptions = smtpOptions.Value;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            StringValues bearer;
            HttpContext.Request.Headers.TryGetValue("Authorization", out bearer);
            JwtSecurityToken jwt = new(bearer.ToString().Substring(7));
            string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
            string password = jwt.Claims.ToList().Find(x => x.Type == "password").Value;
            bool superAdmin = bool.Parse(jwt.Claims.ToList().Find(x => x.Type == "superadmin").Value);
            MITApplicationUser currentUser = await _userManager.FindByEmailAsync(userEmail);

            StringValues tenantId;
            HttpContext.Request.Headers.TryGetValue("tenantId", out tenantId);

            if (currentUser == null)
            {
                return Forbid();
            }

            BannedUser bannedUser = await _jsonApiManualService.IsBanned(currentUser.Id);

            if (bannedUser != null)
            {
                return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : true, \"TenantId\" : 0 }"));
            }

            bannedUser = await _jsonApiManualService.IsBanned(currentUser.Id, int.Parse(tenantId));

            if (bannedUser != null)
            {
                return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : false, \"TenantId\" : " + tenantId + " }"));
            }

            int accessExpiresIn;
            int refreshExpiresIn;

            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);
            StringValues deviceHash;
            HttpContext.Request.Headers.TryGetValue("fingerPrint", out deviceHash);
            StringValues userLang;
            HttpContext.Request.Headers.TryGetValue("userLang", out userLang);



            UserDevice device = await _jsonApiManualService.GetUserDevice(currentUser.Id, deviceHash);

            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());

            if (setup == null || setup.accessTokenExpiresIn == 0 || setup.refreshTokenExpiresIn == 0)
            {
                accessExpiresIn = _jwtOptions.AccessTokenExpiresIn;
                refreshExpiresIn = _jwtOptions.RefreshTokenExpiresIn;

            }
            else
            {
                accessExpiresIn = setup.accessTokenExpiresIn;
                refreshExpiresIn = setup.refreshTokenExpiresIn;

            }

            bool firstDailyAccess = (currentUser.LastAccess.HasValue && !currentUser.LastAccess.Value.Date.Equals(DateTime.UtcNow.Date));

            System.Net.Http.Headers.AuthenticationHeaderValue bearerToken = (await HttpContext.RefreshJwtBearer(refreshExpiresIn));
            if (bearerToken == null)
            {
                HttpContext.Response.Headers.Remove("Set-Cookie");
                return Unauthorized("Token non valido.");
            }

            UserProfile up = await _jsonApiManualService.GetUserProfileByUsername(currentUser.UserName);

            if (!string.IsNullOrEmpty(userLang) && userLang != up.UserLang)
            {
                up.UserLang = userLang;
                await _jsonApiManualService.UpdateUserProfile(up);
            }

            List<Tenant> tenants = await _jsonApiManualService.GetTenantsByUsername(currentUser.UserName);

            List<AssociatedTenant> asTen = [];

            foreach (Tenant t in tenants)
            {
                if (!t.Enabled)
                {
                    continue;
                }

                AssociatedTenant tempAsTen = new()
                {
                    TenantId = t.Id,
                    Name = t.Name,
                };

                asTen.Add(tempAsTen);
            }

            User userToUpdate = await _jsonApiManualService.GetUserByEmail(currentUser.Email);
            userToUpdate.LastAccess = DateTime.UtcNow;
            await _jsonApiManualService.UpdateAsync<User, string>(userToUpdate);
            device.lastAccess = DateTime.UtcNow;
            await _jsonApiManualService.UpdateUserDevice(device);

            HttpContext.Response.Headers.Remove("Set-Cookie");
            return Ok(new CurrentUserViewModel()
            {
                Email = currentUser.Email,
                FirstName = up.FirstName,
                LastName = up.LastName,
                AuthorizationHeader = string.Empty,
                AuthorizationBearer = (await HttpContext.ToJwtBearer(currentUser.UserName, "559e2f95-ee64-45af-b6c7-30801842496f", password, accessExpiresIn, device.salt, superAdmin, tenantId)).ToString(),
                AuthorizationExpiresIn = accessExpiresIn,
                AuthorizationRefreshBearer = bearerToken.ToString(),
                AuthorizationRefreshExpiresIn = refreshExpiresIn,
                SecurityStamp = currentUser.SecurityStamp,
                IsAuthenticated = true,
                IsEnabled = true,
                DefaultLanguage = up.UserLang,
                TenantId = int.Parse(tenantId),
                Id = currentUser.Id,
                ClaimsList = await _jsonApiManualService.GetClaimsByUsername(currentUser.UserName, tenantId),
                AssociatedTenants = asTen,
                ProfileImageId = up.ProfileImageId,
                CurrentTenantActive = tenants.First(x => x.Id == int.Parse(tenantId)).Enabled,
                TermsAcceptanceDate = up.termsAcceptanceDate == null ? null : up.termsAcceptanceDate.Value.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00"
            });
        }

        [HttpGet]
        [Route("changeTenant")]
        public async Task<IActionResult> GetChangeTenant()
        {
            StringValues bearer;
            HttpContext.Request.Headers.TryGetValue("Authorization", out bearer);
            JwtSecurityToken jwt = new(bearer.ToString().Substring(7));
            string userEmail = jwt.Claims.ToList().Find(x => x.Type == "username").Value;
            string oldTenant = jwt.Claims.ToList().Find(x => x.Type == "tenantId").Value;
            string password = jwt.Claims.ToList().Find(x => x.Type == "password").Value;
            bool superAdmin = bool.Parse(jwt.Claims.ToList().Find(x => x.Type == "superadmin").Value);
            MITApplicationUser currentUser = await _userManager.FindByEmailAsync(userEmail);

            if (currentUser == null)
            {
                return Forbid();
            }

            BannedUser bannedUser = await _jsonApiManualService.IsBanned(currentUser.Id);

            if (bannedUser != null)
            {
                return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : true, \"TenantId\" : 0 }"));
            }

            bannedUser = await _jsonApiManualService.IsBanned(currentUser.Id, int.Parse(oldTenant));

            if (bannedUser != null)
            {
                return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : false, \"TenantId\" : " + oldTenant + " }"));
            }

            int accessExpiresIn;
            int refreshExpiresIn;

            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);
            StringValues deviceHash;
            HttpContext.Request.Headers.TryGetValue("fingerPrint", out deviceHash);

            UserDevice device = await _jsonApiManualService.GetUserDevice(currentUser.Id, deviceHash);

            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());

            if (setup == null || setup.accessTokenExpiresIn == 0 || setup.refreshTokenExpiresIn == 0)
            {
                accessExpiresIn = _jwtOptions.AccessTokenExpiresIn;
                refreshExpiresIn = _jwtOptions.RefreshTokenExpiresIn;

            }
            else
            {
                accessExpiresIn = setup.accessTokenExpiresIn;
                refreshExpiresIn = setup.refreshTokenExpiresIn;

            }


            bool firstDailyAccess = (currentUser.LastAccess.HasValue && !currentUser.LastAccess.Value.Date.Equals(DateTime.UtcNow.Date));

            StringValues tenantId;
            HttpContext.Request.Headers.TryGetValue("tenantId", out tenantId);

            if (tenantId != oldTenant)
            {
                bannedUser = await _jsonApiManualService.IsBanned(currentUser.Id, int.Parse(tenantId));

                if (bannedUser != null)
                {
                    return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : false, \"TenantId\" : " + tenantId + " }"));
                }

                Tenant existingAssociation = (await _jsonApiManualService.GetTenantsByUsername(userEmail)).FirstOrDefault(x => x.Id == int.Parse(tenantId));

                if (existingAssociation == null)
                {
                    bool associated = false;

                    if (await _jsonApiManualService.CheckIsSuperadmin(currentUser.Id))
                    {
                        await _jsonApiManualService.RegisterUserTenant(currentUser.Id, int.Parse(tenantId), HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                        UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), "SuperAdmin");
                        associated = true;
                    }

                    if (await _jsonApiManualService.CheckIsOwner(currentUser.Id))
                    {
                        if (!associated)
                        {
                            await _jsonApiManualService.RegisterUserTenant(currentUser.Id, int.Parse(tenantId), HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                        }
                        UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), "Owner");
                        associated = true;
                    }

                    if (!String.IsNullOrEmpty(_configuration["Travelers"]))
                    {
                        foreach (string traveler in _configuration["Travelers"].Split(",").ToList())
                        {
                            if (await _jsonApiManualService.CheckClaimsById(currentUser.Id, "1", ["is" + traveler]))
                            {
                                if (!associated)
                                {
                                    await _jsonApiManualService.RegisterUserTenant(currentUser.Id, int.Parse(tenantId), HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                                }

                                UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), traveler);
                                associated = true;
                            }
                        }
                    }

                    if (!associated)
                    {
                        return StatusCode(401, JsonConvert.DeserializeObject("{ \"Error\": \"Association with this tenant not existing.\"}"));
                    }
                }
                else
                {
                    List<UserRole> thisUserRoles = await _jsonApiManualService.GetAllUserRolesByEmail(currentUser.Email);
                    List<Role> allTenant1Roles = _jsonApiManualService.GetAllRolesByTenant(1);

                    if (await _jsonApiManualService.CheckIsSuperadmin(currentUser.Id))
                    {
                        if (thisUserRoles.Find(x => x.TenantId == int.Parse(tenantId) && x.RoleId == allTenant1Roles.First(x => x.Name == "SuperAdmin").Id) == null)
                        {
                            UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), "SuperAdmin");
                        }
                    }

                    if (await _jsonApiManualService.CheckIsOwner(currentUser.Id))
                    {
                        if (thisUserRoles.Find(x => x.TenantId == int.Parse(tenantId) && x.RoleId == allTenant1Roles.First(x => x.Name == "Owner").Id) == null)
                        {
                            UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), "Owner");
                        }
                    }

                    if (!String.IsNullOrEmpty(_configuration["Travelers"]))
                    {
                        foreach (string traveler in _configuration["Travelers"].Split(",").ToList())
                        {
                            if (await _jsonApiManualService.CheckClaimsById(currentUser.Id, "1", ["is" + traveler]))
                            {
                                if (thisUserRoles.Find(x => x.TenantId == int.Parse(tenantId) && x.RoleId == allTenant1Roles.First(x => x.Name == traveler).Id) == null)
                                {
                                    UserRole newRole = await _jsonApiManualService.RegisterUserRole(currentUser.Id, int.Parse(tenantId), traveler);
                                }
                            }
                        }
                    }
                }
            }

            System.Net.Http.Headers.AuthenticationHeaderValue bearerToken = (await HttpContext.RefreshJwtBearer(refreshExpiresIn, tenantId.ToString(), currentUser.Id));
            if (bearerToken == null)
            {
                return Unauthorized("Token non valido.");
            }

            UserProfile up = await _jsonApiManualService.GetUserProfileByUsername(currentUser.UserName);

            List<Tenant> tenants = await _jsonApiManualService.GetTenantsByUsername(currentUser.UserName);

            List<AssociatedTenant> asTen = [];

            foreach (Tenant t in tenants)
            {
                if (!t.Enabled)
                {
                    continue;
                }

                AssociatedTenant tempAsTen = new()
                {
                    TenantId = t.Id,
                    Name = t.Name,
                };

                asTen.Add(tempAsTen);
            }

            User userToUpdate = await _jsonApiManualService.GetUserByEmail(currentUser.Email);
            userToUpdate.LastAccess = DateTime.UtcNow;
            await _jsonApiManualService.UpdateAsync<User, string>(userToUpdate);
            device.lastAccess = DateTime.UtcNow;
            await _jsonApiManualService.UpdateUserDevice(device);

            return Ok(new CurrentUserViewModel()
            {
                Email = currentUser.Email,
                FirstName = up.FirstName,
                LastName = up.LastName,
                AuthorizationHeader = string.Empty,
                AuthorizationBearer = (await HttpContext.ToJwtBearer(currentUser.UserName, "559e2f95-ee64-45af-b6c7-30801842496f", password, accessExpiresIn, device.salt, superAdmin, tenantId)).ToString(),
                AuthorizationExpiresIn = accessExpiresIn,
                AuthorizationRefreshBearer = bearerToken.ToString(),
                AuthorizationRefreshExpiresIn = refreshExpiresIn,
                SecurityStamp = currentUser.SecurityStamp,
                IsAuthenticated = true,
                IsEnabled = true,
                DefaultLanguage = "IT-it",
                TenantId = int.Parse(tenantId),
                Id = currentUser.Id,
                ClaimsList = await _jsonApiManualService.GetClaimsByUsername(currentUser.UserName, tenantId),
                AssociatedTenants = asTen,
                ProfileImageId = up.ProfileImageId,
                CurrentTenantActive = tenants.First(x => x.Id == int.Parse(tenantId)).Enabled,
                TermsAcceptanceDate = up.termsAcceptanceDate == null ? null : up.termsAcceptanceDate.Value.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00"
            });
        }
    }
}
