using Microsoft.AspNetCore.Identity;

namespace MIT.Fwk.Infrastructure.Entities
{
    public class MITApplicationUserRole : IdentityUserRole<string>
    {
        public int TenantId { get; set; }
    }

}
