using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    public class TemplateCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public TemplateCustomController(
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

        [HttpPost]
        [Route("templates/getOrCreate")]
        public async Task<IActionResult> AddVoice([FromBody] Template template)
        {
            StringValues tenantId;
            HttpContext.Request.Headers.TryGetValue("tenantId", out tenantId);

            Template existing = await _jsonService.getTemplateByCodeAndLanguage(template.Code, template.Language, int.Parse(tenantId));

            if (existing == null)
            {
                template.Id = Guid.NewGuid().ToString();
                await _jsonService.CreateAsync<Template, string>(template);
                return StatusCode(201, template);
            }

            // Per problemi di ridondanza su JsonApiDotNetCore

            Template result = new()
            {
                Id = existing.Id,
                Language = existing.Language,
                Active = existing.Active,
                CategoryId = existing.CategoryId,
                Code = existing.Code,
                Content = existing.Content,
                ContentNoHtml = existing.ContentNoHtml,
                CopyInNewTenants = existing.CopyInNewTenants,
                Description = existing.Description,
                Erasable = existing.Erasable,
                Name = existing.Name,
                LocalId = existing.LocalId,
                ObjectText = existing.ObjectText,
                StringId = existing.StringId,
                Tags = existing.Tags,
                Erased = existing.Erased,
                Order = existing.Order,
                FeaturedImage = existing.FeaturedImage,
                FreeField = existing.FreeField,
            };

            return StatusCode(200, result);

        }
    }
}

