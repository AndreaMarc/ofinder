using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class Translation : Identifiable<int>
    {
        [Attr]
        public string languageCode { get; set; }

        [Attr]
        public string translationWeb { get; set; }

        [Attr]
        public string translationApp { get; set; }


    }
}
