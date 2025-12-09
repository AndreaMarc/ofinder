using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.ManageViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }

        public string Username { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        public string SecurityToken { get; set; } //  "FD51DA5E-BFBB-4F41-892C-72EE417BEC27"

        public string Username { get; set; }
    }

    public class ResetPasswordOtpModel
    {
        [Required]
        public string Otp { get; set; } //  "FD51DA5E-BFBB-4F41-892C-72EE417BEC27"

        public string md5Password { get; set; }

        public string email { get; set; }

    }
}
