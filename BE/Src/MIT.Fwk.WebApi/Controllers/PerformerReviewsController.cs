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
    /// Controller for PerformerReview with validation logic.
    /// Prevents duplicate reviews from same user.
    /// </summary>
    public class PerformerReviewsController : JsonApiController<PerformerReview, string>
    {
        private readonly IPerformerManualService _performerManualService;
        private readonly UserManager<MITApplicationUser> _userManager;
        private readonly JsonApiDbContext _context;

        [ActivatorUtilitiesConstructor]
        public PerformerReviewsController(
            IPerformerManualService performerManualService,
            UserManager<MITApplicationUser> userManager,
            IJsonApiOptions options,
            IResourceGraph resourceGraph,
            ILoggerFactory loggerFactory,
            IResourceService<PerformerReview, string> resourceService,
            JsonApiDbContext context)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
            _performerManualService = performerManualService;
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Override POST to validate user hasn't already reviewed.
        /// AUTHENTICATED endpoint - requires valid JWT.
        /// </summary>
        [HttpPost]
        [Authorize]
        public override async Task<IActionResult> PostAsync([FromBody] PerformerReview review, CancellationToken cancellationToken)
        {
            try
            {
                // Get current user
                var username = Request.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == "username")?.Value;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { error = "User not authenticated" });

                var user = await _userManager.FindByEmailAsync(username)
                        ?? await _userManager.FindByNameAsync(username);

                if (user == null)
                    return Unauthorized(new { error = "User not found" });

                // Check if user already reviewed this performer (UNIQUE constraint)
                var hasReviewed = await _performerManualService.HasUserReviewedAsync(review.PerformerId, user.Id);
                if (hasReviewed)
                    return BadRequest(new { error = "You have already reviewed this performer. Only one review per user is allowed." });

                // Set UserId from authenticated user
                review.UserId = user.Id;
                review.Id = Guid.NewGuid().ToString();
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                return await base.PostAsync(review, cancellationToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Override DELETE to require admin role.
        /// Only admins can delete reviews.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "NeededRoleLevel10")]
        public override async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return await base.DeleteAsync(id, cancellationToken);
        }
    }
}
