using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpEmployeeRole")]
    public class ErpEmployeeRole : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string EmployeeId { get; set; }
        [Attr] public string RoleId { get; set; }
        [Attr] public string ManagerId { get; set; }
        [Attr] public DateTime StartDate { get; set; }
        [Attr] public DateTime? EndDate { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual ErpEmployee Employee { get; set; }
        [HasOne] public virtual ErpRole Role { get; set; }
        [HasOne] public virtual ErpEmployee Manager { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
