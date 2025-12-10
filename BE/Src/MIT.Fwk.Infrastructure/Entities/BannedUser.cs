using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("BannedUsers")]
    public class BannedUser : Identifiable<string>
    {
        [Attr]
        public string UserId { get; set; }

        [Attr]
        public string SupervisorId { get; set; }

        [Attr]
        public DateTimeOffset? LockStart { get; set; }

        [Attr]
        public DateTimeOffset? LockEnd { get; set; }

        [Attr]
        public bool LockActive { get; set; }

        [Attr]
        public bool CrossTenantBanned { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [Attr]
        public int LockDays { get; set; }

        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual Tenant Tenant { get; set; }
    }
}
