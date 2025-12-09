using JsonApiDotNetCore.Services;
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
    [SkipClaimsValidation(JwtHttpMethod.GET)]
    public class CategoryCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public CategoryCustomController(
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

        public class CategoryHierarchy
        {
            public string Id { get; set; }

            public String ElementType { get; set; }

            public String Name { get; set; }

            public String Description { get; set; }

            public String Type { get; set; }

            public bool Erasable { get; set; }
            public bool? Erased { get; set; }

            public bool? Active { get; set; }

            public String Code { get; set; }

            public String Language { get; set; }

            public int Order { get; set; }

            public List<CategoryHierarchy> Child { get { return _child.OrderBy(x => x.ElementType).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList(); } }

            private readonly List<CategoryHierarchy> _child = [];

            public void AddChild(CategoryHierarchy child)
            {
                _child.Add(child);
            }
        }

        private List<CategoryHierarchy> CreateCategoryHierarchy(List<Category> categories)
        {
            Dictionary<int, CategoryHierarchy> dict = [];
            List<CategoryHierarchy> roots = [];

            // Prima aggiungi tutte le categorie al dizionario
            foreach (Category category in categories)
            {
                if (!dict.ContainsKey(category.Id))
                {
                    dict[category.Id] = new CategoryHierarchy
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        Description = category.Description,
                        Type = category.Type,
                        Erasable = category.Erasable,
                        Order = category.Order,
                        ElementType = "category"
                    };
                }
            }

            // Poi costruisci la gerarchia
            foreach (Category category in categories)
            {
                if (category.ParentCategory != 0)
                {
                    if (dict.ContainsKey(category.ParentCategory))
                    {
                        dict[category.ParentCategory].AddChild(dict[category.Id]);
                    }
                    else
                    {
                        roots.Add(dict[category.Id]);
                    }
                }
                else
                {
                    roots.Add(dict[category.Id]);
                }
            }

            return roots.OrderBy(x => x.ElementType).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList();
        }

        private List<CategoryHierarchy> AddTemplatesToHierarchy(List<Template> templates, List<Category> categories)
        {
            Dictionary<string, CategoryHierarchy> dict = [];
            List<CategoryHierarchy> roots = [];

            // Prima aggiungi tutte le categorie al dizionario
            foreach (Category category in categories)
            {
                if (!dict.ContainsKey("c" + category.Id))
                {
                    dict["c" + category.Id] = new CategoryHierarchy
                    {
                        Id = category.Id.ToString(),
                        Name = category.Name,
                        Description = category.Description,
                        Type = category.Type,
                        Erasable = category.Erasable,
                        Order = category.Order,
                        ElementType = "category"
                    };
                }
            }

            // Poi aggiungi tutti i template al dizionario
            foreach (Template template in templates)
            {
                if (!dict.ContainsKey("t" + template.Id))
                {
                    dict["t" + template.Id] = new CategoryHierarchy
                    {
                        Id = template.Id,
                        Name = template.Name,
                        Description = template.Description,
                        Erasable = template.Erasable,
                        Active = template.Active,
                        Code = template.Code,
                        Language = template.Language,
                        Erased = template.Erased,
                        Order = template.Order,
                        ElementType = "template"
                    };
                }
            }

            // Poi costruisci la gerarchia
            foreach (Category category in categories)
            {
                if (dict.ContainsKey("c" + category.ParentCategory))
                {
                    dict["c" + category.ParentCategory].AddChild(dict["c" + category.Id]);
                }
                else
                {
                    roots.Add(dict["c" + category.Id]);
                }
            }

            foreach (Template template in templates)
            {
                if (dict.ContainsKey("c" + template.CategoryId))
                {
                    dict["c" + template.CategoryId].AddChild(dict["t" + template.Id]);
                }
            }

            return roots.OrderBy(x => x.ElementType).ThenBy(x => x.Order).ThenBy(x => x.Name).ToList();
        }

        [HttpGet]
        [Route("categories/getTree")]
        public async Task<IActionResult> AddVoice(int tenantId)
        {
            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;
            int tenant;

            if (!queryString.ContainsKey("tenantId"))
            {
                return StatusCode(400, "Tenant missing.");
            }

            tenant = Int32.Parse(queryString["tenantId"].ToString());

            List<Category> categories = await _jsonService.GetAllCategoriesByTenantId(tenant);

            return Ok(CreateCategoryHierarchy(categories));

        }

        [HttpGet]
        [Route("categories/getFullTree")]
        public async Task<IActionResult> GetFullTree(int tenantId)
        {
            Microsoft.AspNetCore.Http.IQueryCollection queryString = HttpContext.Request.Query;
            int tenant;
            string categoryIds = "";
            string language;

            if (!queryString.ContainsKey("tenantId") || !queryString.ContainsKey("language"))
            {
                return StatusCode(400, "Tenant or language missing.");
            }

            tenant = Int32.Parse(queryString["tenantId"].ToString());
            language = queryString["language"].ToString();

            if (queryString.ContainsKey("categoryIds"))
            {
                categoryIds = queryString["categoryIds"].ToString();
            }

            List<Category> categories = await _jsonService.GetAllCategoriesByTenantId(tenant);

            if (categoryIds != "")
            {
                categories = categories.FindAll(x => categoryIds.Split(",").Contains(x.Id.ToString()));
            }

            categories = categories.FindAll(x => x.ParentCategory == 0 || !categories.Select(y => y.Id).Contains(x.ParentCategory));

            List<Category> startingCategories = [.. categories];

            foreach (Category category in startingCategories)
            {
                categories.AddRange(await _jsonService.GetChildrenCategories(category));
            }

            categoryIds = "";

            if (categories.Count > 0)
            {
                foreach (Category c in categories)
                {
                    categoryIds += c.Id.ToString() + ",";
                }

                categoryIds = categoryIds.Substring(0, categoryIds.Length - 1);
            }

            List<Template> templates = await _jsonService.GetAllTemplatesByCategories(categoryIds);

            templates = templates.FindAll(x => x.Language == language);

            return Ok(AddTemplatesToHierarchy(templates, categories));

        }




    }
}

