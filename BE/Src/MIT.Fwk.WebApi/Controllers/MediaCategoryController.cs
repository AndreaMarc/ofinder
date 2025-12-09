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
    public class MediaCategoryController : JsonApiController<MediaCategory, String>
    {
        private readonly IResourceService<MediaCategory, String> _resourceService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public MediaCategoryController(IJsonApiManualService jsonManualService, IDocumentService docService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<MediaCategory, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(String id, CancellationToken cancellationToken)
        {
            MediaCategory category = (await _resourceService.GetAsync(cancellationToken)).ToList().FirstOrDefault(x => x.Id == id);

            if (category == null)
            {
                return StatusCode(404);
            }
            else if (!category.Erasable)
            {
                return StatusCode(405);
            }

            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;
            string alternativeCategory;

            if (!queryString.ContainsKey("alternativeCategory"))
            {
                return StatusCode(400);
            }

            alternativeCategory = queryString["alternativeCategory"].ToString();

            if (alternativeCategory == "")
            {
                await _jsonService.DeleteMediaCategoryRecursively(id, _docService);
            }
            else
            {
                MediaCategory newCategory = (await _resourceService.GetAsync(cancellationToken)).ToList().FirstOrDefault(x => x.Id == alternativeCategory);

                if (newCategory == null)
                {
                    return StatusCode(406);
                }
                else if (newCategory.Type != category.Type)
                {
                    return StatusCode(403);
                }

                await _jsonService.MoveAllToOtherMediaCategory(id, alternativeCategory);
            }

            return await base.DeleteAsync(id, cancellationToken);
        }
    }
}

