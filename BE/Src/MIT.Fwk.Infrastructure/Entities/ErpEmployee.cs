using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpEmployee")]
    public class ErpEmployee : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string UserId { get; set; }
        [Attr] public bool IsActive { get; set; } = true;
        [Attr] public DateTime HiredDate { get; set; }
        [Attr] public DateTime? TerminatedDate { get; set; }
        [Attr] public string ContractType { get; set; } = "full-time";
        [Attr] public string EmployeeType { get; set; } = "internal";
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual User User { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
