using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpShift")]
    public class ErpShift : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string Name { get; set; }
        [Attr] public string Description { get; set; }
        [Attr] public TimeSpan StartTime { get; set; }
        [Attr] public int StandardWorkingTime { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
