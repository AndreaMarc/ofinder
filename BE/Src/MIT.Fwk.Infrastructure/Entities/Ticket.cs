using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Models;
using System;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Resource]
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class Ticket : Identifiable<string>
    {
        [Attr]
        public string Email { get; set; }
        [Attr]
        public string Organization { get; set; }
        [Attr]
        public string Vat { get; set; }
        [Attr]
        public string Status { get; set; }
        [Attr]
        public string Message { get; set; }
        [Attr]
        public DateTime CreationDate { get; set; }
        [Attr]
        public string AssignedId { get; set; }
        [Attr]
        public string Phone { get; set; }
        [Attr]
        public string UserId { get; set; }
        [Attr]
        public int TenantId { get; set; }
        [Attr]
        public int Priority { get; set; }
        [Attr]
        public string ClosedById { get; set; }
        [Attr]
        public string Note { get; set; }
        [Attr]
        public DateTime? ClosedDate { get; set; }
        [Attr]
        public string Answer { get; set; }
        [Attr]
        public string FirstName { get; set; }
        [Attr]
        public string LastName { get; set; }
        [Attr]
        public string OrganizationToBeConfirmed { get; set; }
        [Attr]
        [IdentityDB]
        public int? Number { get; set; }
        [Attr]
        public string TicketTagId { get; set; }


        [HasOne]
        public virtual User User { get; set; }
        [HasOne]
        public virtual User Assigned { get; set; }
        [HasOne]
        public virtual Tenant Tenant { get; set; }
        [HasOne]
        public virtual TicketTag TicketTag { get; set; }





    }
}
