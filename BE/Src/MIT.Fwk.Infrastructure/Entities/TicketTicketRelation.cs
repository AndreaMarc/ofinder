using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketTicketRelations")]
    public class TicketTicketRelation : Identifiable<string>
    {
        [Attr]
        public string TicketRelationId { get; set; }

        [Attr]
        public string TicketId { get; set; }
        [Attr]
        public string Typology { get; set; }

        [Attr]
        public string FatherTicketId { get; set; }


        [HasOne]
        public virtual Ticket Ticket { get; set; }

        [HasOne]
        public virtual Ticket FatherTicket { get; set; }

        [HasOne]
        public virtual TicketRelation TicketRelation { get; set; }








    }
}
