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

        // Personal Information
        [Attr]
        public string Email { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string NickName { get; set; }

        [Attr]
        public string MobilePhone { get; set; }

        [Attr]
        public string Sex { get; set; }

        [Attr]
        public DateTime? BirthDate { get; set; }

        [Attr]
        public string BirthState { get; set; }

        [Attr]
        public string ResidenceState { get; set; }

        [Attr]
        public string ResidenceRegion { get; set; }

        [Attr]
        public string ResidenceProvince { get; set; }

        [Attr]
        public string Occupation { get; set; }

        // Performer-specific fields
        [Attr]
        public string Description { get; set; }

        [Attr]
        public bool IsActive { get; set; } = true;

        [Attr]
        public bool Active { get; set; } = false;

        [Attr]
        public bool IsVerified { get; set; } = false;

        [Attr]
        public DateTime? Verified { get; set; }

        // Timestamps
        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual User User { get; set; }

        

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

        [HasMany]
        public virtual ICollection<AvailabilityVerification> AvailabilityVerifications { get; set; }
    }
}
