using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("Category")]
    public class Category : Identifiable<int>
    {

        [Attr] public string Name { get; set; }
        [Attr] public string Description { get; set; }
        [Attr] public string Type { get; set; }
        [Attr] public int ParentCategory { get; set; }
        [Attr] public bool Erasable { get; set; }
        [Attr] public bool CopyInNewTenants { get; set; }
        [Attr] public int TenantId { get; set; }
        [Attr] public int Order { get; set; }
        [Attr] public string Code { get; set; }

        [HasOne]
        public virtual Tenant Tenant { get; set; }

        [HasMany]
        public virtual ICollection<Template> Template { get; set; }



    }
}
