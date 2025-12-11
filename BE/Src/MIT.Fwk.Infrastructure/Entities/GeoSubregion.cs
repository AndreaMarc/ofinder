using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Attributes;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [SkipJwtAuthentication]
    [SkipClaimsValidation]
    [SkipRequestLogging]
    public class GeoSubregion : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public string Translations { get; set; }
        [Attr]
        public int GeoRegionId { get; set; }

        [HasOne]
        public virtual GeoRegion GeoRegion { get; set; }
        [HasMany]
        public virtual ICollection<GeoCountry> GeoCountries { get; set; }

    }
}
