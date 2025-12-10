using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class TemplateController : JsonApiController<Template, String>
    {
        private readonly IResourceService<Template, String> _resourceService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public TemplateController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Template, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
        }

        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public override async Task<IActionResult> GetAsync(String id, CancellationToken cancellationToken)
        {
            StringValues tenantId;
            HttpContext.Request.Headers.TryGetValue("tenantId", out tenantId);

            List<Category> categories = await _jsonService.GetAllCategoriesByTenantId(Int32.Parse(tenantId));
            List<Template> templates = await _jsonService.GetAllTemplatesByCategories(categories);

            foreach (Template template in templates)
            {
                if (template.Id == id)
                {
                    return await base.GetAsync(id, cancellationToken);
                }
            }

            return Unauthorized();
        }

    }
}

