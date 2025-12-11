using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Social media or platform channel managed by a performer
    /// </summary>
    [Resource]
    [Table("Channels")]
    public class Channel : Identifiable<string>
    {
        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        [Required]
        public PlatformType Platform { get; set; }

        [Attr]
        [Required]
        public ChannelType ChannelType { get; set; }

        [Attr]
        [MaxLength(50)]
        public string UsernameHandle { get; set; }

        [Attr]
        [MaxLength(150)]
        public string ProfileLink { get; set; }

        [Attr]
        public string Note { get; set; }

        [Attr]
        public bool IsActive { get; set; } = true;

        // Timestamps
        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual Performer Performer { get; set; }

        [HasOne]
        public virtual ChannelPricing ChannelPricing { get; set; }

        [HasMany]
        public virtual ICollection<ChannelSchedule> ChannelSchedules { get; set; }

        [HasMany]
        public virtual ICollection<ChannelContentType> ChannelContentTypes { get; set; }
    }
}
