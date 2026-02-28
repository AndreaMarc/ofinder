using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Entities.AccountViewModels;
using MIT.Fwk.Infrastructure.Entities.ManageViewModels;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.WebApi.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebSocketSharp;
using static Google.Apis.Requests.BatchRequest;
using static MIT.Fwk.Infrastructure.Services.GoogleService;

namespace MIT.Fwk.WebApi.Controllers
{
    [SkipJwtAuthentication]
    [SkipClaimsValidation]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<MITApplicationUser> _signInManager;
        private readonly IJsonApiManualService _jsonApiManualService;
        private readonly IGoogleService _googleService;
        private readonly IEmailSender _mail;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected ILogger _logger;
        private readonly JwtOptions _jwtOptions;

        public class DisconnectAll
        {
            public string userId { get; set; }
            public string deviceHash { get; set; }
            public bool isMine { get; set; }
        }

        public class PermissionResult
        {
            public string otp { get; set; }
            public bool result { get; set; }
        }

        public class SendPermissionAgainModel
        {
            public string userTenantId { get; set; }
        }

        public class SendConfirmationAgainAfterLoginModel
        {
            public string userEmail { get; set; }
        }

        public class SendRegisterOtpModel
        {
            public string otp { get; set; }
        }

        public AccountController(
            SignInManager<MITApplicationUser> signInManager,
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IJsonApiManualService jsonApiManualService,
            IGoogleService googleService,
            IEmailSender email,
            IOptions<JwtOptions> jwtOptions,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AccountController>();
            _signInManager = signInManager;
            _mail = email;
            _jsonApiManualService = jsonApiManualService;
            _googleService = googleService;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
           
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/getExitingHash")]
        public virtual async Task<IActionResult> getExitingHash([FromBody] LoginToOtherAppViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Integration integration = _jsonApiManualService.FirstOrDefault<Integration, string>(x => x.Id == model.IntegrationId);

            if (integration == null)
            {
                return NotFound();
            }
            else if (!integration.Active)
            {
                return Forbid();
            }

            User user = await _jsonApiManualService.GetUserById(model.UserId);

            return StatusCode(200, "{ \"Hash\": \"" + await _jsonApiManualService.Encrypt($"{user.Email}|{DateTime.UtcNow}|{integration.Code}", integration.EncryptionKey) + "\" }");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/AccessRequestWithHash")]
        public virtual async Task<IActionResult> AccessRequestWithHash([FromBody] AccessRequestWithHash model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Setup webSetup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");

            GoogleInfos googleInfos = await _googleService.GetGoogleInfos(webSetup);

            Integration integration = _jsonApiManualService.FirstOrDefault<Integration, string>(x => x.Code == model.Code);

            if (integration == null)
            {
                return NotFound();
            }

            if (!integration.Active) { return Forbid(); }

            string[] hashInfos = (await _jsonApiManualService.Decrypt(model.Hash, integration.EncryptionKey)).Split("|");

            if (hashInfos.Length != 3 || hashInfos[2] != integration.Code)
            {
                return Conflict();
            }

            User utente = await _jsonApiManualService.GetUserByGoogleEmail(hashInfos[0]);

            if (utente == null)
            {
                utente = await _jsonApiManualService.GetUserByEmail(hashInfos[0]);
            }

            if (!HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            }

            if (utente == null) //Register
            {
                if (!webSetup.publicRegistration)
                {
                    return Ok(new { url = googleInfos.RedirectAfterGoogleError + "?error=NRA" });
                }

                Otp otp = await _jsonApiManualService.GenerateNewOtp("registering", hashInfos[0], 0);

                return Ok(new { url = googleInfos.RedirectAfterGoogleRegister + "?otp=" + otp.OtpSended });

            }
            else //Login
            {
                Otp otp = await _jsonApiManualService.GenerateNewOtp(utente.Id, hashInfos[0], utente.TenantId);


                return Ok(new { url = googleInfos.RedirectAfterGoogleLogin + "?otp=" + otp.OtpSended });

            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/login")]
        [Consumes("application/vnd.api+json")]

        public virtual async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MITApplicationUser user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!user.EmailConfirmed)
            {
                return StatusCode(418);
            }

            BannedUser bannedUser = await _jsonApiManualService.IsBanned(user.Id);

            if (bannedUser != null)
            {

                return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : true, \"TenantId\" : 0 }"));
            }

            bannedUser = await _jsonApiManualService.IsBanned(user.Id, user.TenantId);

            List<Tenant> nonBlockedTenants = await _jsonApiManualService.GetNonBlockedTenantsByUserId(user.Id); //todo ottimizzare

            User jsonApiUser = await _jsonApiManualService.GetUserById(user.Id);
            if (bannedUser != null)
            {
                if (nonBlockedTenants.FirstOrDefault() == null)
                {
                    HttpContext.Response.Headers.Remove("Set-Cookie");
                    return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : false, \"TenantId\" : " + user.TenantId + " }"));
                }


                jsonApiUser.TenantId = nonBlockedTenants.FirstOrDefault().Id;

                await _jsonApiManualService.UpdateAsync<User, string>(jsonApiUser);
            }

            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            UserDevice device = await _jsonApiManualService.GetUserDevice(user.Id, model.FingerPrint);

            if (device == null)
            {
                StringValues deviceType;
                if (setupType == "app")
                {
                    HttpContext.Request.Headers.TryGetValue("appPlatform", out deviceType);
                }
                else
                {
                    deviceType = setupType;
                }

                StringValues deviceName;
                HttpContext.Request.Headers.TryGetValue("devicename", out deviceName);

                UserDevice ud = new()
                {
                    deviceHash = model.FingerPrint,
                    userId = user.Id,
                    salt = await _jsonApiManualService.NewSalt(),
                    lastAccess = DateTime.UtcNow,
                    Platform = deviceType,
                    DeviceName = deviceName
                };
                device = await _jsonApiManualService.CreateUserDevice(ud);
                //TODO: Logiche per due fattori ecc..
            }
            else
            {
                device.lastAccess = DateTime.UtcNow;
                await _jsonApiManualService.UpdateUserDevice(device);
            }

            Setup setup = setupType.ToString() == ""
                ? _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web")
                : _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());
            string passwordToUse = model.Password;
            MITApplicationUser currentUser = null;
            HttpClientExtentions.LoginResultModel result = null;
            if (setup.useMD5)
            {
                //se me la passa in md5 controllo che sia in md5 anche quella dell'utente
                //se lo é ok, altrimenti deve fare il reset
                if (!(bool)jsonApiUser.IsPasswordMd5)
                {
                    //todo trovare un codice adatto
                    return StatusCode(403);

                }
                else
                {
                    result = await HttpContext.AuthenticateUser(model.Email, passwordToUse, _jsonApiManualService);
                    currentUser = result.user;
                }

            }
            else
            {

                //se me la passa in chiaro, se nel db é gia in md5 trasformo in md5 quella che mi arriva per confrontalra, altrimenti la trasformo in md5 e poi l'aggiorno all'utente in md5
                if (!(bool)jsonApiUser.IsPasswordMd5)
                {
                    passwordToUse = await _jsonApiManualService.CalculateMD5Hash(model.Password);
                    result = await HttpContext.AuthenticateUser(model.Email, model.Password, _jsonApiManualService);
                    currentUser = result.user;

                    if (currentUser != null)
                    {
                        IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, model.Password, passwordToUse);

                        //scrivo nelle password hisotry la nuova password

                        if (changePasswordResult.Succeeded)
                        {
                            jsonApiUser = await _jsonApiManualService.GetUserByEmail(jsonApiUser.Email);
                            jsonApiUser.IsPasswordMd5 = true;
                            await _jsonApiManualService.UpdateAsync<User, string>(jsonApiUser);
                            await _jsonApiManualService.CreatePasswordHistory(user.Id, user.Email, passwordToUse, DateTime.Now);
                        }
                        else
                        {
                            HttpContext.Response.Headers.Remove("Set-Cookie");
                            return StatusCode(403);
                        }
                    }
                }
                else
                {
                    passwordToUse = await _jsonApiManualService.CalculateMD5Hash(model.Password);
                    result = await HttpContext.AuthenticateUser(model.Email, passwordToUse, _jsonApiManualService); //todo ottimizzare
                    currentUser = result.user;
                }
            }

            if (currentUser == null)
            {
                if (result.message == "Invalid password.")
                {
                    HttpContext.Response.Headers.Remove("Set-Cookie");
                    return StatusCode(401, JsonConvert.DeserializeObject("{ \"AttemptsRemaining\": " + (setup.failedLoginAttempts - (await _jsonApiManualService.GetUserByEmail(model.Email)).AccessFailedCount) + ", \"AccountLocked\": false, \"LockExpiresIn\": \"\"}"));
                }
                else if (result.message == "Account locked.")
                {

                    string time = (await _jsonApiManualService.GetUserByEmail(model.Email)).LockoutEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00";
                    HttpContext.Response.Headers.Remove("Set-Cookie");
                    return StatusCode(401, JsonConvert.DeserializeObject("{ \"AttemptsRemaining\": 0, \"AccountLocked\": true, \"LockExpiresIn\": \"" + time + "\" }"));
                }
                else
                {
                    HttpContext.Response.Headers.Remove("Set-Cookie");
                    return StatusCode(409);
                }
            }

            bool firstDailyAccess = (currentUser.LastAccess.HasValue && !currentUser.LastAccess.Value.Date.Equals(DateTime.UtcNow.Date));

            int accessExpiresIn, refreshExpiresIn;

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

            _logger.LogInformation(1, "User logged in.");
            //var currentUser = _userManager.FindByEmailAsync(model.Email).Result;
            //var issuperuser = currentUser.Roles.Exists(x => x.Id == "2c77b599-5b43-4589-87c1-69a1e0ebe3f7");

            UserProfile up = await _jsonApiManualService.GetUserProfileByUsername(currentUser.UserName);
            if (!model.UserLang.IsNullOrEmpty() && model.UserLang != up.UserLang)
            {
                up.UserLang = model.UserLang;
                await _jsonApiManualService.UpdateUserProfile(up);
            }

            List<Tenant> tenants = await _jsonApiManualService.GetTenantsByUsername(currentUser.UserName); //todo ottimizzare

            List<AssociatedTenant> asTen = [];

            foreach (Tenant t in tenants)
            {
                if (!t.Enabled && !nonBlockedTenants.Select(x => x.Id).Contains(t.Id))
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

            if (tenants.FirstOrDefault(x => x.Id == currentUser.TenantId) == null)
            {
                currentUser.TenantId = tenants.First().Id;
                jsonApiUser = await _jsonApiManualService.GetUserByEmail(jsonApiUser.Email);
                jsonApiUser.TenantId = tenants.First().Id;
                await _jsonApiManualService.UpdateAsync<User, string>(jsonApiUser);
            }

            CurrentUserViewModel response = new()
            {
                Email = currentUser.Email,
                FirstName = up.FirstName,
                LastName = up.LastName,
                // AuthorizationHeader = currentUser.UserName.ToAuthHeaderValue(model.Password).ToString(),
                AuthorizationBearer = (await HttpContext.ToJwtBearer(currentUser.UserName, passwordToUse, user.PasswordHash, accessExpiresIn, device.salt, await _jsonApiManualService.CheckIsSuperadmin(currentUser.Id), currentUser.TenantId.ToString())).ToString(),
                AuthorizationRefreshBearer = (await HttpContext.ToJwtBearerRefresh(currentUser.UserName, passwordToUse, user.PasswordHash, refreshExpiresIn, device.salt, await _jsonApiManualService.CheckIsSuperadmin(currentUser.Id), currentUser.TenantId.ToString())).ToString(),
                AuthorizationExpiresIn = accessExpiresIn,
                AuthorizationRefreshExpiresIn = refreshExpiresIn,
                SecurityStamp = currentUser.SecurityStamp,
                IsAuthenticated = true,
                IsEnabled = true,
                DefaultLanguage = up.UserLang,
                TenantId = currentUser.TenantId,
                Id = currentUser.Id,
                ClaimsList = await _jsonApiManualService.GetClaimsByUsername(currentUser.UserName, currentUser.TenantId.ToString()),
                AssociatedTenants = asTen,
                ProfileImageId = up.ProfileImageId,
                CurrentTenantActive = tenants.First(x => x.Id == currentUser.TenantId).Enabled,
                TermsAcceptanceDate = up.termsAcceptanceDate == null ? null : up.termsAcceptanceDate.Value.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00"
            };

            HttpContext.Response.Headers.Remove("Set-Cookie");

            return Ok(new { success = true, data = response });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/loginOtp")]
        [Consumes("application/vnd.api+json")]
        public virtual async Task<IActionResult> LoginOTP([FromBody] LoginOTPViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            Setup setup = setupType.ToString() == ""
                ? _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web")
                : _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());
            Otp otp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == model.Otp);

            if (otp == null)
            {
                return NotFound("Otp not found");
            }
            else if (!otp.IsValid || (DateTime.UtcNow - otp.CreationDate) > TimeSpan.FromMinutes(setup.mailTokenExpiresIn))
            {
                otp.IsValid = false;

                await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

                return StatusCode(406);
            }

            otp.IsValid = false;

            await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

            User user = await _jsonApiManualService.GetUserByEmail(otp.OtpValue);

            if (user == null)
            {
                return StatusCode(422);
            }

            BannedUser bannedUser = await _jsonApiManualService.IsBanned(user.Id);

            List<Tenant> nonBlockedTenants = await _jsonApiManualService.GetNonBlockedTenantsByUserId(user.Id);

            User jsonApiUser = await _jsonApiManualService.GetUserById(user.Id);

            if (bannedUser != null)
            {
                if (nonBlockedTenants.FirstOrDefault() == null)
                {
                    return StatusCode(423, JsonConvert.DeserializeObject("{ \"LockEnd\" : \"" + bannedUser.LockEnd.Value.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00" + "\", \"CrossTenantBanned\" : false, \"TenantId\" : " + user.TenantId + " }"));
                }


                jsonApiUser.TenantId = nonBlockedTenants.FirstOrDefault().Id;

                await _jsonApiManualService.UpdateAsync<User, string>(jsonApiUser);
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
            }

            user.LastAccess = DateTime.UtcNow;

            await _jsonApiManualService.UpdateAsync<User, string>(user);

            UserDevice device = await _jsonApiManualService.GetUserDevice(user.Id, model.FingerPrint);

            if (device == null)
            {
                StringValues deviceType;
                if (setupType == "app")
                {
                    HttpContext.Request.Headers.TryGetValue("appPlatform", out deviceType);
                }
                else
                {
                    deviceType = setupType;
                }

                StringValues deviceName;
                HttpContext.Request.Headers.TryGetValue("devicename", out deviceName);

                UserDevice ud = new()
                {
                    deviceHash = model.FingerPrint,
                    userId = user.Id,
                    salt = await _jsonApiManualService.NewSalt(),
                    lastAccess = DateTime.UtcNow,
                    Platform = deviceType,
                    DeviceName = deviceName
                };
                device = await _jsonApiManualService.CreateUserDevice(ud);
                //TODO: Logiche per due fattori ecc..
            }
            else
            {
                device.lastAccess = DateTime.UtcNow;
                await _jsonApiManualService.UpdateUserDevice(device);
            }

            int accessExpiresIn, refreshExpiresIn;

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

            _logger.LogInformation(1, "User logged in.");
            //var currentUser = _userManager.FindByEmailAsync(model.Email).Result;
            //var issuperuser = currentUser.Roles.Exists(x => x.Id == "2c77b599-5b43-4589-87c1-69a1e0ebe3f7");

            UserProfile up = await _jsonApiManualService.GetUserProfileByUsername(user.UserName);

            if (!model.UserLang.IsNullOrEmpty() && model.UserLang != up.UserLang)
            {
                up.UserLang = model.UserLang;
                await _jsonApiManualService.UpdateUserProfile(up);
            }

            List<Tenant> tenants = await _jsonApiManualService.GetTenantsByUsername(user.UserName);

            List<AssociatedTenant> asTen = [];

            foreach (Tenant t in tenants)
            {
                if (!t.Enabled && !nonBlockedTenants.Select(x => x.Id).Contains(t.Id))
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

            if (tenants.FirstOrDefault(x => x.Id == user.TenantId) == null)
            {
                user.TenantId = tenants.First().Id;
                jsonApiUser = await _jsonApiManualService.GetUserByEmail(jsonApiUser.Email);
                jsonApiUser.TenantId = tenants.First().Id;
                await _jsonApiManualService.UpdateAsync<User, string>(jsonApiUser);
            }

            CurrentUserViewModel response = new()
            {
                Email = user.Email,
                FirstName = up.FirstName,
                LastName = up.LastName,
                // AuthorizationHeader = currentUser.UserName.ToAuthHeaderValue(model.Password).ToString(),
                AuthorizationBearer = (await HttpContext.ToJwtBearer(user.UserName, "559e2f95-ee64-45af-b6c7-30801842496f", user.PasswordHash, accessExpiresIn, device.salt, await _jsonApiManualService.CheckIsSuperadmin(user.Id), user.TenantId.ToString())).ToString(),
                AuthorizationRefreshBearer = (await HttpContext.ToJwtBearerRefresh(user.UserName, "7e4d0d09-6c42-4afd-a537-ea4ee51cc8a6", user.PasswordHash, refreshExpiresIn, device.salt, await _jsonApiManualService.CheckIsSuperadmin(user.Id), user.TenantId.ToString())).ToString(),
                AuthorizationExpiresIn = accessExpiresIn,
                AuthorizationRefreshExpiresIn = refreshExpiresIn,
                SecurityStamp = user.SecurityStamp,
                IsAuthenticated = true,
                IsEnabled = true,
                DefaultLanguage = up.UserLang,
                TenantId = user.TenantId,
                Id = user.Id,
                ClaimsList = await _jsonApiManualService.GetClaimsByUsername(user.UserName, user.TenantId.ToString()),
                AssociatedTenants = asTen,
                ProfileImageId = up.ProfileImageId,
                CurrentTenantActive = tenants.First(x => x.Id == user.TenantId).Enabled,
                TermsAcceptanceDate = up.termsAcceptanceDate == null ? null : up.termsAcceptanceDate.Value.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture) + "+00:00"
            };

            HttpContext.Response.Headers.Remove("Set-Cookie");

            return Ok(new { success = true, data = response });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/logout")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout([FromBody] DisconnectAll disconnectOne)
        {
            //var currentUser = await GetCurrentUser(Request);
            await _jsonApiManualService.DisconnectOneDevice(disconnectOne.userId, disconnectOne.deviceHash);
            // await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("permission/send-again")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPermissionAgain([FromBody] SendPermissionAgainModel model)
        {
            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            UserTenant ut = await _jsonApiManualService.GetUserTenantById(model.userTenantId);

            if (ut == null)
            {
                return StatusCode(404);
            }

            Otp oldOtp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.UserId == ut.UserId && x.OtpValue == model.userTenantId && x.IsValid);

            if (oldOtp != null)
            {
                oldOtp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(oldOtp);
            }

            Otp newOtp = await _jsonApiManualService.GenerateNewOtp(ut.UserId, ut.Id, ut.TenantId);

            StringValues testNoLog;
            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                ? StatusCode(204)
                : !await _jsonApiManualService.SendOtpEmail(newOtp.OtpSended, setupType.ToString(), baseEndpoint)
                ? StatusCode(428)
                : StatusCode(204);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/confirmRegistrationAgain")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> confirmRegistrationAgain([FromBody] SendRegisterOtpModel sendRegisterOtp)
        {
            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            Otp oldOtp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == sendRegisterOtp.otp);

            if (oldOtp.IsValid)
            {
                oldOtp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(oldOtp);
            }

            User user = await _jsonApiManualService.GetUserById(oldOtp.UserId);

            if (user == null)
            {
                return NotFound();
            }

            Otp newOtp = await _jsonApiManualService.GenerateNewOtp(oldOtp.UserId, oldOtp.OtpValue, oldOtp.TenantId);

            StringValues testNoLog;
            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                ? StatusCode(204)
                : !await _jsonApiManualService.SendOtpEmail(newOtp.OtpValue, newOtp.OtpSended, setupType, newOtp.TenantId, baseEndpoint, "0a16c97f-b76e-4042-802e-7c72d6cc337c")
                ? StatusCode(428)
                : StatusCode(204);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/confirmRegistrationAgainAfterLogin")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> confirmRegistrationAgainAfterLogin([FromBody] SendConfirmationAgainAfterLoginModel model)
        {
            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            User user = await _jsonApiManualService.GetUserByEmail(model.userEmail);

            if (user == null)
            {
                return NotFound();
            }

            Otp oldOtp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.UserId == user.Id && x.OtpValue == user.Email && x.IsValid);

            if (oldOtp.IsValid)
            {
                oldOtp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(oldOtp);
            }

            Otp newOtp = await _jsonApiManualService.GenerateNewOtp(oldOtp.UserId, oldOtp.OtpValue, oldOtp.TenantId);

            StringValues testNoLog;
            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                ? StatusCode(204)
                : !await _jsonApiManualService.SendOtpEmail(newOtp.OtpValue, newOtp.OtpSended, setupType, newOtp.TenantId, baseEndpoint, "0a16c97f-b76e-4042-802e-7c72d6cc337c")
                ? StatusCode(428)
                : StatusCode(204);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/confirmRegistration")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> confirmRegistration([FromBody] SendRegisterOtpModel sendRegisterOtp)
        {
            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");

            Otp oldOtp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == sendRegisterOtp.otp);

            if (oldOtp == null)
            {
                return NotFound();
            }

            if (!oldOtp.IsValid || DateTime.UtcNow - oldOtp.CreationDate > TimeSpan.FromMinutes(setup.mailTokenExpiresIn))
            {
                return StatusCode(406);
            }

            oldOtp.IsValid = false;
            await _jsonApiManualService.UpdateAsync<Otp, string>(oldOtp);

            User user = await _jsonApiManualService.GetUserById(oldOtp.UserId);

            user.EmailConfirmed = true;

            await _jsonApiManualService.UpdateAsync<User, string>(user);

            return StatusCode(204);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("permission/result")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResultPermission([FromBody] PermissionResult permissionResult)
        {
            StringValues setupType;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);

            Setup setup = setupType.ToString() == ""
                ? _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web")
                : _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());
            Otp otp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == permissionResult.otp);

            if (otp == null)
            {
                return NotFound();
            }

            if (!otp.IsValid || (DateTime.UtcNow - otp.CreationDate > TimeSpan.FromMinutes(setup.mailTokenExpiresIn)))
            {
                otp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

                return StatusCode(406);
            }

            otp.IsValid = false;
            await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

            UserTenant ut = await _jsonApiManualService.GetUserTenantById(otp.OtpValue);

            if (ut.State != "pending")
            {
                return StatusCode(409);
            }

            if (permissionResult.result)
            {
                ut.State = "accepted";

                User user = await _jsonApiManualService.GetUserById(ut.UserId);

                await _jsonApiManualService.RegisterUserRole(user.Id, ut.TenantId, "User");

                StringValues baseEndpoint;
                HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                if (!(!string.IsNullOrEmpty(testNoLog) && testNoLog == "True"))
                {
                    if (!await _jsonApiManualService.SendGenericEmail(user.UserName, "9afe9b8c-de32-490a-873f-41171bcc1818", ut.TenantId, baseEndpoint))
                    {
                        return StatusCode(428);
                    }
                }

            }
            else
            {
                ut.State = "denied";
            }

            ut.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            ut.AcceptedAt = DateTime.UtcNow;

            await _jsonApiManualService.UpdateUserTenant(ut);

            return StatusCode(204);
        }

        [HttpPost]
        [Route("account/disconnectAllDevices")]
        public async Task<IActionResult> DisconnectAllDevices([FromBody] DisconnectAll disconnectAll)
        {
            List<UserDevice> devices = await _jsonApiManualService.GetAllUserDevices(disconnectAll.userId);

            if (disconnectAll.isMine)
            {
                UserDevice dev = devices.FirstOrDefault(x => x.deviceHash == disconnectAll.deviceHash);
                if (dev != null)
                {
                    devices.Remove(dev);
                }
            }

            foreach (UserDevice device in devices)
            {
                device.salt = await _jsonApiManualService.NewSalt();
                device.PushToken = null;
                device.AppleToken = null;
                device.GoogleToken = null;
                device.FacebookToken = null;
                device.TwitterToken = null;
                await _jsonApiManualService.UpdateUserDevice(device);
            }

            return StatusCode(204);
        }

        [HttpPost]
        [Route("account/disconnectOneDevice")]
        public async Task<IActionResult> DisconnectOneDevice([FromBody] DisconnectAll disconnectOne)
        {
            await _jsonApiManualService.DisconnectOneDevice(disconnectOne.userId, disconnectOne.deviceHash);

            return StatusCode(204);
        }
        private static bool IsValidCodiceFiscale(string codiceFiscale)
        {
            if (string.IsNullOrEmpty(codiceFiscale))
            {
                return false;
            }

            string pattern = @"^[A-Z]{6}\d{2}[A-Z]\d{2}[A-Z]\d{3}[A-Z]$";

            return Regex.IsMatch(codiceFiscale, pattern, RegexOptions.IgnoreCase);
        }
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        [HttpPost]
        //[Authorize(Policy = "NeededRoleLevel0")]
        [AllowAnonymous]
        [Route("account/register")]
        //todo
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StringValues setupType;
            StringValues wantedRolesSV;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);
            HttpContext.Request.Headers.TryGetValue("wantedRoles", out wantedRolesSV);

            string wantedRoles = wantedRolesSV.ToString();


            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());

            if (!setup.publicRegistration)
            {
                return StatusCode(423);
            }
            else if (model.FirstName.Length < 2 || model.LastName.Length < 2)
            {
                return StatusCode(411);
            }
            else if (model.termsAccepted == false)
            {
                return StatusCode(412);
            }
            else if (!IsValidEmail(model.Email))
            {
                return StatusCode(417);
            }

            int tenant = model.TenantId == null ? 1 : (int)model.TenantId;

            User existingUser = await _jsonApiManualService.GetUserByEmail(model.Email);

            List<String> allowedRoles = new List<String>() { "Cliente", "Modella", "Agency" };


            if (existingUser != null)
            {
                UserTenant existingAssociation = _jsonApiManualService.GetUserTenant(existingUser.Id, tenant);

                if (existingAssociation != null)
                {
                    if (existingAssociation.State == "denied" || existingAssociation.State == "pending")
                    {
                        existingAssociation.State = "selfCreated";
                        existingAssociation.AcceptedAt = DateTime.UtcNow;
                        await _jsonApiManualService.UpdateUserTenant(existingAssociation);
                        await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, "User");
                        if (!String.IsNullOrEmpty(wantedRoles))
                        {
                            foreach (string s in wantedRoles.Split(','))
                            {
                                if (allowedRoles.Contains(s))
                                {
                                    await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, s);
                                }
                            }
                        }


                        StringValues baseEndpoint;
                        HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                        StringValues testNoLog;
                        HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                        return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                            ? StatusCode(202)
                            : !await _jsonApiManualService.SendGenericEmail(model.Email, "eb3c00b6-ed30-414c-b908-dd542b8db87d", tenant, baseEndpoint)
                            ? StatusCode(428)
                            : StatusCode(202);
                    }

                    return StatusCode(409);
                }
                else
                {
                    await _jsonApiManualService.RegisterUserTenant(existingUser.Id, tenant, HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                    await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, "User");
                    if (!String.IsNullOrEmpty(wantedRoles))
                    {
                        foreach (string s in wantedRoles.Split(','))
                        {
                            if (allowedRoles.Contains(s))
                            {
                                await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, s);
                            }
                        }
                    }

                    StringValues testNoLog;
                    HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                    if (!string.IsNullOrEmpty(testNoLog) && testNoLog == "True")
                    {
                        return StatusCode(202);
                    }

                    StringValues baseEndpoint;
                    HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                    return !await _jsonApiManualService.SendGenericEmail(model.Email, "eb3c00b6-ed30-414c-b908-dd542b8db87d", tenant, baseEndpoint)
                        ? StatusCode(428)
                        : StatusCode(202);
                }
            }

            if (model.ContactEmail.IsNullOrEmpty())
            {
                model.ContactEmail = model.Email;
            }

            StringValues deviceType;
            if (setupType == "app")
            {
                HttpContext.Request.Headers.TryGetValue("appPlatform", out deviceType);
            }
            else
            {
                deviceType = setupType;
            }

            dynamic jsonObject = JsonConvert.DeserializeObject(setup.registrationFields);
            dynamic campi = jsonObject.registration;

            foreach (dynamic kvp in campi)
            {
                int parsed;

                try
                {
                    if (kvp.Value.Type == JTokenType.String)
                    {
                        Int32.TryParse(kvp.Value.ToString(), out parsed);
                    }
                    else
                    {
                        Int32.TryParse(kvp.Value.field.ToString(), out parsed);
                    }
                }
                catch
                {
                    continue;
                }

                if (parsed == 2)
                {
                    string propertyName = kvp.Name.Substring(0, 1).ToUpper() + kvp.Name.Substring(1);
                    if (propertyName == "Email") { continue; }
                    else if (propertyName == "TaxId")
                    {
                        if (!IsValidCodiceFiscale(model.TaxId))
                        {
                            return StatusCode(416);
                        }
                    }
                    object propertyValue = model.GetType().GetProperty(propertyName).GetValue(model);

                    if (propertyValue == null || (propertyValue.GetType() == typeof(string) && propertyValue.ToString().IsNullOrEmpty()))
                    {
                        return StatusCode(406);
                    }
                }
            }

            //lo metto non confermato perche gli mandero un otp
            MITApplicationUser user = new() { UserName = model.Email, Email = model.Email, PasswordLastChange = DateTime.UtcNow, TenantId = tenant, EmailConfirmed = false };


            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                User userJsonapi = await _jsonApiManualService.GetUserByEmail(user.UserName);
                userJsonapi.IsPasswordMd5 = true;
                await _jsonApiManualService.UpdateAsync<User, string>(userJsonapi);
                await _jsonApiManualService.CreatePasswordHistory(user.Id, user.Email, model.Password, DateTime.Now);

                UserProfile userProfile = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    BirthCity = model.BirthCity,
                    BirthDate = model.BirthDate,
                    BirthProvince = model.BirthProvince,
                    BirthState = model.BirthState,
                    BirthZIP = model.BirthZIP,
                    ContactEmail = model.ContactEmail,
                    Description = model.Description,
                    FirstName = model.FirstName,
                    FixedPhone = model.FixedPhone,
                    LastName = model.LastName,
                    MobilePhone = model.MobilePhone,
                    NickName = model.NickName,
                    Occupation = model.Occupation,
                    ProfileFreeFieldBoolean = model.ProfileFreeFieldBoolean,
                    ProfileFreeFieldDateTime = model.ProfileFreeFieldDateTime,
                    ProfileFreeFieldInt1 = model.ProfileFreeFieldInt1,
                    ProfileFreeFieldInt2 = model.ProfileFreeFieldInt2,
                    ProfileFreeFieldString1 = model.ProfileFreeFieldString1,
                    ProfileFreeFieldString2 = model.ProfileFreeFieldString2,
                    ProfileFreeFieldString3 = model.ProfileFreeFieldString3,
                    ProfileImageId = model.ProfileImageId,
                    ResidenceAddress = model.ResidenceAddress,
                    ResidenceCity = model.ResidenceCity,
                    ResidenceHouseNumber = model.ResidenceHouseNumber,
                    ResidenceProvince = model.ResidenceProvince,
                    ResidenceState = model.ResidenceState,
                    ResidenceZIP = model.ResidenceZIP,
                    Sex = model.Sex,
                    TaxId = model.TaxId,
                    UserId = user.Id,
                    AppleRefreshToken = model.AppleRefreshToken,
                    FacebookRefreshToken = model.FacebookRefreshToken,
                    GoogleRefreshToken = model.GoogleRefreshToken,
                    TwitterRefreshToken = model.TwitterRefreshToken,
                    registrationDate = model.registrationDate,
                    termsAccepted = model.termsAccepted,
                    cookieAccepted = model.cookieAccepted,
                    termsAcceptanceDate = model.termsAcceptanceDate,
                    UserLang = model.UserLang

                };
                await _jsonApiManualService.CreateAsync<UserProfile, string>(userProfile);

                await _signInManager.SignInAsync(user, false);
                _logger.LogInformation(3, "User created a new account with password.");

                StringValues deviceName;
                HttpContext.Request.Headers.TryGetValue("devicename", out deviceName);

                UserDevice ud = new()
                {
                    deviceHash = model.FingerPrint,
                    userId = user.Id,
                    salt = await _jsonApiManualService.NewSalt(),
                    lastAccess = DateTime.UtcNow,
                    Platform = deviceType,
                    DeviceName = deviceName
                };
                await _jsonApiManualService.CreateUserDevice(ud);

                await _jsonApiManualService.RegisterUserTenant(user.Id, tenant, HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                await _jsonApiManualService.RegisterUserRole(user.Id, tenant, "User");

                StringValues baseEndpoint;
                HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                Otp otp = await _jsonApiManualService.GenerateNewOtp(user.Id, user.Email, tenant);

                HttpContext.Request.Headers.TryGetValue("platform", out setupType);

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                    ? Ok(new
                    {
                        success = true,
                        data = new User
                        {
                            Id = user.Id,
                        }
                    })
                    : !await _jsonApiManualService.SendOtpEmail(model.Email, otp.OtpSended, setupType, tenant, baseEndpoint, "0a16c97f-b76e-4042-802e-7c72d6cc337c")
                    ? StatusCode(428)
                    : Ok(new
                    {
                        success = true,
                        data = new User
                        {
                            Id = user.Id,
                        }
                    });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        //[Authorize(Policy = "NeededRoleLevel0")]
        [AllowAnonymous]
        [Route("account/registerOtp")]
        public async Task<IActionResult> RegisterOtp([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           
            StringValues setupType;
            StringValues wantedRolesSV;
            HttpContext.Request.Headers.TryGetValue("platform", out setupType);
            HttpContext.Request.Headers.TryGetValue("wantedRoles", out wantedRolesSV);

            string wantedRoles = wantedRolesSV.ToString();

            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == setupType.ToString());

            if (!setup.publicRegistration)
            {
                return StatusCode(423);
            }
            else if (model.FirstName.Length < 2 || model.LastName.Length < 2)
            {
                return StatusCode(411);
            }
            else if (model.termsAccepted == false)
            {
                return StatusCode(412);
            }

            // In email c'è l'otp per non dover fare un altro modello
            Otp otp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == model.Email && x.UserId == "registering");

            if (otp == null)
            {
                return NotFound("Otp not found");
            }
            else if (!otp.IsValid || (DateTime.UtcNow - otp.CreationDate) > TimeSpan.FromMinutes(setup.mailTokenExpiresIn))
            {
                otp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

                return StatusCode(406);
            }

            otp.IsValid = false;
            await _jsonApiManualService.UpdateAsync<Otp, string>(otp);

            int tenant = model.TenantId == null ? 1 : (int)model.TenantId;

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            User existingUser = await _jsonApiManualService.GetUserByEmail(otp.OtpValue);
            List<String> allowedRoles = new List<String>() { "Cliente", "Modella", "Agency" };
            if (existingUser != null)
            {
                UserTenant existingAssociation = _jsonApiManualService.GetUserTenant(existingUser.Id, tenant);

                if (existingAssociation != null)
                {
                    if (existingAssociation.State == "denied" || existingAssociation.State == "pending")
                    {
                        existingAssociation.State = "selfCreated";
                        existingAssociation.AcceptedAt = DateTime.UtcNow;
                        await _jsonApiManualService.UpdateUserTenant(existingAssociation);
                        await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, "User");
                        if (!String.IsNullOrEmpty(wantedRoles))
                        {
                            foreach (string s in wantedRoles.Split(','))
                            {
                                if (allowedRoles.Contains(s))
                                {
                                    await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, s);
                                }
                            }
                        }

                        StringValues testNoLog;
                        HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                        return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                            ? StatusCode(202)
                            : !await _jsonApiManualService.SendGenericEmail(otp.OtpValue, "eb3c00b6-ed30-414c-b908-dd542b8db87d", tenant, baseEndpoint)
                            ? StatusCode(428)
                            : StatusCode(202);
                    }

                    return StatusCode(409);
                }
                else
                {
                    await _jsonApiManualService.RegisterUserTenant(existingUser.Id, tenant, HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                    await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, "User");
                    if (!String.IsNullOrEmpty(wantedRoles))
                    {
                        foreach (string s in wantedRoles.Split(','))
                        {
                            if (allowedRoles.Contains(s))
                            {
                                await _jsonApiManualService.RegisterUserRole(existingUser.Id, tenant, s);
                            }
                        }
                    }

                    StringValues testNoLog;
                    HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                    return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                        ? StatusCode(202)
                        : !await _jsonApiManualService.SendGenericEmail(otp.OtpValue, "eb3c00b6-ed30-414c-b908-dd542b8db87d", tenant, baseEndpoint)
                        ? StatusCode(428)
                        : StatusCode(202);
                }
            }

            if (model.ContactEmail.IsNullOrEmpty())
            {
                model.ContactEmail = otp.OtpValue;
            }

            StringValues deviceType;
            if (setupType == "app")
            {
                HttpContext.Request.Headers.TryGetValue("appPlatform", out deviceType);
            }
            else
            {
                deviceType = setupType;
            }

            dynamic jsonObject = JsonConvert.DeserializeObject(setup.registrationFields);
            Dictionary<string, int> campi = jsonObject.registration.ToObject<Dictionary<string, int>>();

            foreach (KeyValuePair<string, int> kvp in campi)
            {
                if (kvp.Value == 2)
                {
                    string propertyName = kvp.Key.Substring(0, 1).ToUpper() + kvp.Key.Substring(1);
                    if (propertyName == "Email") { continue; }
                    else if (propertyName == "TaxId")
                    {
                        if (!IsValidCodiceFiscale(model.TaxId))
                        {
                            return StatusCode(416);
                        }
                    }
                    object propertyValue = model.GetType().GetProperty(propertyName).GetValue(model);

                    if (propertyValue == null || (propertyValue.GetType() == typeof(string) && propertyValue.ToString().IsNullOrEmpty()))
                    {
                        return StatusCode(422);
                    }
                }
            }

            MITApplicationUser user = new() { UserName = otp.OtpValue, Email = otp.OtpValue, PasswordLastChange = DateTime.UtcNow, TenantId = tenant, EmailConfirmed = true };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                User userJsonapi = await _jsonApiManualService.GetUserByEmail(user.UserName);
                userJsonapi.IsPasswordMd5 = true;
                await _jsonApiManualService.UpdateAsync<User, string>(userJsonapi);
                await _jsonApiManualService.CreatePasswordHistory(user.Id, user.Email, model.Password, DateTime.Now);

                UserProfile userProfile = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    BirthCity = model.BirthCity,
                    BirthDate = model.BirthDate,
                    BirthProvince = model.BirthProvince,
                    BirthState = model.BirthState,
                    BirthZIP = model.BirthZIP,
                    ContactEmail = model.ContactEmail,
                    Description = model.Description,
                    FirstName = model.FirstName,
                    FixedPhone = model.FixedPhone,
                    LastName = model.LastName,
                    MobilePhone = model.MobilePhone,
                    NickName = model.NickName,
                    Occupation = model.Occupation,
                    ProfileFreeFieldBoolean = model.ProfileFreeFieldBoolean,
                    ProfileFreeFieldDateTime = model.ProfileFreeFieldDateTime,
                    ProfileFreeFieldInt1 = model.ProfileFreeFieldInt1,
                    ProfileFreeFieldInt2 = model.ProfileFreeFieldInt2,
                    ProfileFreeFieldString1 = model.ProfileFreeFieldString1,
                    ProfileFreeFieldString2 = model.ProfileFreeFieldString2,
                    ProfileFreeFieldString3 = model.ProfileFreeFieldString3,
                    ProfileImageId = model.ProfileImageId,
                    ResidenceAddress = model.ResidenceAddress,
                    ResidenceCity = model.ResidenceCity,
                    ResidenceHouseNumber = model.ResidenceHouseNumber,
                    ResidenceProvince = model.ResidenceProvince,
                    ResidenceState = model.ResidenceState,
                    ResidenceZIP = model.ResidenceZIP,
                    Sex = model.Sex,
                    TaxId = model.TaxId,
                    UserId = user.Id,
                    AppleRefreshToken = model.AppleRefreshToken,
                    FacebookRefreshToken = model.FacebookRefreshToken,
                    GoogleRefreshToken = model.GoogleRefreshToken,
                    TwitterRefreshToken = model.TwitterRefreshToken,
                    registrationDate = model.registrationDate,
                    termsAccepted = model.termsAccepted,
                    cookieAccepted = model.cookieAccepted,
                    termsAcceptanceDate = model.termsAcceptanceDate,
                    UserLang = model.UserLang

                };
                await _jsonApiManualService.CreateAsync<UserProfile, string>(userProfile);

                await _signInManager.SignInAsync(user, false);
                _logger.LogInformation(3, "User created a new account with password.");

                StringValues deviceName;
                HttpContext.Request.Headers.TryGetValue("devicename", out deviceName);

                UserDevice ud = new()
                {
                    deviceHash = model.FingerPrint,
                    userId = user.Id,
                    salt = await _jsonApiManualService.NewSalt(),
                    lastAccess = DateTime.UtcNow,
                    Platform = deviceType,
                    DeviceName = deviceName
                };
                await _jsonApiManualService.CreateUserDevice(ud);

                await _jsonApiManualService.RegisterUserTenant(user.Id, tenant, HttpContext.Connection.RemoteIpAddress.ToString(), "selfCreated");
                await _jsonApiManualService.RegisterUserRole(user.Id, tenant, "User");

                ThirdPartsToken thirdParts = await _jsonApiManualService.GetExistingThirdPartAssociation(user.Email);

                thirdParts.UserId = user.Id;

                await _jsonApiManualService.UpdateAsync<ThirdPartsToken, string>(thirdParts);

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                return !string.IsNullOrEmpty(testNoLog) && testNoLog == "True"
                    ? Ok(new { success = true, data = model })
                    : !await _jsonApiManualService.SendGenericEmail(otp.OtpValue, "4bb9814c-b2ed-4159-891f-46dd700905f3", tenant, baseEndpoint)
                    ? StatusCode(428)
                    : Ok(new { success = true, data = model });
            }

            return BadRequest(result.Errors);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("getOtpUnitTest/{id}/{code}")]
        public async Task<IActionResult> OtpUnitTest(string id, string code)
        {
            try
            {
                if (code != "b0d6aa61-5e91-4ef3-8532-4c72c5011ee6")
                {
                    return Forbid();
                }

                MITApplicationUser user = await _userManager.FindByIdAsync(id);
                Otp otp = _jsonApiManualService.GetAll<Otp, string>().OrderByDescending(x => x.CreationDate).FirstOrDefault(x => x.UserId == user.Id && x.IsValid);
                string otpValue = otp.OtpSended;
                return StatusCode(200, otpValue);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("account/reset-password")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.SecurityToken))
            {

                MITApplicationUser user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    string otp = await _userManager.GeneratePasswordResetTokenAsync(user);

                    Otp newOtp = await _jsonApiManualService.GenerateNewOtp(user.Id, otp, user.TenantId);

                    StringValues setupType;
                    HttpContext.Request.Headers.TryGetValue("platform", out setupType);

                    StringValues baseEndpoint;
                    HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                    StringValues testNoLog;
                    HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                    if (!string.IsNullOrEmpty(testNoLog) && testNoLog == "True")
                    {
                        return StatusCode(204);
                    }

                    if (!await _jsonApiManualService.SendOtpEmail(model.Username, newOtp.OtpSended, setupType, 1, baseEndpoint, "499e95f2-27f2-4e14-b1ee-99c8aefcb6a2"))
                    {
                        return StatusCode(428);
                    }
                }
                else
                {
                    return StatusCode(404);
                }
            }
            else
            {
                return StatusCode(400);
            }

            return StatusCode(204);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("account/reset-password-otp")]
        public async Task<IActionResult> ResetPasswordOtp([FromBody] ResetPasswordOtpModel model)
        {
            try
            {
                MITApplicationUser user = await _userManager.FindByEmailAsync(model.email);
                User userJsonapi = await _jsonApiManualService.GetUserByEmail(model.email);

                if (user == null)
                {
                    return StatusCode(400);
                }

                //controllo che la nuova password non sia stata gia usata(se configurato e nell'intervallo configurato)
                Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
                if (setup.previousPasswordsStored > 0)
                {

                    List<PasswordHistory> oldpasswordList = await _jsonApiManualService.GetUserPasswordHistory(user.Id, setup.previousPasswordsStored);
                    if (oldpasswordList.Select(o => o.PasswordHash).ToList().Contains(model.md5Password))
                    {
                        return StatusCode(409, JsonConvert.DeserializeObject(@"{ ""Error"" : ""Unable to change the password beacause is included in the last '" + setup.previousPasswordsStored + @"' passwords used."" }"));
                    }
                }

                Otp existingOtp = _jsonApiManualService.FirstOrDefault<Otp, string>(x => x.OtpSended == model.Otp);

                if (existingOtp == null)
                {
                    return NotFound();
                }
                if (!existingOtp.IsValid || (DateTime.UtcNow - existingOtp.CreationDate > TimeSpan.FromMinutes(setup.mailTokenExpiresIn)))
                {
                    existingOtp.IsValid = false;
                    await _jsonApiManualService.UpdateAsync<Otp, string>(existingOtp);

                    return StatusCode(406, JsonConvert.DeserializeObject(@" { ""Error"": ""Otp not valid or not matching."" } "));
                }

                existingOtp.IsValid = false;
                await _jsonApiManualService.UpdateAsync<Otp, string>(existingOtp);

                IdentityResult res = await _userManager.ResetPasswordAsync(user, existingOtp.OtpValue, model.md5Password);
                if (!res.Succeeded)
                {
                    return StatusCode(500, "Unable to reset password");
                }
                await _jsonApiManualService.CreatePasswordHistory(user.Id, user.Email, model.md5Password, DateTime.Now);

                userJsonapi = await _jsonApiManualService.GetUserByEmail(model.email);
                userJsonapi.PasswordLastChange = DateTime.Now;
                userJsonapi.IsPasswordMd5 = true;
                await _jsonApiManualService.UpdateAsync<User, string>(userJsonapi);

                StringValues testNoLog;
                HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
                if (!(!string.IsNullOrEmpty(testNoLog) && testNoLog == "True"))
                {
                    StringValues baseEndpoint;
                    HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

                    if (!await _jsonApiManualService.SendGenericEmail(user.UserName, "5547dcaf-75de-493e-a301-cbdce43bc300", existingOtp.TenantId, baseEndpoint))
                    {
                        return StatusCode(428);
                    }
                }

                return !await _jsonApiManualService.setPasswordTryTo0(user.Id) ? StatusCode(500, "Unable to set password try to 0") : StatusCode(204);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("account-management/change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MITApplicationUser user = null;

            if (model.Username != null)
            {
                user = await _userManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Username);
                }
            }
            else
            {
                user = await _userManager.GetUserAsync(User);
            }

            if (user == null)
            {
                return NotFound();
            }


            User userJsonapi = await _jsonApiManualService.GetUserByEmail(user.UserName);

            //controllo che la nuova password non sia stata gia usata(se configurato e nell'intervallo configurato)
            Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
            if (setup.previousPasswordsStored > 0)
            {
                List<PasswordHistory> oldpasswordList = await _jsonApiManualService.GetUserPasswordHistory(user.Id, setup.previousPasswordsStored);
                if (oldpasswordList.Select(o => o.PasswordHash).ToList().Contains(await _jsonApiManualService.CalculateMD5Hash(model.NewPassword)))
                {
                    return StatusCode(409, JsonConvert.DeserializeObject("{ \"Error\": \"Unable to change the password beacause is included in the last '" + setup.previousPasswordsStored + "' passwords used.\" }"));
                }
            }


            string passwordToUse = model.OldPassword;
            if (setup.useMD5)
            {
                //se me la passa in md5 controllo che sia in md5 anche quella dell'utente
                //se lo é ok, altrimenti deve fare il reset
                if (!(bool)userJsonapi.IsPasswordMd5)
                {
                    //todo trovare un codice adatto
                    return StatusCode(403);
                }
            }
            else
            {
                //se me la passa in chiaro, se nel db é gia in md5 trasformo in md5 quella che mi arriva per confrontalra, altrimenti ok
                if ((bool)userJsonapi.IsPasswordMd5)
                {
                    passwordToUse = await _jsonApiManualService.CalculateMD5Hash(model.OldPassword);
                }
            }
            IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordToUse, setup.useMD5 ? model.NewPassword : await _jsonApiManualService.CalculateMD5Hash(model.NewPassword));

            //scrivo nelle password hisotry la nuova password

            if (changePasswordResult.Succeeded)
            {
                await _jsonApiManualService.CreatePasswordHistory(user.Id, user.Email, setup.useMD5 ? model.NewPassword : await _jsonApiManualService.CalculateMD5Hash(model.NewPassword), DateTime.Now);
            }
            else
            {
                return StatusCode(403);
            }

            userJsonapi = await _jsonApiManualService.GetUserByEmail(model.Username);
            userJsonapi.PasswordLastChange = DateTime.Now;
            userJsonapi.IsPasswordMd5 = true;
            userJsonapi.AccessFailedCount = 0;
            await _jsonApiManualService.UpdateAsync<User, string>(userJsonapi);


            //AGGIORNARE LA DATA DI PASSWORD LAST CHANGE
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User changed password successfully.");



            StringValues deviceHash;
            HttpContext.Request.Headers.TryGetValue("fingerPrint", out deviceHash);

            LoginViewModel newLogin = new()
            {
                Email = user.Email,
                FingerPrint = deviceHash,
                Password = model.NewPassword
            };

            return await Login(newLogin);
        }

        [HttpGet]
        //[Authorize(Policy = "NeededRoleLevel20")]
        [Route("account/list")]
        public IActionResult Get()
        {
            return Ok(new { success = true, data = _userManager.Users });
        }

        [HttpGet]
        [Route("account/{userId}")]
        public IActionResult Detail(string userId)
        {
            return Ok(new { success = true, data = _userManager.FindByIdAsync(userId) });
        }

        [HttpGet]
        //[Authorize(Policy = "NeededRoleLevel20")]
        // FASE 8A: Lookup() endpoint removed - IAppService no longer exists

        [HttpGet]
        [AllowAnonymous]
        [Route("account/google")]

        public async Task<IActionResult> Google(string code = "", string scope = "", string authuser = "", string hd = "", string prompt = "", string error = "")
        {
            if (error != "")
            {
                Setup webSetup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");

                GoogleInfos googleInfos = await _googleService.GetGoogleInfos(webSetup);

                return Redirect(googleInfos.RedirectAfterGoogleError);
            }

            return Redirect(await _googleService.GetGoogleCredentials(code));
        }

        [HttpGet]
        [Route("account/googleAccessIsValid")]
        public async Task<IActionResult> GoogleAccessIsValid()
        {
            string jwtToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            JwtSecurityToken jwtValues = new(jwtToken);

            string userEmail = jwtValues.Claims.FirstOrDefault(x => x.Type == "username").Value;

            User user = await _jsonApiManualService.GetUserByEmail(userEmail);

            ThirdPartsToken thirdPartsToken = _jsonApiManualService.GetAllQueryable<ThirdPartsToken, string>()
                .FirstOrDefault(x => x.UserId == user.Id && x.AccessType == "google" && x.RefreshToken != null);

            if (thirdPartsToken == null)
            {
                return Ok(false);
            }

            string accessToken = await _googleService.GetAccessTokenByRefreshToken(thirdPartsToken.RefreshToken);

            if (string.IsNullOrEmpty(accessToken))
            {
                return Ok(false);
            }

            thirdPartsToken.AccessToken = accessToken;

            ThirdPartsToken saved = _jsonApiManualService.Update<ThirdPartsToken, string>(thirdPartsToken);

            return saved == null ? Ok(false) : Ok(true);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/googleLoginByApp")]
        public async Task<IActionResult> GoogleLoginByAppTokenId([FromQuery] string code, [FromQuery] string userId)
        {
            Setup webSetup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");

            string redirectUri = Request.Headers["baseEndpoint"].ToString() + "/oauth2callback";
            string verifierCode = "random_long_string_43chars";

            GoogleInfos googleInfos = await _googleService.GetGoogleInfos(webSetup);

            string userEmail = (await _jsonApiManualService.GetUserById(userId)).Email;

            string credentialsMessage = await _googleService.GetGoogleCredentials(code, true, userEmail, redirectUri, verifierCode);

            if (credentialsMessage.StartsWith(googleInfos.RedirectAfterGoogleError))
            {
                return StatusCode(500, $"Impossibile recuperare le credenziali per l'utente. redirectUri: {redirectUri}, verifierCode: {verifierCode}");
            }
            else if (credentialsMessage == "wrongEmail")
            {
                return StatusCode(406, "L'email Google non appartiene all'utente in sessione.");
            }

            return NoContent();
        }

    }
}
