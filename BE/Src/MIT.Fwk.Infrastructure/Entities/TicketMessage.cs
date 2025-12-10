using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketMessages")]
    public class TicketMessage : Identifiable<string>
    {
        [Attr]
        public string TicketId { get; set; }
        [Attr]
        public string Message { get; set; }
        [Attr]
        public string UserId { get; set; }
        [Attr]
        public string TicketOperatorId { get; set; }

        [Attr]
        public DateTime CreationDate { get; set; }


        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual TicketOperator TicketOperator { get; set; }

        [HasOne]
        public virtual Ticket Ticket { get; set; }





    }
}
