using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Extension
{
    public class ClaimsRequirement : IAuthorizationRequirement
    {
        public string ClaimName { get; }

        public ClaimsRequirement(string claimName)
        {
            ClaimName = claimName;
        }
    }

    public class ClaimsHandler : AuthorizationHandler<ClaimsRequirement>
    {
        readonly IHttpContextAccessor _httpContextAccessor = null;
        private readonly IJsonApiManualService _jsonService;
        private readonly UserManager<MITApplicationUser> _userManager;

        public ClaimsHandler(IHttpContextAccessor httpContextAccessor, IJsonApiManualService jsonService)
        {
            _httpContextAccessor = httpContextAccessor;
            _jsonService = jsonService;
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            _userManager = httpContext.RequestServices.GetRequiredService<UserManager<MITApplicationUser>>();
        }

        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       ClaimsRequirement requirement)
        {
            ClaimsIdentity identity = (ClaimsIdentity)context.User.Identity;
            string userEmail = identity.Claims.First(x => x.Type == "username").Value;
            User user = await _jsonService.GetUserByEmail(userEmail);
            if (await _jsonService.CheckIsSuperadmin(user.Id))
            {
                context.Succeed(requirement);
            }
            else if (await _jsonService.CheckClaimsById(user.Id, user.TenantId.ToString(), [requirement.ClaimName]))
            {
                context.Succeed(requirement);
            }



            return Task.CompletedTask;
        }
    }
}
