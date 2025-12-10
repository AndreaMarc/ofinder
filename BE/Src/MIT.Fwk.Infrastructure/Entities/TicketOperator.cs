using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("TicketOperators")]
    public class TicketOperator : Identifiable<string>
    {
        [Attr]
        public string TicketAreaId { get; set; }

        [Attr]
        public string UserId { get; set; }


        [HasOne]
        public virtual TicketArea TicketArea { get; set; }


        [HasOne]
        public virtual User User { get; set; }







    }
}
