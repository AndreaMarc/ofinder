using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("AspNetRoles")]
    public class Role : Identifiable<string>
    {

        [Attr]
        public string ConcurrencyStamp { get; set; }

        [Attr]
        public string Name { get; set; }

        [Attr]
        public string NormalizedName { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [Attr]
        public bool? Needful { get; set; }

        [Attr]
        public bool CopyInNewTenants { get; set; }

        [Attr]
        public short Level { get; set; }

        [Attr]
        public string Typology { get; set; }

        [Attr]
        public string Initials { get; set; }

        [HasMany]
        public virtual ICollection<RoleClaim> RoleClaims { get; set; }

        [HasMany]
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
