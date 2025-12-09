using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class LegalTerm : Identifiable<string>
    {


        [Attr] public string Title { get; set; }
        [Attr] public string Note { get; set; }

        [Attr] public bool Active { get; set; }

        [Attr] public string Code { get; set; }
        [Attr] public string Language { get; set; }
        [Attr] public string Version { get; set; }
        [Attr] public string Content { get; set; }
        [Attr] public DateTime? DataActivation { get; set; }


    }
}
