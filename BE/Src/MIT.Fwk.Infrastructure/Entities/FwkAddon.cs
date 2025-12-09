using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [SkipJwtAuthentication]
    [SkipClaimsValidation]
    [Table("FwkAddons")]
    public class FwkAddon : Identifiable<string>
    {
        [Attr]
        public int AddonsCode { get; set; }
    }
}
