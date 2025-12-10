using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("ThirdPartsTokens")]
    public class ThirdPartsToken : Identifiable<string>
    {
        [Attr]
        public string UserId { get; set; }

        [Attr]
        public string OtpId { get; set; }

        [Attr]
        public string Email { get; set; }

        [Attr]
        public string AccessToken { get; set; }

        [Attr]
        public string RefreshToken { get; set; }

        [Attr]
        public string AccessType { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
