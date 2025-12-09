using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Types of content published on a channel
    /// (e.g., "photo galleries", "live shows", "private content", etc.)
    /// </summary>
    [Resource]
    [Table("ChannelContentTypes")]
    public class ChannelContentType : Identifiable<int>
    {
        [Attr]
        [Required]
        public string ChannelId { get; set; }

        [Attr]
        [Required]
        [MaxLength(40)]
        public string ContentType { get; set; }

        [Attr]
        public string Description { get; set; }

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
