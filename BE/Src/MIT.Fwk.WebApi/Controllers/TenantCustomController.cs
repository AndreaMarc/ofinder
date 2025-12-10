using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    public class TenantCustomController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;

        public TenantCustomController(
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

        public class TenantHierarchy
        {
            public int Id { get; set; }


            public String Name { get; set; }


            public String Description { get; set; }


            public String Organization { get; set; }


            public bool Enabled { get; set; }
            public bool isErasable { get; set; }
            public bool isRecovery { get; set; }
            public List<TenantHierarchy> Child { get; set; } = [];
        }

        private List<TenantHierarchy> CreateTenantHierarchy(List<Tenant> tenants)
        {
            Dictionary<int, TenantHierarchy> dict = [];
            List<TenantHierarchy> roots = [];

            // Prima aggiungi tutti i tenant al dizionario
            foreach (Tenant tenant in tenants)
            {
                if (!dict.ContainsKey(tenant.Id))
                {
                    dict[tenant.Id] = new TenantHierarchy { Id = tenant.Id, Name = tenant.Name, Description = tenant.Description, Organization = tenant.Organization, Enabled = tenant.Enabled, isErasable = tenant.isErasable, isRecovery = tenant.isRecovery };
                }
            }

            // Poi costruisci la gerarchia
            foreach (Tenant tenant in tenants)
            {
                if (tenant.ParentTenant != 0)
                {
                    if (dict.ContainsKey(tenant.ParentTenant))
                    {
                        dict[tenant.ParentTenant].Child.Add(dict[tenant.Id]);
                    }
                }
                else
                {
                    roots.Add(dict[tenant.Id]);
                }
            }

            return roots;
        }

        [HttpGet]
        [Route("tenants/getTree")]
        public async Task<IActionResult> AddVoice(CancellationToken cancellationToken)
        {
            List<Tenant> tenants = await _jsonService.GetAllAsync<Tenant, int>();
            return Ok(CreateTenantHierarchy(tenants));

        }




    }
}

