using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    [SkipClaimsValidation]
    public class MediaCategoryCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public MediaCategoryCustomController(
        IJsonApiManualService jsonService,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
        }

        public class CategoryHierarchy
        {
            public string Id { get; set; }

            public String ElementType { get; set; }

            public String Name { get; set; }

            public String Description { get; set; }

            public String Type { get; set; }

            public bool Erasable { get; set; }

            public String Code { get; set; }

            public int Order { get; set; }

            public List<CategoryHierarchy> Child { get { return _child.OrderBy(x => x.ElementType).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList(); } }

            private readonly List<CategoryHierarchy> _child = [];

            public void AddChild(CategoryHierarchy child)
            {
                _child.Add(child);
            }
        }

        private List<CategoryHierarchy> CreateHierarchy(List<MediaFile> files, List<MediaCategory> mediaCategories)
        {
            Dictionary<string, CategoryHierarchy> dict = [];
            List<CategoryHierarchy> roots = [];

            // Prima aggiungi tutte le typology, category e album al dizionario
            foreach (MediaCategory category in mediaCategories)
            {
                if (category.Type == "typology" && !dict.ContainsKey("t" + category.Id))
                {
                    dict["t" + category.Id] = new CategoryHierarchy
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        Description = category.Description,
                        Type = category.Type,
                        Erasable = category.Erasable,
                        Order = category.Order,
                        ElementType = "typology",
                        Code = category.Code,
                    };
                }
                else if (category.Type == "category" && !dict.ContainsKey("c" + category.Id))
                {
                    dict["c" + category.Id] = new CategoryHierarchy
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        Description = category.Description,
                        Type = category.Type,
                        Erasable = category.Erasable,
                        Order = category.Order,
                        ElementType = "category",
                        Code = category.Code,
                    };
                }
                else if (category.Type == "album" && !dict.ContainsKey("a" + category.Id))
                {
                    dict["a" + category.Id] = new CategoryHierarchy
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        Description = category.Description,
                        Type = category.Type,
                        Erasable = category.Erasable,
                        Order = category.Order,
                        ElementType = "album",
                        Code = category.Code,
                    };
                }
            }

            // Poi aggiungi tutti i files al dizionario
            foreach (MediaFile file in files)
            {
                if (!dict.ContainsKey("f" + file.Id))
                {
                    dict["f" + file.Id] = new CategoryHierarchy
                    {
                        Id = file.Id.ToString(),
                        Name = file.originalFileName,
                        Description = file.alt,
                        Erasable = true,
                        Order = 0,
                        ElementType = "file",
                        Type = file.extension
                    };
                }
            }

            // Poi costruisci la gerarchia
            foreach (MediaCategory category in mediaCategories)
            {
                MediaCategory parentCategory = mediaCategories.FirstOrDefault(x => x.Id == category.ParentMediaCategory);

                if (parentCategory != null && dict.ContainsKey(parentCategory.Type.Substring(0, 1) + category.ParentMediaCategory))
                {
                    dict[parentCategory.Type.Substring(0, 1) + category.ParentMediaCategory].AddChild(dict[category.Type.Substring(0, 1) + category.Id]);
                }
                else
                {
                    roots.Add(dict[category.Type.Substring(0, 1) + category.Id]);
                }
            }

            foreach (MediaFile file in files)
            {
                if (dict.ContainsKey("a" + file.album))
                {
                    dict["a" + file.album].AddChild(dict["f" + file.Id]);
                }
            }

            return roots.OrderBy(x => x.ElementType).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList();
        }

        [HttpGet]
        [Route("mediaCategories/getFullTree")]
        public async Task<IActionResult> GetFullTree()
        {
            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;
            int tenant;
            string categoryIds = "";

            if (!queryString.ContainsKey("tenantId"))
            {
                return StatusCode(400, "Tenant or language missing.");
            }

            tenant = Int32.Parse(queryString["tenantId"].ToString());

            if (queryString.ContainsKey("categoryIds"))
            {
                categoryIds = queryString["categoryIds"].ToString();
            }

            List<MediaCategory> mediaCategories = await _jsonService.GetAllMediaCategoriesByTenantId(tenant);

            if (categoryIds != "")
            {
                mediaCategories = mediaCategories.FindAll(x => categoryIds.Split(",").Contains(x.Id.ToString()));
            }

            mediaCategories = mediaCategories.FindAll(x => x.ParentMediaCategory == "" || !mediaCategories.Select(y => y.Id).Contains(x.ParentMediaCategory));

            List<MediaCategory> startingCategories = [.. mediaCategories];

            foreach (MediaCategory category in startingCategories)
            {
                mediaCategories.AddRange(await _jsonService.GetChildrenMediaCategories(category));
            }

            categoryIds = "";

            if (mediaCategories.Count > 0)
            {
                foreach (MediaCategory c in mediaCategories)
                {
                    categoryIds += c.Id.ToString() + ",";
                }

                categoryIds = categoryIds.Substring(0, categoryIds.Length - 1);
            }

            List<MediaFile> files = await _jsonService.FindAllMediaInMediaCategories(mediaCategories);

            return Ok(CreateHierarchy(files, mediaCategories));

        }




    }
}

