using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
