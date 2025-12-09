using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    /// <summary>
    /// Custom service for Performer-specific queries and operations.
    /// Auto-discovered by framework via I*ManualService pattern.
    /// Marker interface: IFwkCustomService enables auto-registration.
    /// </summary>
    public interface IPerformerManualService : IFwkCustomService
    {
        /// <summary>
        /// Get top-rated performers with aggregated statistics
        /// </summary>
        /// <param name="limit">Number of top performers to return</param>
        /// <param name="minReviews">Minimum number of reviews required</param>
        Task<List<PerformerWithStatsDTO>> GetTopRatedPerformersAsync(int limit = 10, int minReviews = 1);

        /// <summary>
        /// Get channel statistics for a specific performer
        /// </summary>
        Task<ChannelStatsDTO> GetChannelStatsAsync(string performerId);

        /// <summary>
        /// Get comprehensive dashboard data for a performer
        /// </summary>
        Task<PerformerDashboardDTO> GetPerformerDashboardAsync(string performerId);

        /// <summary>
        /// Track performer profile view (updates timestamp if already viewed)
        /// </summary>
        Task<bool> TrackPerformerViewAsync(string performerId, string userId);

        /// <summary>
        /// Add/remove favorite performer for user (toggle)
        /// </summary>
        Task<bool> ToggleFavoriteAsync(string userId, string performerId);

        /// <summary>
        /// Check if user has already reviewed a performer
        /// </summary>
        Task<bool> HasUserReviewedAsync(string performerId, string userId);
    }
}
