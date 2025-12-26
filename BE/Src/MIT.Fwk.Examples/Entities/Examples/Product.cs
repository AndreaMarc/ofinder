using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Examples.Entities.Examples
{
    /// <summary>
    /// Esempio di entità JsonAPI base con relazioni.
    /// NOTA: Questa classe è solo di esempio/documentazione.
    /// L'entità effettiva usata è ExampleProduct in MIT.Fwk.Examples.Entities
    /// </summary>
    // [Resource(PublicName = "example-products")] // DISABILITATO - usa ExampleProduct invece
    [Table("ExampleProducts")]
    public class Product : Identifiable<int>
    {
        // ATTRIBUTI SEMPLICI

        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public decimal Price { get; set; }

        [Attr]
        public int Stock { get; set; }

        [Attr]
        public bool IsActive { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Attr]
        public DateTime? UpdatedAt { get; set; }

        // RELAZIONI

        /// <summary>
        /// Relazione Many-to-One: Molti prodotti appartengono a una categoria
        /// Esempio query JsonAPI: /api/v2/example-products?include=category
        /// </summary>
        [HasOne]
        public ExampleCategory Category { get; set; }

        public int? CategoryId { get; set; }

        /// <summary>
        /// Relazione One-to-Many: Un prodotto ha molte recensioni
        /// Esempio query JsonAPI: /api/v2/example-products/{id}?include=reviews
        /// </summary>
        [HasMany]
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
