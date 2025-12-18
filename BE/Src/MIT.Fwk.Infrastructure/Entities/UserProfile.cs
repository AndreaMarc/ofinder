using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("UserProfile")]
    public class UserProfile : Identifiable<string>
    {
        [Attr] public string UserId { get; set; }
        [Attr] public string FirstName { get; set; }
        [Attr] public string LastName { get; set; }
        [Attr] public string NickName { get; set; }
        [Attr] public string FixedPhone { get; set; }
        [Attr] public string MobilePhone { get; set; }
        [Attr] public string Sex { get; set; }
        [Attr] public string TaxId { get; set; }
        [Attr] public DateTime? BirthDate { get; set; }
        [Attr] public string BirthCity { get; set; }
        [Attr] public string BirthProvince { get; set; }
        [Attr] public string BirthZIP { get; set; }
        [Attr] public string BirthState { get; set; }
        [Attr] public string ResidenceCity { get; set; }
        [Attr] public string ResidenceProvince { get; set; }
        [Attr] public string ResidenceZIP { get; set; }
        [Attr] public string ResidenceState { get; set; }
        [Attr] public string ResidenceAddress { get; set; }
        [Attr] public string ResidenceHouseNumber { get; set; }
        [Attr] public string Occupation { get; set; }
        [Attr] public string Description { get; set; }
        [Attr] public string ContactEmail { get; set; }
        [Attr] public string ProfileImageId { get; set; }
        [Attr] public string ProfileFreeFieldString1 { get; set; }
        [Attr] public string ProfileFreeFieldString2 { get; set; }
        [Attr] public string ProfileFreeFieldString3 { get; set; }
        [Attr] public string AppleRefreshToken { get; set; }
        [Attr] public string GoogleRefreshToken { get; set; }
        [Attr] public string FacebookRefreshToken { get; set; }
        [Attr] public string TwitterRefreshToken { get; set; }
        [Attr] public string UserLang { get; set; }
        [Attr] public int? ProfileFreeFieldInt1 { get; set; }
        [Attr] public int? ProfileFreeFieldInt2 { get; set; }
        [Attr] public DateTime? ProfileFreeFieldDateTime { get; set; }
        [Attr] public bool? ProfileFreeFieldBoolean { get; set; }

        [Attr] public string cookieAccepted { get; set; }
        [Attr] public bool? termsAccepted { get; set; }
        [Attr] public DateTime? termsAcceptanceDate { get; set; }
        [Attr] public DateTime? registrationDate { get; set; }
        [Attr] public string birthRegion { get; set; }
        [Attr] public string residenceRegion { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
