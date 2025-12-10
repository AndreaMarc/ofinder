using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        [Required]
        public string FingerPrint { get; set; }

        public string UserLang { get; set; }
    }

    public class LoginOTPViewModel
    {
        [Required]
        public string Otp { get; set; }

        [Required]
        public string FingerPrint { get; set; }

        public string UserLang { get; set; }
    }

    public class LoginToOtherAppViewModel
    {
        [Required]
        public string IntegrationId { get; set; }

        [Required]
        public string UserId { get; set; }
    }

    public class AccessRequestWithHash
    {
        [Required]
        public string Hash { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
