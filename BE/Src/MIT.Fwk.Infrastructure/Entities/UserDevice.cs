using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("UserDevices")]
    public class UserDevice : Identifiable<Guid>
    {
        [Attr]
        public string userId { get; set; }

        [Attr]
        public string deviceHash { get; set; }

        [Attr]
        public string salt { get; set; }

        [Attr]
        public DateTimeOffset? lastAccess { get; set; }

        [Attr] public string PushToken { get; set; }
        [Attr] public string AppleToken { get; set; }
        [Attr] public string GoogleToken { get; set; }
        [Attr] public string FacebookToken { get; set; }
        [Attr] public string TwitterToken { get; set; }
        [Attr] public string Platform { get; set; }
        [Attr] public string DeviceName { get; set; }

        [HasOne]
        public virtual User User { get; set; }

    }
}
