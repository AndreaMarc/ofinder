using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("AspNetUserRoles")]
    public class UserRole : Identifiable<string>
    {


        [Attr]
        public string RoleId { get; set; }
        [Attr]
        public string UserId { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual Role Role { get; set; }

        [HasOne]
        public virtual Tenant Tenant { get; set; }
    }
}
