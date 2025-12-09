using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class UserPreferencesController : JsonApiController<UserPreference, String>
    {
        private readonly IResourceService<UserPreference, String> _resourceService;
        private readonly IResourceService<User, String> _userService;
        private readonly IResourceService<UserTenant, String> _userTenantService;
        private readonly IResourceService<Setup, int> _setupService;
        private readonly IJsonApiManualService _jsonApiManualService;
        protected readonly UserManager<MITApplicationUser> _userManager;
        protected readonly IDocumentService _docService;

        [ActivatorUtilitiesConstructor]
        public UserPreferencesController(IDocumentService docService
            , IJsonApiManualService jsonApiManualService
            , IResourceService<User, String> userService
            , IResourceService<UserTenant, String> userTenantService
            , IResourceService<Setup, int> setupService
            , UserManager<MITApplicationUser> userManager
            , IJsonApiOptions options, IResourceGraph resourceGraph
            , ILoggerFactory loggerFactory
            , IResourceService<UserPreference, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _userTenantService = userTenantService;
            _userService = userService;
            _setupService = setupService;
            _userManager = userManager;
            _jsonApiManualService = jsonApiManualService;
        }


    }
}

