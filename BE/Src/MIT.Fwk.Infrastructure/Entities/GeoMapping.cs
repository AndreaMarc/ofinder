using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    public class GeoMapping : Identifiable<int>
    {
        [Attr]
        public string ZipCode { get; set; }
        [Attr]
        public int GeoFirstDivisionId { get; set; }
        [Attr]
        public int GeoSecondDivisionId { get; set; }
        [Attr]
        public int GeoThirdDivisionId { get; set; }
        [Attr]
        public int GeoCityId { get; set; }
        [Attr]
        public int GeoCountryId { get; set; }
        [HasOne]
        public virtual GeoFirstDivision GeoFirstDivision { get; set; }
        [HasOne]
        public virtual GeoSecondDivision GeoSecondDivision { get; set; }
        [HasOne]
        public virtual GeoThirdDivision GeoThirdDivision { get; set; }
        [HasOne]
        public virtual GeoCity GeoCity { get; set; }
        [HasOne]
        public virtual GeoCountry GeoCountry { get; set; }
    }
}
