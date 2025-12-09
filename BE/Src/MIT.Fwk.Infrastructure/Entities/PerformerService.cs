using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Optional services offered by performers
    /// (e.g., modeling, escort services, photography, etc.)
    /// </summary>
    [Resource]
    [Table("PerformerServices")]
    public class PerformerService : Identifiable<string>
    {
        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        [Required]
        public ServiceType ServiceType { get; set; }

        [Attr]
        [MaxLength(400)]
        public string Link { get; set; }

        [Attr]
        public string Description { get; set; }

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
    }
}
