using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Pricing tiers and subscription fees for a channel
    /// 1:1 relationship with Channel
    /// </summary>
    [Resource]
    [Table("ChannelPricing")]
    public class ChannelPricing : Identifiable<int>
    {
        [Attr]
        [Required]
        public string ChannelId { get; set; }

        // Monthly Subscription Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlySubscriptionFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MonthlySubscriptionTo { get; set; }

        // Photo Sale Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PhotoSaleFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PhotoSaleTo { get; set; }

        // Video Sale Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? VideoSaleFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? VideoSaleTo { get; set; }

        // Live Public Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LivePublicFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LivePublicTo { get; set; }

        // Live Private Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LivePrivateFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LivePrivateTo { get; set; }

        // Clothing Sales Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClothingSalesFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClothingSalesTo { get; set; }

        // Extra Content Range
        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExtraContentFrom { get; set; }

        [Attr]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExtraContentTo { get; set; }

        [Attr]
        public string Note { get; set; }

        // Timestamps
        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual Channel Channel { get; set; }
    }
}
