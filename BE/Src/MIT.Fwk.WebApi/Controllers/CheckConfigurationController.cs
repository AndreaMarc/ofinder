using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using String = System.String;
using MIT.Fwk.Core.Attributes;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    [SkipClaimsValidation]
    public class CheckConfigurationController : ControllerBase
    {
        private readonly IJsonApiManualService _jsonService;
        private readonly IConfiguration _configuration;

        public CheckConfigurationController(
        IJsonApiManualService jsonService,
        IConfiguration configuration,
        UserManager<MITApplicationUser> userManager,
        RoleManager<MITApplicationRole> roleManager,
        ILoggerFactory loggerFactory,
        IEmailSender email,
        ISmsSender sms)
        {
            _jsonService = jsonService;
            _configuration = configuration;
        }

        public class reportResponse
        {
            public String Language { get; set; }
            public String ErrorType { get; set; }
            public String Name { get; set; }

            public String Id { get; set; }

        }




        [HttpGet]
        [Route("checkConfiguration")]
        public async Task<IActionResult> Get()
        {

            List<reportResponse> result = [];

            List<Tenant> recoveryTenants = _jsonService.Where<Tenant, int>(x => x.isRecovery).ToList();

            if (recoveryTenants.Count < 1)
            {
                result.Add(new reportResponse { ErrorType = "RecoveryTenantNotFound", Language = "it", Name = "", Id = "" });
            }
            else if (recoveryTenants.Count > 1)
            {
                result.Add(new reportResponse { ErrorType = "MultipleRecoveryTenant", Language = "it", Name = "", Id = "" });
            }

            List<Setup> setups = await _jsonService.GetAllAsync<Setup, int>();
            List<CustomSetup> setupsCustom = await _jsonService.GetAllAsync<CustomSetup, string>();

            Setup web = setups.FirstOrDefault(x => x.environment == "web");
            CustomSetup webCustom = setupsCustom.FirstOrDefault(x => x.Environment == "web");
            Setup app = setups.FirstOrDefault(x => x.environment == "app");
            CustomSetup appCustom = setupsCustom.FirstOrDefault(x => x.Environment == "app");

            if (web.maintenance || webCustom.MaintenanceAdmin)
            {
                result.Add(new reportResponse { ErrorType = "MaintenanceWeb", Language = "it", Name = "", Id = "" });
            }

            if (app.maintenance || appCustom.MaintenanceAdmin)
            {
                result.Add(new reportResponse { ErrorType = "MaintenanceApp", Language = "it", Name = "", Id = "" });
            }

            // Deserializzazione della stringa JSON in una lista di oggetti
            List<dynamic> objects = JsonConvert.DeserializeObject<List<dynamic>>(web.availableLanguages);
            // Selezione dei valori di "code"
            List<string> supportedLanguages = objects.Where(y => y.active).Select(o => (string)o.code).ToList();
            //mo lo faccio diventa un oggetto e lo ciclo

            List<Template> alltemplates = await _jsonService.GetAllSystemTemplates();
            //var alltemplates = await _jsonService.GetAllBasicTemplates();
            List<string> alltemplatesType = alltemplates.Select(o => o.Code).Distinct().ToList();
            foreach (string templateType in alltemplatesType)
            {

                foreach (string language in supportedLanguages)
                {
                    Template temp = alltemplates.FirstOrDefault(x => x.Code == templateType && x.Language == language);
                    if (temp == null || temp.Id == "")
                    {
                        result.Add(new reportResponse { ErrorType = "TemplateNotFound", Language = language, Name = alltemplates.FirstOrDefault(x => x.Code == templateType).Name, Id = alltemplates.FirstOrDefault(x => x.Code == templateType).Id });

                    }
                    else
                    {

                        if ((temp.Content == null || temp.Content.Length < 1) && !temp.Active)
                        {
                            result.Add(new reportResponse { ErrorType = "TemplateNoContentNotActive", Language = language, Name = temp.Name, Id = temp.Id });
                        }
                        else
                        {
                            if (temp.Content == null || temp.Content.Length < 1)
                            {
                                result.Add(new reportResponse { ErrorType = "TemplateNoContent", Language = language, Name = temp.Name, Id = temp.Id });
                            }

                            if (!temp.Active)
                            {
                                result.Add(new reportResponse { ErrorType = "TemplateNotActive", Language = language, Name = temp.Name, Id = temp.Id });
                            }
                        }

                    }

                }

            }

            List<Translation> translations = await _jsonService.GetAllAsync<Translation, int>();

            List<LegalTerm> allLegalTerms = await _jsonService.GetAllAsync<LegalTerm, string>();

            List<string> allLegalTermsType = _configuration["LegalTermsCodes"].Split(",").ToList();
            foreach (string legalTermType in allLegalTermsType)
            {
                List<LegalTerm> allLanguages = allLegalTerms.FindAll(x => x.Code == legalTermType && x.Active);

                IEnumerable<IGrouping<string, LegalTerm>> groups = allLanguages.GroupBy(x => x.Version);

                JObject jsonObject = JObject.Parse(translations.FirstOrDefault(x => x.languageCode == "it").translationWeb);
                string typeName = "";
                try
                {
                    typeName = jsonObject["other"]["utilsIncompleteConfig"][legalTermType].ToString();
                }
                catch { }

                if (groups.Count() > 1)
                {
                    result.Add(new reportResponse { ErrorType = "MismatchLegalTermsVersion", Language = "", Name = typeName, Id = legalTermType });
                }

                foreach (string language in supportedLanguages)
                {
                    jsonObject = JObject.Parse(translations.FirstOrDefault(x => x.languageCode == language).translationWeb);
                    typeName = "";
                    try
                    {
                        typeName = jsonObject["other"]["utilsIncompleteConfig"][legalTermType].ToString();
                    }
                    catch { }

                    bool isAtLeastOneActive = false;
                    int activeCounter = 0;

                    List<LegalTerm> sameType = allLegalTerms.FindAll(x => x.Code == legalTermType && x.Language == language);

                    if (sameType.Count == 0)
                    {
                        result.Add(new reportResponse { ErrorType = "LegalTermNotFound", Language = language, Name = typeName, Id = legalTermType });
                    }

                    foreach (LegalTerm temp in sameType)
                    {
                        if (temp.Active)
                        {
                            activeCounter++;

                            if (!isAtLeastOneActive)
                            {
                                isAtLeastOneActive = true;
                            }
                        }

                        if (temp.Active && temp.Content == null || temp.Content.Length < 1)
                        {
                            result.Add(new reportResponse { ErrorType = "LegalTermActiveNoContent", Language = language, Name = typeName, Id = temp.Id });
                        }

                    }

                    if (!isAtLeastOneActive)
                    {
                        result.Add(new reportResponse { ErrorType = "NoOneLegalTermActive", Language = language, Name = typeName, Id = legalTermType });
                    }

                    if (activeCounter > 1)
                    {
                        result.Add(new reportResponse { ErrorType = "MoreThanOneLegalTermActive", Language = language, Name = typeName, Id = legalTermType });
                    }

                }

            }

            return StatusCode(200, result);
        }



    }
}

