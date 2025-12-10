using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MIT.Fwk.Tests.WebApi.Helpers
{
    /// <summary>
    /// Helper for authentication-related operations in tests.
    /// Creates test users, roles, and generates JWT tokens.
    /// </summary>
    public class AuthHelper
    {
        private readonly UserManager<MITApplicationUser> _userManager;
        private readonly RoleManager<MITApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _jwtOptions;

        public AuthHelper(
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IConfiguration configuration,
            IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _jwtOptions = jwtOptions.Value;
        }

        /// <summary>
        /// Creates a test user with a password and assigns roles.
        /// Returns the created user.
        /// </summary>
        public async Task<MITApplicationUser> CreateTestUserAsync(
            string? email = null,
            string? password = null,
            int tenantId = 1,
            bool isSuperAdmin = false,
            int roleLevel = 50)
        {
            email ??= $"test.{Guid.NewGuid()}@maestrale.it";
            password ??= TestDataBuilder.DefaultTestPassword;

            var user = TestDataBuilder.CreateTestUser(tenantId, email);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign roles
            if (isSuperAdmin)
            {
                // Create SuperAdmin role if it doesn't exist
                var superAdminRole = await EnsureRoleExistsAsync("SuperAdmin", tenantId, 0);
                await _userManager.AddToRoleAsync(user, superAdminRole.Name ?? "");

                // Add superadmin claim
                await _userManager.AddClaimAsync(user, new Claim("superadmin", "True"));
            }
            else
            {
                // Create and assign a test role with the specified level
                var testRole = await EnsureRoleExistsAsync($"TestRole_{roleLevel}", tenantId, roleLevel);
                await _userManager.AddToRoleAsync(user, testRole.Name ?? "");
            }

            return user;
        }

        /// <summary>
        /// Ensures a role exists, creating it if necessary.
        /// </summary>
        private async Task<MITApplicationRole> EnsureRoleExistsAsync(string roleName, int tenantId, int level)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new MITApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper(),
                    TenantId = tenantId,
                    Level = (short)level
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            return role;
        }

        /// <summary>
        /// Generates a JWT token for a user.
        /// Returns the token string without "Bearer " prefix.
        /// </summary>
        public async Task<string> GenerateJwtTokenAsync(MITApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim("username", user.Email ?? user.UserName ?? ""),
                new Claim("userId", user.Id),
                new Claim("tenantId", user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id)
            };

            // Add roles as claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add user claims
            claims.AddRange(userClaims);

            // Get JWT secret from configuration
            var secret = _jwtOptions.SecretKey ?? _configuration["Authentication:Jwt:SecretKey"] ?? "DefaultSecretKeyForTesting";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresIn = _jwtOptions.AccessTokenExpiresIn > 0
                ? _jwtOptions.AccessTokenExpiresIn
                : 3600; // Default 1 hour

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer ?? "Maestrale Information Technology",
                audience: _jwtOptions.Audience ?? "MICH2020",
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(expiresIn),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Creates a test user and generates a JWT token for it.
        /// Returns tuple of (user, token).
        /// </summary>
        public async Task<(MITApplicationUser user, string token)> CreateTestUserWithTokenAsync(
            bool isSuperAdmin = false,
            int tenantId = 1,
            int roleLevel = 50)
        {
            var user = await CreateTestUserAsync(
                email: null,
                password: TestDataBuilder.DefaultTestPassword,
                tenantId: tenantId,
                isSuperAdmin: isSuperAdmin,
                roleLevel: roleLevel
            );

            var token = await GenerateJwtTokenAsync(user);

            return (user, $"Bearer {token}");
        }

        /// <summary>
        /// Deletes a test user by ID.
        /// </summary>
        public async Task DeleteTestUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        /// <summary>
        /// Gets or creates the default test user from configuration.
        /// Used for compatibility with existing tests.
        /// </summary>
        public async Task<(MITApplicationUser user, string token)> GetOrCreateDefaultTestUserAsync()
        {
            var email = _configuration["UnitTest:LoginCreds:email"] ?? "unit.test@maestrale.it";
            var password = _configuration["UnitTest:LoginCreds:password"] ?? TestDataBuilder.DefaultTestPassword;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = await CreateTestUserAsync(
                    email: email,
                    password: password,
                    tenantId: 1,
                    isSuperAdmin: true
                );
            }

            var token = await GenerateJwtTokenAsync(user);

            return (user, $"Bearer {token}");
        }
    }
}
