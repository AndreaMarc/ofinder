using System;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Models
{
    /// <summary>
    /// DTO for top-rated performers with aggregated stats
    /// Used by GetTopRatedPerformersAsync endpoint
    /// </summary>
    public class PerformerWithStatsDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public bool IsVerified { get; set; }
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalViews { get; set; }
        public int TotalFavorites { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for channel statistics
    /// Used by GetChannelStatsAsync endpoint
    /// </summary>
    public class ChannelStatsDTO
    {
        public string PerformerId { get; set; }
        public int TotalChannels { get; set; }
        public List<PlatformChannelDTO> ChannelsByPlatform { get; set; }
        public int TotalContentTypes { get; set; }
        public PricingRangesDTO PricingRanges { get; set; }
    }

    /// <summary>
    /// Channels grouped by platform
    /// </summary>
    public class PlatformChannelDTO
    {
        public string Platform { get; set; }
        public int Count { get; set; }
        public List<string> UsernameHandles { get; set; }
    }

    /// <summary>
    /// Aggregated pricing ranges across all channels
    /// </summary>
    public class PricingRangesDTO
    {
        public decimal? MonthlySubscriptionMin { get; set; }
        public decimal? MonthlySubscriptionMax { get; set; }
        public decimal? PhotoSaleMin { get; set; }
        public decimal? PhotoSaleMax { get; set; }
        public decimal? VideoSaleMin { get; set; }
        public decimal? VideoSaleMax { get; set; }
        public decimal? LivePublicMin { get; set; }
        public decimal? LivePublicMax { get; set; }
        public decimal? LivePrivateMin { get; set; }
        public decimal? LivePrivateMax { get; set; }
    }

    /// <summary>
    /// DTO for performer dashboard (comprehensive stats)
    /// Used by GetPerformerDashboardAsync endpoint
    /// </summary>
    public class PerformerDashboardDTO
    {
        public string PerformerId { get; set; }
        public int TotalViews { get; set; }
        public int TotalReviews { get; set; }
        public double AvgRating { get; set; }
        public int TotalFavorites { get; set; }
        public int TotalChannels { get; set; }
        public int TotalServices { get; set; }
        public DateTime MemberSince { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
