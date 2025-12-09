using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("UserPreference")]
    public class UserPreference : Identifiable<string>
    {
        [Attr] public string UserId { get; set; }
        [Attr] public int TenantId { get; set; }
        [Attr] public string PrefKey { get; set; }
        [Attr] public string PrefValue { get; set; }
        [Attr] public DateTime? CreatedAt { get; set; }
        [Attr] public DateTime? UpdatedAt { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
