using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public class MITUserManager : UserManager<MITApplicationUser>
    {
        protected readonly IServiceProvider _services;
        private readonly JsonApiDbContext _db;
        protected readonly INotificationHandler<DomainNotification> _notifications;
        public MITUserManager(IUserStore<MITApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<MITApplicationUser> passwordHasher, IEnumerable<IUserValidator<MITApplicationUser>> userValidators, IEnumerable<IPasswordValidator<MITApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<MITApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _services = services;
            _notifications = services.GetRequiredService<INotificationHandler<DomainNotification>>();
            _db = services.GetRequiredService<JsonApiDbContext>();
        }

        public override async Task<MITApplicationUser> GetUserAsync(ClaimsPrincipal principal)
        {
            RoleManager<MITApplicationRole> roleManager = _services.GetRequiredService<RoleManager<MITApplicationRole>>();

            MITApplicationUser user = await base.GetUserAsync(principal);
            IList<string> userRoles = await base.GetRolesAsync(user);
            user.Roles = roleManager.Roles.Where(r => userRoles.Contains(r.Name)).ToList();
            return user;
        }

        //public override async Task<bool> CheckPasswordAsync(MITApplicationUser user, string password)
        //{
        //    var result = await base.CheckPasswordAsync(user, password);

        //    if (result)
        //    {


        //        return true;
        //    }
        //    else
        //    {
        //        //await _notifications.Handle(new DomainNotification(DomainNotification.UNAUTHORIZED, "Invalid password"), default(CancellationToken));
        //        return false;
        //    }
        //}

        public override async Task<IdentityResult> ChangePasswordAsync(MITApplicationUser user, string currentPassword, string newPassword)
        {
            if (newPassword.Equals(currentPassword))
            {
                await _notifications.Handle(new DomainNotification(DomainNotification.NOT_ACCEPTABLE, "New password must be different"), default(CancellationToken));
                return IdentityResult.Failed();
            }

            IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                user.LastAccess = DateTime.UtcNow;
                user.PasswordLastChange = DateTime.UtcNow;
                await UpdateAsync(user);
            }
            else
            {
                if (result.Errors != null && result.Errors.Count() > 0)
                {
                    await _notifications.Handle(new DomainNotification(DomainNotification.UNAUTHORIZED, result.Errors.FirstOrDefault().Description), default(CancellationToken));
                }
                else
                {
                    await _notifications.Handle(new DomainNotification(DomainNotification.UNAUTHORIZED, "Login failed."), default(CancellationToken));
                }
            }

            return result;
        }

        public override async Task<IdentityResult> AddToRoleAsync(MITApplicationUser user, string role)
        {
            Role roleToAdd = _db.Roles.FirstOrDefault(r => r.Name.ToUpper() == role.ToUpper());

            if (roleToAdd != null)
            {
                //_db.UserRoles.Add(new UserRole() { UserId = user.Id, RoleId = roleToAdd.Id, TenantId = user.TenantId });
                await _db.UserRoles.AddAsync(new UserRole() { UserId = user.Id, RoleId = roleToAdd.Id, TenantId = user.TenantId });

                if (await _db.SaveChangesAsync() > 0)
                {
                    return IdentityResult.Success;
                }
            }

            return IdentityResult.Failed();
        }

        public async Task<IdentityResult> AddToRoleAsync(MITApplicationUser user, string role, int tenant)
        {
            Role roleToAdd = _db.Roles.FirstOrDefault(r => r.Name.ToUpper() == role.ToUpper());

            if (roleToAdd != null)
            {
                //_db.UserRoles.Add(new UserRole() { UserId = user.Id, RoleId = roleToAdd.Id, TenantId = tenant });
                await _db.UserRoles.AddAsync(new UserRole() { UserId = user.Id, RoleId = roleToAdd.Id, TenantId = tenant });

                if (await _db.SaveChangesAsync() > 0)
                {
                    return IdentityResult.Success;
                }
            }

            return IdentityResult.Failed();
        }

        //public override async Task<MITApplicationUser> FindByNameAsync(string userName)
        //{
        //    if (ConfigurationHelper.SqlProvider == "MySql")
        //    {
        //        //base.Database.BeginTransaction();
        //        LogHelper.Info($"MITUserManager.FindByNameAsync on MySql provider...");

        //        var user = _db.Users
        //                   .Where(u => u.UserName.ToLower() == userName.ToLower()).FirstOrDefault();

        //        if (user != null)
        //            return user;
        //        else
        //            return null;
        //    }
        //    else
        //    {
        //        return await base.FindByNameAsync(userName);
        //    }
        //}

        //public override async Task<MITApplicationUser> FindByEmailAsync(string email)
        //{
        //    if (ConfigurationHelper.SqlProvider == "MySql")
        //    {
        //        //base.Database.BeginTransaction();
        //        LogHelper.Info($"MITUserManager.FindByEmailAsync on MySql provider...");

        //        var user = _db.Users
        //                   .Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();

        //        if (user != null)
        //            return user;
        //        else
        //            return null;
        //    }
        //    else
        //    {
        //        return await base.FindByEmailAsync(email);
        //    }
        //}
    }

}
