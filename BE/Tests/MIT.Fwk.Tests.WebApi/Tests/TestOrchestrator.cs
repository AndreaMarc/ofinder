using MIT.Fwk.Tests.WebApi.Infrastructure;
using MIT.Fwk.Tests.WebApi.Tests.Account;
using MIT.Fwk.Tests.WebApi.Tests.Entities;
using MIT.Fwk.Tests.WebApi.Tests.Entities.Custom;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests
{
    /// <summary>
    /// Main test orchestrator that runs all test suites in sequence.
    /// Provides comprehensive reporting of test results.
    /// This mimics the behavior of the original JsonApiUnitTest.cs.
    /// </summary>
    public class TestOrchestrator : IntegrationTestBase
    {
        public TestOrchestrator(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        /// <summary>
        /// Master test that runs all test suites.
        /// Reports overall success/failure and detailed error information.
        /// </summary>
        [Fact]
        public async Task RunAllTests_ShouldSucceed()
        {
            var stopwatch = Stopwatch.StartNew();
            var errors = new StringBuilder();

            WriteLine("");
            WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            WriteLine("║           MIT.FWK.WEBAPI - INTEGRATION TEST SUITE             ║");
            WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            WriteLine("");

            // PHASE 1: Account Tests
            await RunTestPhase("ACCOUNT TESTS", async () =>
            {
                await RunAccountTests(errors);
            }, errors);

            // PHASE 2: Custom Entity Tests
            await RunTestPhase("CUSTOM ENTITY TESTS", async () =>
            {
                await RunCustomEntityTests(errors);
            }, errors);

            // PHASE 3: Standard Entity Tests (Reflection-Based)
            await RunTestPhase("STANDARD ENTITY TESTS (REFLECTION-BASED)", async () =>
            {
                await RunStandardEntityTests(errors);
            }, errors);

            // Final Report
            stopwatch.Stop();
            WriteLine("");
            WriteLine("═══════════════════════════════════════════════════════════════");
            WriteLine($"Total execution time: {stopwatch.Elapsed.TotalSeconds:F2}s");

            if (errors.Length == 0)
            {
                WriteLine("✓ ALL TESTS PASSED");
            }
            else
            {
                WriteLine("✗ SOME TESTS FAILED - See details below:");
            }

            WriteLine("═══════════════════════════════════════════════════════════════");
            WriteLine("");

            // Assert final result
            Assert.True(errors.Length == 0, errors.ToString());
        }

        /// <summary>
        /// Runs a test phase with error handling and reporting.
        /// </summary>
        private async Task RunTestPhase(string phaseName, Func<Task> testAction, StringBuilder errors)
        {
            WriteLine("");
            WriteLine("───────────────────────────────────────────────────────────────");
            WriteLine($"  {phaseName}");
            WriteLine("───────────────────────────────────────────────────────────────");
            WriteLine("");

            var phaseStopwatch = Stopwatch.StartNew();

            try
            {
                await testAction();
                phaseStopwatch.Stop();
                WriteLine("");
                WriteLine($"✓ {phaseName} completed in {phaseStopwatch.Elapsed.TotalSeconds:F2}s");
            }
            catch (Exception ex)
            {
                phaseStopwatch.Stop();
                WriteLine("");
                WriteLine($"✗ {phaseName} FAILED in {phaseStopwatch.Elapsed.TotalSeconds:F2}s");
                errors.AppendLine($"\n\n═══════════════════════════════════════════════════════════════");
                errors.AppendLine($"ERROR IN {phaseName}:");
                errors.AppendLine("═══════════════════════════════════════════════════════════════");
                errors.AppendLine(ex.ToString());
            }
        }

        /// <summary>
        /// Runs all account-related tests.
        /// </summary>
        private async Task RunAccountTests(StringBuilder errors)
        {
            var accountTests = new AccountTests(Fixture, Output);

            // Registration Test
            try
            {
                await accountTests.AccountRegistration_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN AccountRegistration: {ex.Message}");
            }

            // Login Test
            try
            {
                await accountTests.AccountLogin_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN AccountLogin: {ex.Message}");
            }

            // Password Reset Test
            try
            {
                await accountTests.AccountPasswordReset_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN AccountPasswordReset: {ex.Message}");
            }

            // Default Test User Test
            try
            {
                await accountTests.GetOrCreateDefaultTestUser_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN GetOrCreateDefaultTestUser: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs all custom entity tests.
        /// </summary>
        private async Task RunCustomEntityTests(StringBuilder errors)
        {
            // Category Tests
            try
            {
                var categoryTests = new CategoryTests(Fixture, Output);
                await categoryTests.CategoryCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN CategoryCRUD: {ex.Message}");
            }

            // Tenant Tests
            try
            {
                var tenantTests = new TenantTests(Fixture, Output);
                await tenantTests.TenantCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN TenantCRUD: {ex.Message}");
            }

            // Role Tests
            try
            {
                var roleTests = new RoleTests(Fixture, Output);
                await roleTests.RoleCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN RoleCRUD: {ex.Message}");
            }

            // Other Custom Entity Tests
            var otherTests = new OtherCustomEntityTests(Fixture, Output);

            try
            {
                await otherTests.MediaCategoryCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN MediaCategoryCRUD: {ex.Message}");
            }

            try
            {
                await otherTests.LegalTermCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN LegalTermCRUD: {ex.Message}");
            }

            try
            {
                await otherTests.TemplateCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN TemplateCRUD: {ex.Message}");
            }

            try
            {
                await otherTests.MediaFileCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN MediaFileCRUD: {ex.Message}");
            }

            try
            {
                await otherTests.IntegrationCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN IntegrationCRUD: {ex.Message}");
            }

            try
            {
                await otherTests.SetupCRUD_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN SetupCRUD: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs reflection-based standard entity tests.
        /// </summary>
        private async Task RunStandardEntityTests(StringBuilder errors)
        {
            var standardTests = new StandardEntityTests(Fixture, Output);

            try
            {
                await standardTests.TestAllStandardEntities_JsonApiDbContext_ShouldSucceed();
            }
            catch (Exception ex)
            {
                errors.AppendLine($"\n\nERROR IN StandardEntityTests (JsonApiDbContext): {ex.Message}");
                // Note: StandardEntityTests already accumulates individual entity errors internally
            }
        }
    }
}
