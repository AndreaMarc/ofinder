using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    public class GeoCountry : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public string Iso3 { get; set; }
        [Attr]
        public string NumericCode { get; set; }
        [Attr]
        public string Iso2 { get; set; }
        [Attr]
        public string PhoneCode { get; set; }
        [Attr]
        public string Capital { get; set; }
        [Attr]
        public string Currency { get; set; }
        [Attr]
        public string CurrencyName { get; set; }
        [Attr]
        public string CurrencySymbol { get; set; }
        [Attr]
        public string InternetDomain { get; set; }
        [Attr]
        public string Native { get; set; }
        [Attr]
        public int? GeoRegionId { get; set; }
        [Attr]
        public int? GeoSubregionId { get; set; }
        [Attr]
        public string TimeZones { get; set; }
        [Attr]
        public string Translations { get; set; }
        [Attr]
        public int? NumberOfDivisions { get; set; }

        [HasOne]
        public virtual GeoRegion GeoRegion { get; set; }

        [HasOne]
        public virtual GeoSubregion GeoSubregion { get; set; }
        [HasMany]
        public virtual ICollection<GeoCity> GeoCities { get; set; }
        [HasMany]
        public virtual ICollection<GeoFirstDivision> GeoFirstDivisions { get; set; }
        [HasMany]
        public virtual ICollection<GeoSecondDivision> GeoSecondDivisions { get; set; }
        [HasMany]
        public virtual ICollection<GeoThirdDivision> GeoThirdDivisions { get; set; }
        [HasMany]
        public virtual ICollection<GeoMapping> GeoMappings { get; set; }
    }
}
