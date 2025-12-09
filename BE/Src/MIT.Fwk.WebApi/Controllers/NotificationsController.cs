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
using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    public class NotificationsController : JsonApiController<Notification, String>
    {
        private readonly IResourceService<Notification, String> _resourceService;
        private readonly IJsonApiManualService _jsonService;
        protected readonly IDocumentService _docService;
        protected readonly INotificationService _notificationService;

        [ActivatorUtilitiesConstructor]
        public NotificationsController(IJsonApiManualService jsonManualService, IDocumentService docService, INotificationService notificationService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Notification, String> resourceService) : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _resourceService = resourceService;
            _docService = docService;
            _jsonService = jsonManualService;
            _notificationService = notificationService;
        }

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] Notification resource, CancellationToken cancellationToken)
        {
            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            StringValues testNoLog;
            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            if (!string.IsNullOrEmpty(testNoLog) && testNoLog == "True")
            {
                return StatusCode(200);
            }

            Setup setup = _jsonService.FirstOrDefault<Setup, int>(x => x.environment == "web");

            return StatusCode(await _notificationService.SendNotification(resource, setup, baseEndpoint));
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(String id, [FromBody] Notification resource, CancellationToken cancellationToken)
        {
            StringValues testNoLog;
            HttpContext.Request.Headers.TryGetValue("unitTest", out testNoLog);
            if (!string.IsNullOrEmpty(testNoLog) && testNoLog == "True")
            {
                return StatusCode(200);
            }

            StringValues baseEndpoint;
            HttpContext.Request.Headers.TryGetValue("baseEndpoint", out baseEndpoint);

            Setup setup = _jsonService.FirstOrDefault<Setup, int>(x => x.environment == "web");

            Notification cancelBadge = new()
            {
                UsersGuidList = resource.UserId,
                SendEmail = false,
                SendPushNotification = true,
                SendWebSocket = true,
                ForceWebSocketApp = false,
                PushType = "refreshNotifications",
                OnlyData = true,
            };

            await _notificationService.SendNotification(cancelBadge, setup, baseEndpoint);

            return await base.PatchAsync(id, resource, cancellationToken);
        }
    }
}

