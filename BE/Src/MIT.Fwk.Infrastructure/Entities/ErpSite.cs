using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    [Table("ErpSite")]
    public class ErpSite : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string Name { get; set; }
        [Attr] public string Address { get; set; }
        [Attr] public int? AddressNumber { get; set; }
        [Attr] public string Phone { get; set; }
        [Attr] public string City { get; set; }
        [Attr] public string Province { get; set; }
        [Attr] public string Zip { get; set; }
        [Attr] public string State { get; set; }
        [Attr] public bool AdministrativeHeadquarters { get; set; }
        [Attr] public bool RegisteredOffice { get; set; }
        [Attr] public bool OperationalHeadquarters { get; set; }
        [Attr] public string ParentSiteId { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual ErpSite ParentSite { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }

}
