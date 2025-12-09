using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketHistory")]
    public class TicketHistory : Identifiable<string>
    {
        [Attr]
        public string Operation { get; set; }

        [Attr]
        public string UserId { get; set; }

        [Attr]
        public string TicketOperatorId { get; set; }

        [Attr]
        public string Details { get; set; }

        [Attr]
        public DateTime? CreationDate { get; set; }



        [HasOne]
        public virtual User User { get; set; }

        [HasOne]
        public virtual TicketOperator TicketOperator { get; set; }





    }
}
