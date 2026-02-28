using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Availability verification for performers
    /// </summary>
    [Resource]
    [Table("AvailabilityVerifications")]
    public class AvailabilityVerification : Identifiable<string>
    {
        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        public DateTime? From { get; set; }

        [Attr]
        public DateTime? To { get; set; }

        [Attr]
        public bool Chosen { get; set; } = false;

        [Attr]
        public bool Canceled { get; set; } = false;

        [Attr]
        public bool Missing { get; set; } = false;

        // Navigation Properties
        [HasOne]
        public virtual Performer Performer { get; set; }
    }
}
