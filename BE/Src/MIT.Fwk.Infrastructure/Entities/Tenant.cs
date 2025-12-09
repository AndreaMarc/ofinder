using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{

    [Table("Tenants")]
    public class Tenant : Identifiable<int>
    {

        [Attr]
        public string Name { get; set; }

        [Attr]
        public string Description { get; set; }

        [Attr]
        public string Organization { get; set; }

        [Attr]
        public bool Enabled { get; set; }

        [Attr]
        public int ParentTenant { get; set; }

        [Attr]
        public string TenantVAT { get; set; }

        [Attr]
        public string TaxId { get; set; }

        [Attr]
        public string Email { get; set; }

        [Attr]
        public string TenantPEC { get; set; }

        [Attr]
        public string PhoneNumber { get; set; }

        [Attr]
        public string WebSite { get; set; }

        [Attr]
        public string TenantSDI { get; set; }

        [Attr]
        public string TenantIBAN { get; set; }

        [Attr]
        public string Owner { get; set; }

        [Attr]
        public string Commercial { get; set; }

        [Attr]
        public decimal? ShareCapital { get; set; }

        [Attr]
        public string RegisteredOfficeAddress { get; set; }

        [Attr]
        public string RegisteredOfficeCity { get; set; }

        [Attr]
        public string RegisteredOfficeProvince { get; set; }

        [Attr]
        public string RegisteredOfficeState { get; set; }

        [Attr]
        public string RegisteredOfficeRegion { get; set; }

        [Attr]
        public string RegisteredOfficeZIP { get; set; }

        [Attr]
        public string BillingAddressAddress { get; set; }

        [Attr]
        public string BillingAddressCity { get; set; }

        [Attr]
        public string BillingAddressProvince { get; set; }

        [Attr]
        public string BillingAddressState { get; set; }

        [Attr]
        public string BillingAddressRegion { get; set; }

        [Attr]
        public string BillingAddressZIP { get; set; }

        [Attr]
        public bool matchBillingAddress { get; set; }

        [Attr]
        public bool isErasable { get; set; }

        [Attr]
        public bool isRecovery { get; set; }

        [Attr]
        public string FreeFieldString1 { get; set; }
        [Attr]
        public string FreeFieldString2 { get; set; }
        [Attr]
        public string FreeFieldString3 { get; set; }
        [Attr]
        public DateTime? FreeFieldDateTime1 { get; set; }
        [Attr]
        public DateTime? FreeFieldDateTime2 { get; set; }
        [Attr]
        public int? FreeFieldInt2 { get; set; }
        [Attr]
        public int? FreeFieldInt1 { get; set; }
        [Attr]
        public decimal? FreeFieldFloat { get; set; }
        [Attr]
        public bool? FreeFieldBoolean1 { get; set; }
        [Attr]
        public bool? FreeFieldBoolean2 { get; set; }

        [HasMany]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        [HasMany]
        public virtual ICollection<UserTenant> UserTenants { get; set; }

        [HasMany]
        public virtual ICollection<Category> Categories { get; set; }

        [HasMany]
        public virtual ICollection<BlockNotification> BlockNotifications { get; set; }
    }
}
