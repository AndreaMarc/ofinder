using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Infrastructure.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Services
{
    public class AuditableSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;

        public AuditableSignInManager(UserManager<TUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<TUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<TUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<TUser> confirmation, IConfiguration configuration)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            SignInResult result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

            MITApplicationUser appUser = user as MITApplicationUser;


            bool useAudit = _configuration.GetValue("UseAudit", false);
            if (user != null && useAudit) // We can only log an audit record if we can access the user object and it's ID
            {
                /*var ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                AspNetUserAudit auditRecord = null;

                switch (result.ToString())
                {
                    case "Succeeded":
                        auditRecord = AspNetUserAudit.CreateAuditEvent(appUser.Id, UserAuditEventType.Login, ip);

                        appUser.LastAccess = DateTime.Now;
                        await _userManager.UpdateAsync(appUser as TUser);

                        break;

                    case "Failed":
                        auditRecord = AspNetUserAudit.CreateAuditEvent(appUser.Id, UserAuditEventType.FailedLogin, ip);
                        break;
				}

                if (auditRecord != null)
                {
                    _db.UserAuditEvents.Add(auditRecord);
                    //_db.Add<AspNetUserAudit>(auditRecord);

                    try
                    {
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warn($"AuditableSignInManager.PasswordSignInAsync ex: {ex.Message} st: {ex.StackTrace} ie: {ex.InnerException?.Message}");
                    }
                }*/
            }

            return result;
        }

        public override async Task SignOutAsync()
        {
            await base.SignOutAsync();

            IdentityUser user = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Claims.ToList().Find(x => x.Type == "username").Value) as IdentityUser;

            bool useAudit2 = _configuration.GetValue("UseAudit", false);

            if (user != null && useAudit2)
            {
                /*var ip = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                var auditRecord = AspNetUserAudit.CreateAuditEvent(user.Id, UserAuditEventType.LogOut, ip);
                _db.UserAuditEvents.Add(auditRecord);
                //_db.Add<AspNetUserAudit>(auditRecord);

                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Warn($"AuditableSignInManager.SignOutAsync ex: {ex.Message} st: {ex.StackTrace} ie: {ex.InnerException?.Message}");
                }*/
            }
        }
    }

}
