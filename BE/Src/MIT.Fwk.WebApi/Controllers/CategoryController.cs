
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class CategoryController : JsonApiController<Category, int>
    {
        private readonly IResourceService<Category, int> _resourceService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public CategoryController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Category, int> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] Category resource, CancellationToken cancellationToken)
        {
            return await base.PostAsync(resource, cancellationToken);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            if ((await _resourceService.GetAsync(cancellationToken)).ToList().FindAll(x => x.ParentCategory == id).Count > 0)
            {
                return StatusCode(403);
            }

            Category category = (await _resourceService.GetAsync(cancellationToken)).ToList().FirstOrDefault(x => x.Id == id);

            if (category == null)
            {
                return StatusCode(404);
            }
            else if (!category.Erasable)
            {
                return StatusCode(405);
            }

            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;
            int alternativeCategory;

            if (!queryString.ContainsKey("alternativeCategory"))
            {
                return StatusCode(400);
            }

            alternativeCategory = Int32.Parse(queryString["alternativeCategory"].ToString());

            if (alternativeCategory == 0)
            {
                await _jsonService.DeleteAllTemplatesByCategoryId(id);
            }
            else
            {
                category = (await _resourceService.GetAsync(cancellationToken)).ToList().FirstOrDefault(x => x.Id == alternativeCategory);

                if (category == null)
                {
                    return StatusCode(406);
                }

                await _jsonService.MoveAllTemplatesByCategoryId(id, alternativeCategory);
            }

            return await base.DeleteAsync(id, cancellationToken);
        }

    }
}

