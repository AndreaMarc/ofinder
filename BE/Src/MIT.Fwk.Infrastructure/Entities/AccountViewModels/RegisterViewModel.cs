using System;
using System.ComponentModel.DataAnnotations;

namespace MIT.Fwk.Infrastructure.Entities.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string FingerPrint { get; set; }
        public int? TenantId { get; set; }



        public bool? IsPasswordMd5 { get; set; }

        // CAMPI NECESSARI PER LA CREAZIONE DI USERPROFILE
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string FixedPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Sex { get; set; }
        public string TaxId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string BirthCity { get; set; }
        public string BirthProvince { get; set; }
        public string BirthZIP { get; set; }
        public string BirthState { get; set; }
        public string ResidenceCity { get; set; }
        public string ResidenceProvince { get; set; }
        public string ResidenceZIP { get; set; }
        public string ResidenceState { get; set; }
        public string ResidenceAddress { get; set; }
        public string ResidenceHouseNumber { get; set; }
        public string Occupation { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public string ProfileImageId { get; set; }
        public string ProfileFreeFieldString1 { get; set; }
        public string ProfileFreeFieldString2 { get; set; }
        public string ProfileFreeFieldString3 { get; set; }
        public int? ProfileFreeFieldInt1 { get; set; }
        public int? ProfileFreeFieldInt2 { get; set; }
        public DateTime? ProfileFreeFieldDateTime { get; set; }
        public bool? ProfileFreeFieldBoolean { get; set; }
        public string AppleRefreshToken { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string FacebookRefreshToken { get; set; }
        public string TwitterRefreshToken { get; set; }
        public string UserLang { get; set; }
        public string cookieAccepted { get; set; }
        public bool? termsAccepted { get; set; }
        public DateTime? termsAcceptanceDate { get; set; }
        public DateTime? registrationDate { get; set; }

    }
}
