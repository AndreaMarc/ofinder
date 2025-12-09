using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Examples.Entities
{
    /// <summary>
    /// Example Product entity with JsonAPI support.
    /// The [Resource] attribute auto-generates CRUD endpoints at /api/v2/products
    /// </summary>
    [Resource]
    [Table("ExampleProducts")]
    [SkipJwtAuthentication]
    [SkipClaimsValidation]
    [SkipRequestLogging]
    public class ExampleProduct : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [Attr]
        public int Stock { get; set; }

        [Attr]
        public string Sku { get; set; }

        [Attr]
        public bool IsActive { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; }

        [Attr]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Foreign key to Category
        /// </summary>
        [Attr]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Navigation property to Category.
        /// [HasOne] creates a relationship in JsonAPI responses.
        /// </summary>
        [HasOne]
        public virtual ExampleCategory Category { get; set; }
    }
}
