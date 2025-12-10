using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    /// <summary>
    /// Activity schedule for a channel (live hours, streaming times, etc.)
    /// </summary>
    [Resource]
    [Table("ChannelSchedules")]
    public class ChannelSchedule : Identifiable<int>
    {
        [Attr]
        [Required]
        public string ChannelId { get; set; }

        [Attr]
        [Range(0, 6)] // 0=Sunday, 6=Saturday
        public int DayOfWeek { get; set; }

        [Attr]
        public TimeSpan StartTime { get; set; }

        [Attr]
        public TimeSpan EndTime { get; set; }

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
