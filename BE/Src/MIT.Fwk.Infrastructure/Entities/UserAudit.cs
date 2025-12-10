using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("AspNetUserAudit")]
    public class UserAudit : Identifiable<int>
    {

        [Attr]
        public DateTimeOffset Timestamp { get; set; }

        [Attr]
        public short AuditEvent { get; set; }

        [Attr]
        public string IpAddress { get; set; }

        [Attr]
        public string UserId { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
