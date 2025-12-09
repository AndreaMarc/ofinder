using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class MediaFile : Identifiable<string>
    {
        [Attr]
        public string typologyArea { get; set; }

        [Attr]
        public string category { get; set; }
        [Attr]
        public string album { get; set; }
        [Attr]
        public string originalFileName { get; set; }
        [Attr]
        public string extension { get; set; }
        [Attr]
        public string fileUrl { get; set; }
        [Attr]
        public string base64 { get; set; }

        [Attr]
        public string mongoGuid { get; set; }

        [Attr]
        public string alt { get; set; }
        [Attr]
        public string tag { get; set; }
        [Attr]
        public int tenantId { get; set; }
        [Attr]
        public string userGuid { get; set; }
        [Attr]
        public DateTime uploadDate { get; set; }
        [Attr]
        public string primaryContentType { get; set; }
        [Attr]
        public bool global { get; set; }

        [HasOne]
        public virtual MediaCategory TypologyAreaRel { get; set; }

        [HasOne]
        public virtual MediaCategory CategoryRel { get; set; }

        [HasOne]
        public virtual MediaCategory AlbumRel { get; set; }
    }
}
