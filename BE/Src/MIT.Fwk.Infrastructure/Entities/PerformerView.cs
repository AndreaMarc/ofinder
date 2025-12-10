using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Tracks performer profile views by users
    /// UNIQUE constraint on (PerformerId, UserId) - one view record per user per performer
    /// Updated on subsequent views
    /// </summary>
    [Resource]
    [Table("PerformerViews")]
    public class PerformerView : Identifiable<string>
    {
        [Attr]
        [Required]
        public string PerformerId { get; set; }

        [Attr]
        [Required]
        public string UserId { get; set; }

        [Attr]
        public DateTime ViewedAt { get; set; }

        // Navigation Properties
        [HasOne]
        public virtual Performer Performer { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
