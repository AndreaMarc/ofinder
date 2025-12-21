using Microsoft.AspNetCore.Identity;

namespace MIT.Fwk.Infrastructure.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class MITApplicationRole : IdentityRole<string>
    {
        public int TenantId { get; set; }

        public short Level { get; set; }

        public bool? Needful { get; set; }

        public bool CopyInNewTenants { get; set; }

        public string Typology { get; set; }

        public string Initials { get; set; }
    }
}
