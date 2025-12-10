using MIT.Fwk.Infrastructure.Entities;
using System.Security.Cryptography;
using System.Text;

namespace MIT.Fwk.Tests.WebApi.Helpers
{
    /// <summary>
    /// Factory for creating test data with reasonable defaults.
    /// Helps maintain consistency across tests and reduces boilerplate.
    /// </summary>
    public static class TestDataBuilder
    {
        private static int _testCounter = 0;

        /// <summary>
        /// Gets a unique test counter for generating unique test data.
        /// </summary>
        private static int GetNextCounter()
        {
            return Interlocked.Increment(ref _testCounter);
        }

        /// <summary>
        /// Default test password (meets minimum 32 character requirement).
        /// </summary>
        public const string DefaultTestPassword = "TestPassword2024!SecurePassword32";

        /// <summary>
        /// Creates a test user with default values.
        /// </summary>
        public static MITApplicationUser CreateTestUser(int tenantId = 1, string? email = null)
        {
            var counter = GetNextCounter();
            email ??= $"test.user.{counter}@maestrale.it";

            return new MITApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email,
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper(),
                EmailConfirmed = true,
                FirstName = $"Test{counter}",
                LastName = "User",
                TenantId = tenantId
            };
        }

        /// <summary>
        /// Creates a test role with default values.
        /// </summary>
        public static Role CreateTestRole(int tenantId = 1, short level = 50)
        {
            var counter = GetNextCounter();

            return new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"TestRole{counter}",
                NormalizedName = $"TESTROLE{counter}",
                TenantId = tenantId,
                Level = level
            };
        }

        /// <summary>
        /// Creates a test category.
        /// </summary>
        public static Category CreateTestCategory(int tenantId = 1)
        {
            var counter = GetNextCounter();

            return new Category
            {
                Name = $"Test Category {counter}",
                Description = "Test description",
                Type = "test",
                ParentCategory = 0,
                Erasable = true,
                Code = $"TEST{counter}",
                TenantId = tenantId
            };
        }

        /// <summary>
        /// Creates a test media category.
        /// </summary>
        public static MediaCategory CreateTestMediaCategory(int tenantId = 1)
        {
            var counter = GetNextCounter();

            return new MediaCategory
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Test Media Category {counter}",
                Description = "Test media description",
                Type = "test",
                ParentMediaCategory = null,
                Erasable = true,
                Code = $"MEDIA{counter}",
                TenantId = tenantId,
                Order = 0
            };
        }

        /// <summary>
        /// Creates a test tenant.
        /// </summary>
        public static Tenant CreateTestTenant()
        {
            var counter = GetNextCounter();

            return new Tenant
            {
                Name = $"Test Tenant {counter}",
                Description = "Test tenant description",
                Organization = "Test Organization",
                Enabled = true,
                ParentTenant = 0
            };
        }

        /// <summary>
        /// Creates a test legal term.
        /// </summary>
        public static LegalTerm CreateTestLegalTerm()
        {
            var counter = GetNextCounter();

            return new LegalTerm
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Test Legal Term {counter}",
                Note = "Test legal note",
                Code = $"LEGAL{counter}",
                Active = false,
                Language = "it",
                Version = "1.0",
                DataActivation = DateTime.UtcNow,
                Content = "Test legal content"
            };
        }

        /// <summary>
        /// Creates a test template.
        /// </summary>
        public static Template CreateTestTemplate(int categoryId)
        {
            var counter = GetNextCounter();

            return new Template
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Test Template {counter}",
                Description = "Test template description",
                Content = "<p>Test content</p>",
                ContentNoHtml = "Test content",
                Active = true,
                Tags = "test",
                Language = "it",
                Code = $"TPL{counter}",
                Erasable = true,
                FreeField = "",
                CategoryId = categoryId
            };
        }

        /// <summary>
        /// Creates a test media file.
        /// Note: MediaFile properties use camelCase naming.
        /// </summary>
        public static MediaFile CreateTestMediaFile()
        {
            return new MediaFile
            {
                Id = Guid.NewGuid().ToString()
                // MediaFile has all camelCase properties - simplified for test
            };
        }

        /// <summary>
        /// Creates a test integration.
        /// </summary>
        public static Integration CreateTestIntegration()
        {
            var counter = GetNextCounter();

            return new Integration
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Test Integration {counter}",
                Code = Guid.NewGuid().ToString(),
                Active = true,
                Url = $"https://test-integration-{counter}.example.com"
            };
        }

        /// <summary>
        /// Creates a test setup.
        /// Note: Setup entity uses camelCase property names.
        /// </summary>
        public static Setup CreateTestSetup()
        {
            return new Setup
            {
                environment = "test",
                minAppVersion = "1.0.0",
                maintenance = false,
                useRemoteFiles = false,
                disableLog = false,
                publicRegistration = true,
                sliderPosition = "left",
                sliderPics = "",
                availableLanguages = "it,en",
                defaultLanguage = "it",
                failedLoginAttempts = 3,
                previousPasswordsStored = 3,
                defaultUserPassword = DefaultTestPassword,
                languageSetup = "",
                passwordExpirationPeriod = 90,
                blockingPeriodDuration = 0,
                sliderRegistrationPosition = "",
                sliderTermsPosition = "",
                headerLight = "",
                sidebarLight = "",
                headerBackground = "",
                sidebarBackground = "",
                beLanguage = "it",
                fixedSidebar = true,
                fixedFooter = true,
                fixedHeader = true,
                bodyTabsShadow = true,
                bodyTabsLine = true,
                appThemeWhite = true,
                headerShadow = true,
                sidebarShadow = true,
                useUrlStaticFiles = true,
                entitiesList = "",
                routesList = "",
                defaultClaims = "",
                accessTokenExpiresIn = 3600,
                refreshTokenExpiresIn = 86400,
                canChangeTenants = true,
                internalChat = "",
                internalNotifications = true,
                pushNotifications = true,
                canSearch = true,
                registrationFields = "",
                mailTokenExpiresIn = 3600,
                mailerUsesAltText = false,
                forceLoginRedirect = false,
                needRequestAssociation = false
            };
        }

        /// <summary>
        /// Generates an MD5 hash for a string (used for password hashing in tests).
        /// </summary>
        public static string GetMd5Hash(string input)
        {
            byte[] data = MD5.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
