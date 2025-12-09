using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("Otps")]
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class Otp : Identifiable<string>
    {
        [Attr]
        public string UserId { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [Attr]
        public string OtpValue { get; set; }

        [Attr]
        public string OtpSended { get; set; }

        [Attr]
        public DateTimeOffset? CreationDate { get; set; }

        [Attr]
        public bool IsValid { get; set; }

        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual Tenant Tenant { get; set; }
    }
}
