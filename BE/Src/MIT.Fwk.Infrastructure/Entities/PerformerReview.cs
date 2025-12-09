using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// User ratings and reviews for performers
    /// UNIQUE constraint on (PerformerId, UserId) - one review per user per performer
    /// NOTE: [Resource] removed - using custom controller in WebApi
    /// </summary>
    [Table("PerformerReviews")]
    public class PerformerReview : Identifiable<string>
    {
        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        [Required]
        public string UserId { get; set; }

        [Attr]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Attr]
        public string ReviewText { get; set; }

        [Attr]
        public bool IsVerifiedPurchase { get; set; } = false;

        // Timestamps
        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual Performer Performer { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
