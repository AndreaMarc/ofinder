using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.WebApi.Controllers;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Infrastructure
{
    /// <summary>
    /// Base class for all integration tests.
    /// Provides access to DI services (DbContext, UserManager, Mapper, etc.).
    /// Implements IClassFixture to share TestDatabaseFixture across tests.
    /// Implements IDisposable for per-test cleanup.
    /// </summary>
    public abstract class IntegrationTestBase : IClassFixture<TestDatabaseFixture>, IDisposable
    {
        protected readonly TestDatabaseFixture Fixture;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IServiceScope Scope;
        protected readonly IConfiguration Configuration;
        protected readonly ITestOutputHelper Output;

        // Common services accessible to all tests
        protected readonly JsonApiDbContext Context;
        protected readonly IMapper Mapper;
        protected readonly UserManager<MITApplicationUser> UserManager;
        protected readonly SignInManager<MITApplicationUser> SignInManager;
        protected readonly RoleManager<MITApplicationRole> RoleManager;
        protected readonly IJsonApiManualService JsonApiManualService;

        protected IntegrationTestBase(TestDatabaseFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
            ServiceProvider = fixture.ServiceProvider;
            Configuration = fixture.Configuration;

            // Create a new scope for this test instance
            Scope = ServiceProvider.CreateScope();

            // Resolve common services
            Context = Scope.ServiceProvider.GetRequiredService<JsonApiDbContext>();
            Mapper = Scope.ServiceProvider.GetRequiredService<IMapper>();
            UserManager = Scope.ServiceProvider.GetRequiredService<UserManager<MITApplicationUser>>();
            SignInManager = Scope.ServiceProvider.GetRequiredService<SignInManager<MITApplicationUser>>();
            RoleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<MITApplicationRole>>();
            JsonApiManualService = Scope.ServiceProvider.GetRequiredService<IJsonApiManualService>();
        }

        /// <summary>
        /// Gets a service from the DI container.
        /// Useful for accessing custom DbContexts or other services.
        /// </summary>
        protected T GetService<T>() where T : notnull
        {
            return Scope.ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets a service or null if not registered.
        /// </summary>
        protected T? GetServiceOrDefault<T>() where T : class
        {
            return Scope.ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// Disposes the test scope after each test completes.
        /// Override this method to add custom cleanup logic.
        /// </summary>
        public virtual void Dispose()
        {
            Scope?.Dispose();
        }

        /// <summary>
        /// Helper to write formatted output to test console.
        /// </summary>
        protected void WriteLine(string message)
        {
            Output.WriteLine($"{message}\n");
        }

        /// <summary>
        /// Helper to write success message to test console.
        /// </summary>
        protected void WriteSuccess(string entityName)
        {
            Output.WriteLine($"{entityName,-40} | TEST RIUSCITO | OK");
        }

        /// <summary>
        /// Helper to write failure message to test console.
        /// </summary>
        protected void WriteFailure(string entityName, string? reason = null)
        {
            Output.WriteLine($"{entityName,-40} | TEST FALLITO | {reason ?? "Error"}");
        }

        /// <summary>
        /// Helper to write section header to test console.
        /// </summary>
        protected void WriteSectionHeader(string section)
        {
            Output.WriteLine("");
            Output.WriteLine("----------------------------------------");
            Output.WriteLine(section);
            Output.WriteLine("----------------------------------------");
        }

        /// <summary>
        /// Creates an AccountController instance with all required dependencies for testing.
        /// </summary>
        protected AccountController CreateAccountController()
        {
            return new AccountController(
                signInManager: SignInManager,
                userManager: UserManager,
                roleManager: RoleManager,
                jsonApiManualService: JsonApiManualService,
                googleService: GetService<IGoogleService>(),
                email: GetService<IEmailSender>(),
                jwtOptions: GetService<IOptions<JwtOptions>>(),
                loggerFactory: GetService<ILoggerFactory>()
            );
        }

        /// <summary>
        /// Sets up HttpContext for controller testing.
        /// Simulates HTTP headers and context for integration tests.
        /// Configures BOTH controller.ControllerContext.HttpContext AND IHttpContextAccessor from DI.
        /// </summary>
        protected void SetupHttpContext(AccountController controller, string platform = "web", bool unitTest = true)
        {
            var httpContext = new DefaultHttpContext
            {
                // Inject ServiceProvider into HttpContext so SignInManager can resolve services
                RequestServices = Scope.ServiceProvider
            };

            // Add required headers
            httpContext.Request.Headers["platform"] = platform;
            if (unitTest)
            {
                httpContext.Request.Headers["unitTest"] = "True"; // Skip email sending in tests
            }
            httpContext.Request.Headers["Content-Type"] = "application/vnd.api+json";

            // Simulate connection info
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Set controller HttpContext
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // CRITICAL: Configure IHttpContextAccessor from DI to use the same HttpContext
            // This ensures SignInManager (which depends on IHttpContextAccessor) can access the context
            var httpContextAccessor = Scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = httpContext;
        }

        /// <summary>
        /// Cleanup helper: Deletes user by email if exists.
        /// </summary>
        protected async Task DeleteUserByEmail(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user != null)
            {
                await UserManager.DeleteAsync(user);
            }
        }
    }
}
