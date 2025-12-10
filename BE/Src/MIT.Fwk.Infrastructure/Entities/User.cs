using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("AspNetUsers")]
    public class User : Identifiable<string>
    {
        [Attr]
        public int AccessFailedCount { get; set; }

        [Attr]
        public string ConcurrencyStamp { get; set; }

        [Attr]
        public string Email { get; set; }

        [Attr]
        public bool EmailConfirmed { get; set; }

        [Attr]
        public bool LockoutEnabled { get; set; }

        [Attr]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Attr]
        public string NormalizedEmail { get; set; }

        [Attr]
        public string NormalizedUserName { get; set; }

        [Attr]
        public string PasswordHash { get; set; }

        [Attr]
        public string PhoneNumber { get; set; }

        [Attr]
        public bool PhoneNumberConfirmed { get; set; }

        [Attr]
        public string SecurityStamp { get; set; }

        [Attr]
        public bool TwoFactorEnabled { get; set; }

        [Attr]
        public string UserName { get; set; }

        [Attr]
        public string LastIp { get; set; }

        [Attr]
        public string FirstName { get; set; }

        [Attr]
        public string LastName { get; set; }

        [Attr]
        public DateTime? LastAccess { get; set; }

        [Attr]
        public int TenantId { get; set; }

        [Attr]
        public DateTime PasswordLastChange { get; set; }

        [Attr]
        public bool? IsPasswordMd5 { get; set; }

        [Attr]
        public bool? Deleted { get; set; }

        [Attr]
        public bool? FakeEmail { get; set; }



        [Attr]
        public string FreeFieldString1 { get; set; }
        [Attr]
        public string FreeFieldString2 { get; set; }
        [Attr]
        public string FreeFieldString3 { get; set; }
        [Attr]
        public DateTime? FreeFieldDateTime { get; set; }
        [Attr]
        public int? FreeFieldInt2 { get; set; }
        [Attr]
        public int? FreeFieldInt1 { get; set; }
        [Attr]
        public bool? FreeFieldBoolean { get; set; }


        [HasOne]
        public virtual UserProfile UserProfile { get; set; }

        [HasMany]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        [HasMany]
        public virtual ICollection<UserPreference> UserPreference { get; set; }

        [HasMany]
        public virtual ICollection<BannedUser> BannedUsers { get; set; }

        [HasMany]
        public virtual ICollection<UserTenant> UserTenants { get; set; }

        [HasMany]
        public virtual ICollection<UserAudit> UserAudits { get; set; }

        [HasMany]
        public virtual ICollection<BlockNotification> BlockNotifications { get; set; }
    }
}
