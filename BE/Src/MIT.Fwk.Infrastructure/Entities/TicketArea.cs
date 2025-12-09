using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketAreas")]
    public class TicketArea : Identifiable<string>
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Note { get; set; }


        [HasMany]
        public virtual ICollection<TicketOperator> TicketOperator { get; set; }








    }
}
