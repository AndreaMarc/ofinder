using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Linq;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    public class UsersCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;
        protected readonly UserManager<MITApplicationUser> _userManager;

        public UsersCustomController(
        IJsonApiManualService jsonService,
        IResourceService<Translation, int> translationService,
        IResourceService<Setup, int> setupService,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationUser> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
            _userManager = userManager;
        }

        [HttpDelete]
        [Route("deleteSelf")]
        public async Task<IActionResult> DeleteSelf()
        {
            // Get current user from request
            string username = Request.HttpContext.User.Claims.ToList().Find(x => x.Type == "username").Value;
            MITApplicationUser currentUser = await _userManager.FindByEmailAsync(username);
            if (currentUser == null)
            {
                currentUser = await _userManager.FindByNameAsync(username);
            }

            Setup setup = _jsonService.FirstOrDefault<Setup, int>(x => x.environment == "web");
            if (setup.logicDelete)
            {
                await _jsonService.DeleteUserLite(currentUser.Id);
            }
            else
            {
                await _jsonService.DeleteUser(currentUser.Id);
            }

            return StatusCode(200, true);

        }
    }
}

