using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("ErpExternalWorkerDetails")]
    public class ErpExternalWorkerDetails : Identifiable<string>
    {
        [Attr] public int TenantId { get; set; }
        [Attr] public string EmployeeId { get; set; }
        [Attr] public string VatNumber { get; set; }
        [Attr] public string ContractDetails { get; set; }
        [Attr] public string PaymentFrequency { get; set; }
        [Attr] public decimal? HourlyRate { get; set; }
        [Attr] public DateTime CreatedAt { get; set; }
        [Attr] public DateTime UpdatedAt { get; set; }

        [HasOne] public virtual ErpEmployee Employee { get; set; }
        [HasOne] public virtual Tenant Tenant { get; set; }
    }
}
