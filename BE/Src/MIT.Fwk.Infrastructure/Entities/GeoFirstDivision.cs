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
    public class GeoFirstDivision : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public int GeoCountryId { get; set; }
        [HasOne]
        public virtual GeoCountry GeoCountry { get; set; }
        [HasMany]
        public virtual ICollection<GeoSecondDivision> GeoSecondDivisions { get; set; }
        [HasMany]
        public virtual ICollection<GeoThirdDivision> GeoThirdDivisions { get; set; }
        [HasMany]
        public virtual ICollection<GeoMapping> GeoMappings { get; set; }
    }
}
