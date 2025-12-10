using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Entities.AccountViewModels;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Account
{
    /// <summary>
    /// Consolidated tests for Account operations: Registration, Login, Logout, Password Reset.
    /// Tests use direct UserManager/SignInManager instead of HTTP calls.
    /// </summary>
    public class AccountTests : IntegrationTestBase
    {
        private readonly AuthHelper _authHelper;
        private readonly IJsonApiManualService _jsonApiManualService;

        public AccountTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            _authHelper = new AuthHelper(UserManager, RoleManager, Configuration,
                GetService<Microsoft.Extensions.Options.IOptions<MIT.Fwk.Core.Options.JwtOptions>>());
            _jsonApiManualService = GetService<IJsonApiManualService>();
        }

        public async Task AccountRegistration_ShouldSucceed()
        {
            var email = $"test.registration.{Guid.NewGuid()}@maestrale.it";
            var password = TestDataBuilder.DefaultTestPassword;
            string userId = "";

            try
            {
                WriteSectionHeader("Account Registration Test");

                // Arrange
                var controller = CreateAccountController();
                SetupHttpContext(controller);

                var registerModel = new RegisterViewModel
                {
                    Email = email,
                    Password = password,
                    ConfirmPassword = password,
                    FirstName = "Integration",
                    LastName = "Test",
                    FingerPrint = Guid.NewGuid().ToString(),
                    TenantId = 1,
                    termsAccepted = true,
                    ContactEmail = email
                };

                WriteLine($"Attempting to register user: {email}");

                // Act - Call controller.Register() endpoint
                var result = await controller.Register(registerModel);

                // Assert - Registration accepted (StatusCode 202)
                var statusResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, statusResult.StatusCode);

                WriteLine($"✓ User registration accepted: {email}");

                // Verify user was created in database
                var user = await UserManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                Assert.Equal(email, user.Email);
                userId = user.Id;

                WriteLine($"✓ User found in database: {user.Email}");

                WriteSuccess("AccountRegistration");
            }
            catch (Exception ex)
            {
                WriteFailure("AccountRegistration", ex.Message);
                throw;
            } finally
            {
                // Cleanup
                await _jsonApiManualService.DeleteUser(userId);
            }
        }

        public async Task AccountLogin_ShouldSucceed()
        {
            var email = $"test.login.{Guid.NewGuid()}@maestrale.it";
            var password = TestDataBuilder.DefaultTestPassword;
            string userId = "";

            try
            {
                WriteSectionHeader("Account Login Test");

                // Arrange - Register user first using controller
                var controller = CreateAccountController();
                SetupHttpContext(controller);

                var registerModel = new RegisterViewModel
                {
                    Email = email,
                    Password = password,
                    ConfirmPassword = password,
                    FirstName = "Login",
                    LastName = "Test",
                    FingerPrint = Guid.NewGuid().ToString(),
                    TenantId = 1,
                    termsAccepted = true,
                    ContactEmail = email
                };

                await controller.Register(registerModel);
                WriteLine($"Test user registered: {email}");

                // Confirm email (simulate OTP confirmation)
                var user = await UserManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                user.EmailConfirmed = true;
                userId = user.Id;
                await UserManager.UpdateAsync(user);
                WriteLine($"Email confirmed for user: {email}");

                // Act - Login via controller
                var loginModel = new LoginViewModel
                {
                    Email = email,
                    Password = password,
                    RememberMe = true,
                    FingerPrint = Guid.NewGuid().ToString(),
                    UserLang = "it"
                };

                var loginResult = await controller.Login(loginModel);

                // Assert - Login successful (returns OkObjectResult with token)
                var okResult = Assert.IsType<OkObjectResult>(loginResult);
                Assert.NotNull(okResult.Value);

                WriteLine($"✓ Login successful for: {email}");
                WriteLine($"✓ Response type: {okResult.Value.GetType().Name}");

                WriteSuccess("AccountLogin");
            }
            catch (Exception ex)
            {
                WriteFailure("AccountLogin", ex.Message);
                throw;
            }
            finally
            {
                // Cleanup
                await _jsonApiManualService.DeleteUser(userId);
            }
        }

        public async Task AccountPasswordReset_ShouldSucceed()
        {
            var email = $"test.reset.{Guid.NewGuid()}@maestrale.it";
            var oldPassword = await JsonApiManualService.CalculateMD5Hash("OldPassword2024!SecurePassword32Old");
            var newPassword = await JsonApiManualService.CalculateMD5Hash("NewPassword2024!SecurePassword32New");
            string userId = "";

            try
            {
                WriteSectionHeader("Account Password Reset Test");

                // Arrange - Register user first
                var controller = CreateAccountController();
                SetupHttpContext(controller);

                var registerModel = new RegisterViewModel
                {
                    Email = email,
                    Password = oldPassword,
                    ConfirmPassword = oldPassword,
                    FirstName = "Reset",
                    LastName = "Test",
                    FingerPrint = Guid.NewGuid().ToString(),
                    TenantId = 1,
                    termsAccepted = true,
                    ContactEmail = email
                };

                await controller.Register(registerModel);
                WriteLine($"Test user registered: {email}");

                // Confirm email
                var user = await UserManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                user.EmailConfirmed = true;
                userId = user.Id;
                await UserManager.UpdateAsync(user);

                // Act - Generate OTP for password reset
                string otpString = await UserManager.GeneratePasswordResetTokenAsync(user);
                var otp = await _jsonApiManualService.GenerateNewOtp(user.Id, otpString, user.TenantId);
                Assert.NotNull(otp);
                WriteLine($"OTP generated for password reset: {otp.OtpSended}");

                // Act - Reset password via controller
                var resetModel = new MIT.Fwk.Infrastructure.Entities.ManageViewModels.ResetPasswordOtpModel
                {
                    Otp = otp.OtpSended,
                    md5Password = newPassword,
                    email = email
                };

                var resetResult = await controller.ResetPasswordOtp(resetModel);
                var resetOk = Assert.IsType<StatusCodeResult>(resetResult);
                Assert.Equal(204, resetOk.StatusCode);
                WriteLine($"✓ Password reset successful");

                // Act - Verify new password works via Login controller
                var loginModel = new LoginViewModel
                {
                    Email = email,
                    Password = newPassword,
                    RememberMe = false,
                    FingerPrint = Guid.NewGuid().ToString(),
                    UserLang = "it"
                };

                var loginResult = await controller.Login(loginModel);
                var loginOk = Assert.IsType<OkObjectResult>(loginResult);
                WriteLine($"✓ Login with new password successful");

                // Act - Verify old password doesn't work
                var oldLoginModel = new LoginViewModel
                {
                    Email = email,
                    Password = oldPassword,
                    RememberMe = false,
                    FingerPrint = Guid.NewGuid().ToString(),
                    UserLang = "it"
                };

                var oldLoginResult = await controller.Login(oldLoginModel);
                // Should return NotFound or Unauthorized, not Ok
                Assert.IsNotType<OkObjectResult>(oldLoginResult);
                WriteLine($"✓ Old password correctly invalidated");

                WriteSuccess("AccountPasswordReset");
            }
            catch (Exception ex)
            {
                WriteFailure("AccountPasswordReset", ex.Message);
                throw;
            } finally
            {
                // Cleanup
                await _jsonApiManualService.DeleteUser(userId);
            }
        }

        public async Task GetOrCreateDefaultTestUser_ShouldSucceed()
        {
            try
            {
                WriteSectionHeader("Default Test User");

                // Act
                var (user, token) = await _authHelper.GetOrCreateDefaultTestUserAsync();

                // Assert
                Assert.NotNull(user);
                Assert.NotNull(token);
                Assert.Contains("Bearer", token);

                WriteLine($"Default test user: {user.Email}");
                WriteLine($"Token generated: {token.Substring(0, Math.Min(50, token.Length))}...");

                WriteSuccess("GetOrCreateDefaultTestUser");
            }
            catch (Exception ex)
            {
                WriteFailure("GetOrCreateDefaultTestUser", ex.Message);
                throw;
            }
        }
    }
}
