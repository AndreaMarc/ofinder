using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("CustomSetups")]
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class CustomSetup : Identifiable<string>
    {

        [Attr] public string Generic { get; set; }
        [Attr] public string Environment { get; set; }
        [Attr] public bool MaintenanceAdmin { get; set; }

    }
}
