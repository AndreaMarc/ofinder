using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("Template")]
    public class Template : Identifiable<string>
    {


        [Attr] public string Name { get; set; }
        [Attr] public string Description { get; set; }
        [Attr] public string Content { get; set; }
        [Attr] public string ContentNoHtml { get; set; }
        [Attr] public int CategoryId { get; set; }
        [Attr] public bool Active { get; set; }
        [Attr] public string Tags { get; set; }
        [Attr] public string Code { get; set; }
        [Attr] public string Language { get; set; }
        [Attr] public string ObjectText { get; set; }
        [Attr] public string FeaturedImage { get; set; }
        [Attr] public bool Erasable { get; set; }
        [Attr] public bool CopyInNewTenants { get; set; }
        [Attr] public bool Erased { get; set; }
        [Attr] public int Order { get; set; }
        [Attr] public string FreeField { get; set; }
        [Attr] public DateTime? CreatedAt { get; set; }

        [HasOne]
        public virtual Category Category { get; set; }
    }
}
