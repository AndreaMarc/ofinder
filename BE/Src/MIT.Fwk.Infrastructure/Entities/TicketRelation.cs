using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketRelations")]
    public class TicketRelation : Identifiable<string>
    {



        [HasMany]
        public virtual ICollection<TicketTicketRelation> TicketTicketRelation { get; set; }



    }
}
