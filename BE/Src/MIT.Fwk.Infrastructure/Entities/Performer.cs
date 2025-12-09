using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Main performer profile entity
    /// Extends User with performer-specific data
    /// NOTE: [Resource] removed - using custom controller in WebApi
    /// </summary>
    [Table("Performers")]
    public class Performer : Identifiable<string>
    {
        // Foreign Keys
        [Attr]
        [Required]
        public string UserId { get; set; }

        [Attr]
        public int? GeoCountryId { get; set; }

        [Attr]
        public int? GeoFirstDivisionId { get; set; }

        [Attr]
        public int? GeoSecondDivisionId { get; set; }

        // Performer-specific fields
        [Attr]
        [MaxLength(500)]
        public string Description { get; set; }

        [Attr]
        public bool IsActive { get; set; } = true;

        [Attr]
        public bool IsVerified { get; set; } = false;

        // Timestamps
        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual GeoCountry GeoCountry { get; set; }

        [HasOne]
        public virtual GeoFirstDivision GeoFirstDivision { get; set; }

        [HasOne]
        public virtual GeoSecondDivision GeoSecondDivision { get; set; }

        [HasMany]
        public virtual ICollection<Channel> Channels { get; set; }

        [HasMany]
        public virtual ICollection<PerformerReview> PerformerReviews { get; set; }

        [HasMany]
        public virtual ICollection<PerformerService> PerformerServices { get; set; }

        [HasMany]
        public virtual ICollection<PerformerView> PerformerViews { get; set; }

        [HasMany]
        public virtual ICollection<UserFavorite> UserFavorites { get; set; }
    }
}
