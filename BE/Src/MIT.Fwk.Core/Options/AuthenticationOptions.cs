namespace MIT.Fwk.Core.Options
{
    /// <summary>
    /// Authentication configuration options.
    /// Maps to authentication-related settings in customsettings.json
    /// </summary>
    public class AuthenticationOptions
    {
        public bool RequireOTP { get; set; } = false;
        public int OTPExpirationMinutes { get; set; } = 5;
        public string RolesByPassOtp { get; set; } = string.Empty;
        public string SpecialUsers { get; set; } = string.Empty;
        public int PasswordValidityPeriod { get; set; } = 0;
        public bool PasswordCheckFirstAccess { get; set; } = false;
    }
}
