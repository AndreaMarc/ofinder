using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketAttachments")]
    public class TicketAttachment : Identifiable<string>
    {
        [Attr]
        public string Field { get; set; }
        [Attr]
        public string TicketMessageId { get; set; }
        [Attr]
        public string TicketOperatorId { get; set; }
        [Attr]
        public string UserId { get; set; }



        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual TicketOperator TicketOperator { get; set; }

        [HasOne]
        public virtual TicketMessage TicketMessage { get; set; }


    }
}
