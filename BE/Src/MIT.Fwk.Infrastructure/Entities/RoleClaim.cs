using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("AspNetRoleClaims")]
    public class RoleClaim : Identifiable<int>
    {
        [Attr]
        public string ClaimType { get; set; }

        [Attr]
        public string ClaimValue { get; set; }

        [Attr]
        public string RoleId { get; set; }

        [HasOne]
        public virtual Role Role { get; set; }
    }
}
