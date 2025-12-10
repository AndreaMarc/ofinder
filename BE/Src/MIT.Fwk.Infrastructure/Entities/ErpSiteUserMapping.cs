using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpSiteUserMapping")]
    public class ErpSiteUserMapping : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string SiteId { get; set; }
        [Attr] public string EmployeeId { get; set; }
        [Attr] public bool IsPrimary { get; set; }
        [Attr] public DateTime MappingStartDate { get; set; }
        [Attr] public DateTime? MappingEndDate { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual ErpSite Site { get; set; }
        [HasOne] public virtual User User { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
