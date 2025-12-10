using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpRole")]
    public class ErpRole : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string RoleId { get; set; }
        [Attr] public bool IsManagerial { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual Role Role { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }

}
