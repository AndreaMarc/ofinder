using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Infrastructure.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public class RoleLevelRequirement : IAuthorizationRequirement
    {
        public int Level { get; }

        public RoleLevelRequirement(int level)
        {
            Level = level;
        }
    }

    public class RoleLevelHandler : AuthorizationHandler<RoleLevelRequirement>
    {
        readonly IHttpContextAccessor _httpContextAccessor = null;
        private readonly UserManager<MITApplicationUser> _userManager;

        public RoleLevelHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            _userManager = httpContext.RequestServices.GetRequiredService<UserManager<MITApplicationUser>>();
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                            RoleLevelRequirement requirement)
        {

            MITApplicationUser user = await _userManager.GetUserAsync(context.User);

            if (user?.Roles?.Any(r => r.Level <= requirement.Level) == true)
            {
                context.Succeed(requirement);
            }
        }
    }
}
