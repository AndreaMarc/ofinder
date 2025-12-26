using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Examples.Entities.Examples
{
    /// <summary>
    /// Esempio di entità per recensioni prodotti
    /// NOTA: Questa classe è solo di esempio/documentazione.
    /// Non è attualmente implementata nel DbContext.
    /// </summary>
    // [Resource(PublicName = "reviews")] // DISABILITATO - non implementato nel DbContext
    [Table("Reviews")]
    public class Review : Identifiable<int>
    {
        [Attr]
        [Range(1, 5, ErrorMessage = "Il rating deve essere tra 1 e 5")]
        public int Rating { get; set; }

        [Attr]
        [StringLength(1000)]
        public string Comment { get; set; }

        [Attr]
        public string UserName { get; set; }

        [Attr]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // RELAZIONI

        [HasOne]
        public Product Product { get; set; }

        public int ProductId { get; set; }
    }
}
