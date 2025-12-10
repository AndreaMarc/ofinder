using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("AspNetUserClaims")]
    public class AspNetUserClaim : Identifiable<int>
    {
        [Attr]
        public string ClaimType { get; set; }

        [Attr]
        public string ClaimValue { get; set; }

        [Attr]
        public string UserId { get; set; }
    }
}
