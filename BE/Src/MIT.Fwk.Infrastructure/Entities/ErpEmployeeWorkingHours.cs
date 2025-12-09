using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("ErpEmployeeWorkingHours")]
    public class ErpEmployeeWorkingHours : Identifiable<string>
    {
        [Attr] public string ErpEmployeeId { get; set; }
        [Attr] public int TenantId { get; set; }
        [Attr] public int Day { get; set; }
        [Attr] public TimeSpan? StartTime { get; set; }
        [Attr] public TimeSpan? EndTime { get; set; }
        [Attr] public int? StartFlexibility { get; set; }
        [Attr] public int? EndFlexibility { get; set; }
        [Attr] public int? MinimumBreakDuration { get; set; }
        [Attr] public int? MaximumBreakDuration { get; set; }
        [Attr] public int? DailyWorkingTime { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual User ErpEmployee { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
