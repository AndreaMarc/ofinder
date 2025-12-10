using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    public class GeoRegion : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public string Translations { get; set; }

        [HasMany]
        public virtual ICollection<GeoSubregion> GeoSubregions { get; set; }
        [HasMany]
        public virtual ICollection<GeoCountry> GeoCountries { get; set; }
    }
}
