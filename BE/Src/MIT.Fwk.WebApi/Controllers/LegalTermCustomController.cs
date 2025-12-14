using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    [Route("legalTerms")]
    public class LegalTermCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public LegalTermCustomController(
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
        }

        public class ActivationModel
        {
            public string Id { get; set; }
        }

        [HttpPatch]
        [Route("activation")]
        public async Task<IActionResult> Activation([FromBody] ActivationModel model)
        {

            await _jsonService.SetLegalTerms(model.Id);

            return Ok(new { success = true, data = true });

        }

        [HttpPost]
        [Route("getOrCreate")]
        public async Task<IActionResult> getOrCreate([FromBody] LegalTerm model)
        {

            LegalTerm existing = await _jsonService.GetLegalTermByKeyLCV(model.Language, model.Code, model.Version);

            if (existing == null)
            {
                model.Id = Guid.NewGuid().ToString();
                await _jsonService.CreateAsync<LegalTerm, string>(model);
                return StatusCode(201, new { success = true, data = model });
            }

            return Ok(new { success = true, data = existing });

        }
    }
}

