using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public static class HttpClientExtentions
    {

        public static AuthenticationHeaderValue ToAuthHeaderValue(this string username, string password)
        {
            return new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{username}:{password}")));
        }

        public static async Task<AuthenticationHeaderValue> ToJwtBearer(this HttpContext context, string username, string password, string passwordHash, int tokenExpiresIn, string salt, bool superadmin, string tenantId)
        {
            JwtSignInManager provider = new(context);
            string token = await provider.Login(username, password, passwordHash, tokenExpiresIn, salt, superadmin, tenantId);

            return new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<AuthenticationHeaderValue> ToJwtBearerRefresh(this HttpContext context, string username, string password, string passwordHash, int tokenExpiresIn, string salt, bool superadmin, string tenantId)
        {
            JwtSignInManager provider = new(context);
            string token = await provider.LoginRefresh(username, password, passwordHash, tokenExpiresIn, salt, superadmin, tenantId);

            return new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<AuthenticationHeaderValue> ToJwtExternalBearer(this HttpContext context, string username, int days)
        {
            JwtSignInManager provider = new(context);
            string token = await provider.ExternalToken(username, days);

            return new AuthenticationHeaderValue("Bearer", token);
        }
        public class LoginResultModel
        {
            public string domainNotification { get; set; }
            public string message { get; set; }
            public MITApplicationUser user { get; set; }
        }

        public static async Task<LoginResultModel> AuthenticateUser(this HttpContext context, string username, string password, IJsonApiManualService _jsonApiManualService)
        {
            SignInManager<MITApplicationUser> signInManager = context.RequestServices.GetRequiredService<SignInManager<MITApplicationUser>>();
            UserManager<MITApplicationUser> userManager = context.RequestServices.GetRequiredService<UserManager<MITApplicationUser>>();
            IMediatorHandler bus = (IMediatorHandler)context.RequestServices.GetService(typeof(IMediatorHandler));

            MITApplicationUser user = await userManager.FindByNameAsync(username);

            if (user == null)
            {
                user = await userManager.FindByEmailAsync(username);
            }

            if (user != null)
            {
                SignInResult result = await signInManager.PasswordSignInAsync(user, password, false, true);
                if (result.Succeeded)
                {
                    AuthenticationOptions authOptions = context.RequestServices.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

                    //legge i valori da setup, se non li trovo li prendo dal customsettings
                    int passwordValidityPeriod = authOptions.PasswordValidityPeriod;
                    bool checkFirstAccess = authOptions.PasswordCheckFirstAccess;

                    Setup setup = _jsonApiManualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
                    if (setup != null)
                    {
                        passwordValidityPeriod = setup.passwordExpirationPeriod;

                    }

                    if (authOptions.SpecialUsers.Contains(username))
                    {
                        context.User = await signInManager.CreateUserPrincipalAsync(user);

                        return (new LoginResultModel { domainNotification = DomainNotification.OK, message = "Login succes", user = user });
                    }

                    // passwordValidityPeriod=0 -> never expire
                    if (passwordValidityPeriod > 0 && user.PasswordLastChange.HasValue)
                    {
                        if (DateTime.Compare(DateTime.UtcNow, user.PasswordLastChange.Value.AddDays(passwordValidityPeriod)) > 0)
                        {
                            if (checkFirstAccess && DateTime.Compare(DateTime.UtcNow, user.PasswordLastChange.Value.AddYears(5)) > 0)
                            {

                                // first access
                                return (new LoginResultModel { domainNotification = DomainNotification.PRECONDITION_FAILED, message = "Primo accesso, necessario cambio password." });
                            }
                            else
                            {
                                // password expired

                                return (new LoginResultModel { domainNotification = DomainNotification.CONFLICT, message = "Password scaduta." });

                            }
                        }
                    }
                    else if (passwordValidityPeriod > 0 || !user.PasswordLastChange.HasValue || (user.PasswordLastChange <= DateTime.MinValue))
                    {
                        // first access
                        return (new LoginResultModel { domainNotification = DomainNotification.PRECONDITION_FAILED, message = "Primo accesso, necessario cambio password." });

                    }

                    context.User = await signInManager.CreateUserPrincipalAsync(user);

                    return (new LoginResultModel { domainNotification = DomainNotification.OK, message = "Login succes", user = user });


                }
                else
                {
                    return result.ToString() == "Lockedout"
                        ? new LoginResultModel { domainNotification = DomainNotification.LOCKED, message = "Account locked." }
                        : new LoginResultModel { domainNotification = DomainNotification.UNAUTHORIZED, message = "Invalid password." };

                }
            }
            else
            {
                return (new LoginResultModel { domainNotification = DomainNotification.UNAUTHORIZED, message = "Invalid username." });
            }
        }

        public static async Task<AuthenticationHeaderValue> RefreshJwtBearer(this HttpContext context, int minutes, string tenantId = "-1", string userId = "-1")
        {
            if (JwtTokenProvider.Enabled && context != null)
            {
                if (context.Request.Headers.Keys.Contains("Authorization"))
                {
                    // Extract JWT header value (replaces removed JwtAuthentication.GetJwtAuthenticationHeaderValue)
                    string authorizationHeader = context.Request.Headers["Authorization"]
                        .FirstOrDefault(header => header.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase));
                    JwtAuthenticationHeaderValue authenticationHeader = new(authorizationHeader);

                    if (authenticationHeader.IsValid)
                    {
                        JwtSignInManager authenticationManager = new(context, authenticationHeader);
                        StringValues deviceHash;
                        context.Request.Headers.TryGetValue("fingerPrint", out deviceHash);
                        if (await authenticationManager.TrySignInRefresh(deviceHash.ToString()))
                        {
                            string token = tenantId == "-1" || userId == "-1"
                                ? await authenticationManager.RefreshToken(minutes)
                                : await authenticationManager.RefreshToken(minutes, tenantId, userId);
                            return new AuthenticationHeaderValue("Bearer", token);
                        }
                    }
                }
            }

            return null;
        }

        //public static async Task SignOut(this HttpContext context)
        //{
        //    var isJwt = Convert.ToBoolean(ConfigurationHelper.Get("Jwt:Enabled"));

        //    if (isJwt)
        //    {
        //        await new JwtSignInManager(context).SignOutAsync();
        //    }
        //    else
        //    {
        //        await new BasicAuthenticationSignInManager(context, null).SignOutAsync();
        //    }
        //}
    }

}