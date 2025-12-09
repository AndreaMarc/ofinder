using MIT.Fwk.Infrastructure.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.AccountViewModels
{
    public class CurrentUserViewModel
    {
        public string Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsAuthenticated { get; set; }

        public bool IsEnabled { get; set; }
        public string TermsAcceptanceDate { get; set; }

        public string AuthorizationHeader { get; set; }
        public string AuthorizationBearer { get; set; }
        public string AuthorizationRefreshBearer { get; set; }
        public int AuthorizationExpiresIn { get; set; }
        public int AuthorizationRefreshExpiresIn { get; set; }

        public string SecurityStamp { get; set; }

        public string DefaultLanguage { get; set; }

        public int TenantId { get; set; }

        public List<AssociatedTenant> AssociatedTenants { get; set; }
        public string ClaimsList { get; set; }
        public string ProfileImageId { get; set; }
        public bool CurrentTenantActive { get; set; }

    }
}
