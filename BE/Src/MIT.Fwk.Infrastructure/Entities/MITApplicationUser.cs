using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MIT.Fwk.Infrastructure.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class MITApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get { return $"{FirstName} {LastName}"; } }

        public DateTime? LastAccess { get; set; }

        public string LastIp { get; set; }

        public int TenantId { get; set; }

        // NUOVI CAMPI BE LIKE
        //public DateTime Prova { get; set; }

        public DateTime? PasswordLastChange { get; set; }

        [NotMapped]
        public List<MITApplicationRole> Roles { get; set; }

        [NotMapped]
        public int UserLevel { get { return Roles != null && Roles.Count() > 0 ? Roles.Min(r => r.Level) : 999; } }
    }
}
