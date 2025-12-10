using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    public class GeoThirdDivision : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public int GeoFirstDivisionId { get; set; }
        [Attr]
        public int GeoSecondDivisionId { get; set; }
        [Attr]
        public int GeoCountryId { get; set; }
        [HasOne]
        public virtual GeoFirstDivision GeoFirstDivision { get; set; }
        [HasOne]
        public virtual GeoSecondDivision GeoSecondDivision { get; set; }
        [HasOne]
        public virtual GeoCountry GeoCountry { get; set; }
        [HasMany]
        public virtual ICollection<GeoMapping> GeoMappings { get; set; }
    }
}
