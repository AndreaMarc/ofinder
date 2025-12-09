using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("MediaCategories")]
    public class MediaCategory : Identifiable<string>
    {

        [Attr] public string Name { get; set; }
        [Attr] public string Code { get; set; }
        [Attr] public string Description { get; set; }
        [Attr] public int Order { get; set; }
        [Attr] public int TenantId { get; set; }
        [Attr] public bool Erasable { get; set; }
        [Attr] public string Type { get; set; }
        [Attr] public string ParentMediaCategory { get; set; }
        [Attr] public bool copyInNewTenant { get; set; }

        [HasOne]
        public virtual Tenant Tenant { get; set; }

        [HasMany]
        public virtual ICollection<MediaFile> TypologyMediaFiles { get; }

        [HasMany]
        public virtual ICollection<MediaFile> CategoryMediaFiles { get; }

        [HasMany]
        public virtual ICollection<MediaFile> AlbumMediaFiles { get; }
    }
}
