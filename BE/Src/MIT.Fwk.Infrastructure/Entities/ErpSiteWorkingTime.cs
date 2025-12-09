using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("ErpSiteWorkingTime")]
    public class ErpSiteWorkingTime : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string SiteId { get; set; }
        [Attr] public string ShiftId { get; set; }
        [Attr] public int Day { get; set; }
        [Attr] public int StartFlexibility { get; set; }
        [Attr] public int EndFlexibility { get; set; }
        [Attr] public int MinimumBreakDuration { get; set; }
        [Attr] public int MaximumBreakDuration { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual ErpSite Site { get; set; }
        [HasOne] public virtual ErpShift Shift { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
