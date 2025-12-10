using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("UserTenants")]
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    [SkipClaimsValidation(JwtHttpMethod.GET)]
    public class UserTenant : Identifiable<string>
    {

        [Attr]
        public string UserId { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [Attr]
        public DateTime? CreatedAt { get; set; }

        [Attr]
        public string Ip { get; set; }

        [Attr]
        public string State { get; set; }

        [Attr]
        public DateTime? AcceptedAt { get; set; }

        [HasOne]
        public virtual User User { get; set; }


        [HasOne]
        public virtual Tenant Tenant { get; set; }
    }
}
