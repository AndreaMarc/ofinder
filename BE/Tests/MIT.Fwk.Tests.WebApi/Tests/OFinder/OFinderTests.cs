using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.OFinder
{
    /// <summary>
    /// Comprehensive CRUD tests for all OFinder entities (Performer*, Channel*).
    /// Uses TransactionalEntityTestRunner for automatic dependency resolution and rollback.
    /// Database state remains unchanged after test execution.
    /// </summary>
    public class OFinderTests : IntegrationTestBase
    {
        public OFinderTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        /// <summary>
        /// Master test: Tests CRUD operations for all OFinder entities with automatic dependency handling.
        /// Execution order: User(seed) → Performer → [Channel/PerformerService/etc.] → [ChannelSchedule/etc.]
        /// All operations execute within a transaction that rolls back at the end.
        /// Database remains unchanged after test completion.
        /// </summary>
        [Fact]
        public async Task TestAllOFinderEntities_ShouldSucceed()
        {
            WriteSectionHeader("OFinder Entity Tests (Transactional + Dependency-Aware)");

            // Define all OFinder entity types to test
            var ofinderEntityTypes = new List<Type>
            {
                // Tier 1: Root entities (depend on User)
                typeof(Performer),

                // Tier 2: Performer children
                typeof(Channel),
                typeof(PerformerService),
                typeof(PerformerView),
                typeof(PerformerReview),
                typeof(UserFavorite),

                // Tier 3: Channel children
                typeof(ChannelSchedule),
                typeof(ChannelContentType),
                typeof(ChannelPricing)
            };

            WriteLine($"Testing {ofinderEntityTypes.Count} OFinder entities");
            WriteLine("");

            // Create runner with UserManager for User seed support
            var runner = new TransactionalEntityTestRunner(Context, WriteLine, UserManager);
            var report = await runner.RunTransactionalCrudTestAsync(ofinderEntityTypes);

            // Output summary
            WriteLine(report.GetSummary());

            // Assert all tests passed
            if (!report.AllTestsPassed())
            {
                var detailedErrors = report.GetDetailedErrors();
                Assert.Fail($"Some OFinder entity tests failed:\n{detailedErrors}");
            }
        }

        /// <summary>
        /// Individual test for Performer entity with custom controller logic.
        /// Tests Performer-specific business logic not covered by automatic CRUD.
        /// Note: Performer does not have [Resource] attribute - uses custom controller.
        /// </summary>
        [Fact]
        public async Task Performer_CustomLogic_ShouldSucceed()
        {
            WriteSectionHeader("Performer Custom Logic Test");

            MITApplicationUser? testUser = null;
            Performer? performer = null;

            try
            {
                // Create test user (prerequisite for Performer)
                var authHelper = new AuthHelper(UserManager, RoleManager, Configuration,
                    GetService<Microsoft.Extensions.Options.IOptions<MIT.Fwk.Core.Options.JwtOptions>>());
                testUser = await authHelper.CreateTestUserAsync(tenantId: 1);

                // Create performer
                performer = TestDataBuilder.CreateTestPerformer(testUser.Id);
                Context.Performers.Add(performer);
                await Context.SaveChangesAsync();

                WriteLine($"Created Performer: Id={performer.Id}, UserId={performer.UserId}");

                // Test: Read performer
                var retrievedPerformer = await Context.Performers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == performer.Id);

                Assert.NotNull(retrievedPerformer);
                Assert.Equal(performer.UserId, retrievedPerformer.UserId);
                Assert.Equal(performer.Description, retrievedPerformer.Description);
                Assert.True(retrievedPerformer.IsActive);
                Assert.False(retrievedPerformer.IsVerified);

                WriteLine($"✓ Performer retrieved successfully");

                // Test: Update performer
                performer.Description = "Updated test description";
                performer.IsVerified = true;
                await Context.SaveChangesAsync();

                var updatedPerformer = await Context.Performers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == performer.Id);

                Assert.Equal("Updated test description", updatedPerformer?.Description);
                Assert.True(updatedPerformer?.IsVerified);

                WriteLine($"✓ Performer updated successfully");

                WriteSuccess("Performer_CustomLogic");
            }
            finally
            {
                // Cleanup in reverse dependency order
                if (performer != null)
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.Performers.FindAsync(performer.Id);
                        if (entityToDelete != null)
                        {
                            Context.Performers.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                            WriteLine($"✓ Cleaned up Performer (Id={performer.Id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"✗ Failed to cleanup Performer: {ex.Message}");
                    }
                }

                if (testUser != null)
                {
                    try
                    {
                        await UserManager.DeleteAsync(testUser);
                        WriteLine($"✓ Cleaned up User (Id={testUser.Id})");
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"✗ Failed to cleanup User: {ex.Message}");
                    }
                }

                Context.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Individual test for PerformerReview entity (no [Resource] attribute).
        /// Tests custom controller logic for reviews and validates unique constraint.
        /// Unique constraint: (PerformerId, UserId) - one review per user per performer.
        /// </summary>
        [Fact]
        public async Task PerformerReview_CustomLogic_ShouldSucceed()
        {
            WriteSectionHeader("PerformerReview Custom Logic Test");

            MITApplicationUser? testUser = null;
            Performer? performer = null;
            PerformerReview? review = null;

            try
            {
                // Create test user
                var authHelper = new AuthHelper(UserManager, RoleManager, Configuration,
                    GetService<Microsoft.Extensions.Options.IOptions<MIT.Fwk.Core.Options.JwtOptions>>());
                testUser = await authHelper.CreateTestUserAsync(tenantId: 1);

                // Create performer
                performer = TestDataBuilder.CreateTestPerformer(testUser.Id);
                Context.Performers.Add(performer);
                await Context.SaveChangesAsync();

                WriteLine($"Created Performer: Id={performer.Id}");

                // Create review
                review = TestDataBuilder.CreateTestPerformerReview(performer.Id, testUser.Id);
                Context.PerformerReviews.Add(review);
                await Context.SaveChangesAsync();

                WriteLine($"Created PerformerReview: Id={review.Id}, Rating={review.Rating}");

                // Test: Read review
                var retrievedReview = await Context.PerformerReviews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == review.Id);

                Assert.NotNull(retrievedReview);
                Assert.Equal(review.PerformerId, retrievedReview.PerformerId);
                Assert.Equal(review.UserId, retrievedReview.UserId);
                Assert.Equal(review.Rating, retrievedReview.Rating);
                Assert.Equal(review.ReviewText, retrievedReview.ReviewText);
                Assert.True(retrievedReview.IsVerifiedPurchase);

                WriteLine($"✓ PerformerReview retrieved successfully");

                // Test: Update review
                review.Rating = 5;
                review.ReviewText = "Excellent performer!";
                await Context.SaveChangesAsync();

                var updatedReview = await Context.PerformerReviews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == review.Id);

                Assert.Equal(5, updatedReview?.Rating);
                Assert.Equal("Excellent performer!", updatedReview?.ReviewText);

                WriteLine($"✓ PerformerReview updated successfully");

                WriteSuccess("PerformerReview_CustomLogic");
            }
            finally
            {
                // Cleanup in reverse dependency order: Review → Performer → User
                if (review != null)
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.PerformerReviews.FindAsync(review.Id);
                        if (entityToDelete != null)
                        {
                            Context.PerformerReviews.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                            WriteLine($"✓ Cleaned up PerformerReview (Id={review.Id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"✗ Failed to cleanup PerformerReview: {ex.Message}");
                    }
                }

                if (performer != null)
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.Performers.FindAsync(performer.Id);
                        if (entityToDelete != null)
                        {
                            Context.Performers.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                            WriteLine($"✓ Cleaned up Performer (Id={performer.Id})");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"✗ Failed to cleanup Performer: {ex.Message}");
                    }
                }

                if (testUser != null)
                {
                    try
                    {
                        await UserManager.DeleteAsync(testUser);
                        WriteLine($"✓ Cleaned up User (Id={testUser.Id})");
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"✗ Failed to cleanup User: {ex.Message}");
                    }
                }

                Context.ChangeTracker.Clear();
            }
        }
    }
}
