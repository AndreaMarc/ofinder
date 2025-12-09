using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Examples.Entities
{
    /// <summary>
    /// Example Category entity with JsonAPI support.
    /// The [Resource] attribute auto-generates CRUD endpoints at /api/v2/categories
    /// </summary>
    [Table("ExampleCategories")]
    public class ExampleCategory : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public bool IsActive { get; set; }

        /// <summary>
        /// Navigation property for related products.
        /// [HasMany] creates a relationship in JsonAPI responses.
        /// </summary>
        [HasMany]
        public virtual ICollection<ExampleProduct> Products { get; set; }
    }
}
