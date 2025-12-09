using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketTags")]
    public class TicketTag : Identifiable<string>
    {
        [Attr]
        public string Name { get; set; }
        [Attr]
        public string Tag { get; set; }
        [Attr]
        public string Note { get; set; }



        [HasMany]
        public virtual ICollection<Ticket> Ticket { get; set; }






    }
}
