using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MIT.Fwk.Core.Options;
using System;
using System.Text;

namespace MIT.Fwk.WebApi.Extension
{
    public class JwtTokenProvider
    {
        private static JwtOptions _jwtOptions;

        public JwtTokenProvider() { }

        public static void Initialize(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value ?? throw new InvalidOperationException("JwtOptions not configured. Ensure IOptions<JwtOptions> is registered in DI.");
        }

        // FASE 7: ConfigurationHelper fallbacks removed - Initialize() must be called before accessing these properties
        public static string Base64Key => Convert.ToBase64String(Encoding.UTF8.GetBytes(_jwtOptions?.SecretKey ?? throw new InvalidOperationException("JwtTokenProvider.Initialize() must be called before accessing properties.")));
        public static byte[] SecretKey => Encoding.UTF8.GetBytes(_jwtOptions?.SecretKey ?? throw new InvalidOperationException("JwtTokenProvider.Initialize() must be called before accessing properties."));
        public static string Issuer => _jwtOptions?.Issuer ?? throw new InvalidOperationException("JwtTokenProvider.Initialize() must be called before accessing properties.");
        public static bool Enabled => _jwtOptions?.Enabled ?? throw new InvalidOperationException("JwtTokenProvider.Initialize() must be called before accessing properties.");

        public static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                //Same Secret key will be used while creating the token
                IssuerSigningKey = new SymmetricSecurityKey(JwtTokenProvider.SecretKey) { KeyId = "C59FB46A-005E-43DA-AF15-1900DD3A803A" },
                ValidateIssuer = true,
                //Usually, this is your application base URL
                ValidIssuer = Issuer,
                ValidateAudience = false,
                //Here, we are creating and using JWT within the same application.
                //In this case, base URL is fine.
                //If the JWT is created using a web service, then this would be the consumer URL.
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }

    }
}
