using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("BlockNotifications")]
    [Resource]
    public class BlockNotification : Identifiable<string>
    {
        [Attr]
        public string UserId { get; set; }
        [Attr]
        public int TenantId { get; set; }
        [Attr]
        public DateTime? PushBlock { get; set; }
        [Attr]
        public DateTime? EmailBlock { get; set; }

        [HasOne] public virtual User User { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
