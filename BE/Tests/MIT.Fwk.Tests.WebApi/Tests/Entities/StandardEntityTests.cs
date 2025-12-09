using MIT.Fwk.Examples.Data;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Entities
{
    /// <summary>
    /// Automated tests for all standard entities with [Resource] attribute.
    /// Uses reflection to discover entities and performs CRUD operations via DbContext.
    /// Tests are executed in a single transaction with dependency-aware ordering.
    /// </summary>
    public class StandardEntityTests : IntegrationTestBase
    {
        public StandardEntityTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        /// <summary>
        /// Tests CRUD operations for all standard [Resource] entities in JsonApiDbContext.
        /// Uses reflection to discover entities and test them automatically.
        /// Executes all tests in a single transaction with proper dependency ordering.
        /// </summary>
        public async Task TestAllStandardEntities_JsonApiDbContext_ShouldSucceed()
        {
            WriteSectionHeader("JsonApiDbContext Entity Tests (Transactional + Dependency-Aware)");

            // Discover all [Resource] entities from JsonApiDbContext
            var entityTypes = EntityReflectionHelper.DiscoverResourceEntities();
            WriteLine($"Found {entityTypes.Count} [Resource] entities in JsonApiDbContext");
            WriteLine("");

            // Create runner and execute tests
            var runner = new TransactionalEntityTestRunner(Context, WriteLine);
            var report = await runner.RunTransactionalCrudTestAsync(entityTypes);

            // Output summary
            WriteLine(report.GetSummary());

            // Assert all tests passed
            if (!report.AllTestsPassed())
            {
                var detailedErrors = report.GetDetailedErrors();
                Assert.Fail($"Some JsonApiDbContext entity tests failed:\n{detailedErrors}");
            }
        }

        /// <summary>
        /// Tests CRUD operations for all [Resource] entities in OtherDbContext (custom example context).
        /// Uses reflection to discover entities and test them automatically.
        /// Executes all tests in a single transaction with proper dependency ordering.
        /// </summary>
        public async Task TestAllStandardEntities_OtherDbContext_ShouldSucceed()
        {
            WriteSectionHeader("OtherDbContext Entity Tests (Transactional + Dependency-Aware)");

            // Get OtherDbContext from DI
            var otherContext = GetService<OtherDbContext>();

            // Discover all [Resource] entities from OtherDbContext
            var entityTypes = EntityReflectionHelper.DiscoverResourceEntities(typeof(OtherDbContext));
            WriteLine($"Found {entityTypes.Count} [Resource] entities in OtherDbContext");
            WriteLine("");

            // Create runner and execute tests
            var runner = new TransactionalEntityTestRunner(otherContext, WriteLine);
            var report = await runner.RunTransactionalCrudTestAsync(entityTypes);

            // Output summary
            WriteLine(report.GetSummary());

            // Assert all tests passed
            if (!report.AllTestsPassed())
            {
                var detailedErrors = report.GetDetailedErrors();
                Assert.Fail($"Some OtherDbContext entity tests failed:\n{detailedErrors}");
            }
        }

    }
}
