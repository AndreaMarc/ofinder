using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    /// <summary>
    /// Controller for Performer management with custom endpoints.
    /// Extends JsonApiController for auto-generated CRUD + custom actions.
    /// </summary>
    public class PerformersController : JsonApiController<Performer, string>
    {
        private readonly IPerformerManualService _performerManualService;
        private readonly UserManager<MITApplicationUser> _userManager;

        [ActivatorUtilitiesConstructor]
        public PerformersController(
            IPerformerManualService performerManualService,
            UserManager<MITApplicationUser> userManager,
            IJsonApiOptions options,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<Performer, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _performerManualService = performerManualService;
            _userManager = userManager;
        }

        /// <summary>
        /// GET /api/v2/performers/top-rated
        /// Returns top-rated performers with aggregated stats.
        /// PUBLIC endpoint - no authentication required.
        /// </summary>
        [HttpGet("top-rated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopRated([FromQuery] int limit = 10, [FromQuery] int minReviews = 1)
        {
            try
            {
                var topPerformers = await _performerManualService.GetTopRatedPerformersAsync(limit, minReviews);
                return Ok(topPerformers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v2/performers/{id}/channel-stats
        /// Returns channel statistics for a specific performer.
        /// PUBLIC endpoint - no authentication required.
        /// </summary>
        [HttpGet("{id}/channel-stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetChannelStats(string id)
        {
            try
            {
                var stats = await _performerManualService.GetChannelStatsAsync(id);

                if (stats == null)
                    return NotFound(new { error = $"Performer with ID {id} not found or has no channels" });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/v2/performers/{id}/dashboard
        /// Returns comprehensive dashboard data for a performer.
        /// PUBLIC endpoint - no authentication required.
        /// </summary>
        [HttpGet("{id}/dashboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDashboard(string id)
        {
            try
            {
                var dashboard = await _performerManualService.GetPerformerDashboardAsync(id);

                if (dashboard == null)
                    return NotFound(new { error = $"Performer with ID {id} not found" });

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/v2/performers/{id}/track-view
        /// Tracks a user viewing a performer profile.
        /// AUTHENTICATED endpoint - requires valid JWT.
        /// </summary>
        [HttpPost("{id}/track-view")]
        [Authorize]
        public async Task<IActionResult> TrackView(string id)
        {
            try
            {
                // Get current user from JWT claims
                var username = Request.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "username")?.Value;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { error = "User not authenticated" });

                var user = await _userManager.FindByEmailAsync(username)
                        ?? await _userManager.FindByNameAsync(username);

                if (user == null)
                    return Unauthorized(new { error = "User not found" });

                var success = await _performerManualService.TrackPerformerViewAsync(id, user.Id);

                return success
                    ? Ok(new { message = "View tracked successfully" })
                    : StatusCode(500, new { error = "Failed to track view" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /api/v2/performers/{id}/toggle-favorite
        /// Adds or removes performer from user's favorites.
        /// AUTHENTICATED endpoint - requires valid JWT.
        /// </summary>
        [HttpPost("{id}/toggle-favorite")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(string id)
        {
            try
            {
                var username = Request.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "username")?.Value;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { error = "User not authenticated" });

                var user = await _userManager.FindByEmailAsync(username)
                        ?? await _userManager.FindByNameAsync(username);

                if (user == null)
                    return Unauthorized(new { error = "User not found" });

                var success = await _performerManualService.ToggleFavoriteAsync(user.Id, id);

                return success
                    ? Ok(new { message = "Favorite toggled successfully" })
                    : StatusCode(500, new { error = "Failed to toggle favorite" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Override DELETE to require admin role.
        /// Only admins (NeededRoleLevel10) can delete performers.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "NeededRoleLevel10")]
        public override async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return await base.DeleteAsync(id, cancellationToken);
        }
    }
}
