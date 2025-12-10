using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [Table("DeadlineArchives")]
    public class DeadlineArchive : Identifiable<string>
    {
        [Attr]
        public DateTime CreatedAt { get; set; }
        [Attr]
        public bool Replaced { get; set; }
        [Attr]
        public DateTime Deadline { get; set; }
        [Attr]
        public string OperatorId { get; set; }
        [Attr]
        public string Entity { get; set; }

        [HasOne]
        public virtual TicketOperator Operator { get; set; }






    }
}
