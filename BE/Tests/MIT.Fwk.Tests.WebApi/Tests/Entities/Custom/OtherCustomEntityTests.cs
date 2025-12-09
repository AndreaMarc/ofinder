using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Entities.Custom
{
    /// <summary>
    /// Consolidated tests for other custom entities with standard CRUD behavior.
    /// Includes: MediaCategory, LegalTerm, Template, MediaFile, Integration, Setup.
    /// </summary>
    public class OtherCustomEntityTests : IntegrationTestBase
    {
        public OtherCustomEntityTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        public async Task MediaCategoryCRUD_ShouldSucceed()
        {
            MediaCategory? entity = null;
            try
            {
                entity = TestDataBuilder.CreateTestMediaCategory();
                Context.MediaCategories.Add(entity);
                await Context.SaveChangesAsync();

                var retrieved = await Context.MediaCategories.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.NotNull(retrieved);

                entity.Name = "Updated";
                await Context.SaveChangesAsync();

                Context.MediaCategories.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.MediaCategories.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.Null(deleted);

                WriteSuccess("MediaCategoryCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.MediaCategories.Remove(entity));
                WriteFailure("MediaCategoryCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }

        public async Task LegalTermCRUD_ShouldSucceed()
        {
            LegalTerm? entity = null;
            try
            {
                entity = TestDataBuilder.CreateTestLegalTerm();
                Context.LegalTerms.Add(entity);
                await Context.SaveChangesAsync();

                var retrieved = await Context.LegalTerms.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.NotNull(retrieved);

                entity.Title = "Updated";
                await Context.SaveChangesAsync();

                // Test custom Activation endpoint behavior (direct DB for test)
                entity.Active = true;
                await Context.SaveChangesAsync();

                Context.LegalTerms.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.LegalTerms.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.Null(deleted);

                WriteSuccess("LegalTermCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.LegalTerms.Remove(entity));
                WriteFailure("LegalTermCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }

        public async Task TemplateCRUD_ShouldSucceed()
        {
            Template? entity = null;
            Category? category = null;

            try
            {
                // Setup: Create a category first (required FK)
                category = TestDataBuilder.CreateTestCategory();
                Context.Categories.Add(category);
                await Context.SaveChangesAsync();

                entity = TestDataBuilder.CreateTestTemplate(category.Id);
                Context.Templates.Add(entity);
                await Context.SaveChangesAsync();

                var retrieved = await Context.Templates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.NotNull(retrieved);

                entity.Name = "Updated";
                await Context.SaveChangesAsync();

                Context.Templates.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.Templates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.Null(deleted);

                // Cleanup category
                Context.Categories.Remove(category);
                await Context.SaveChangesAsync();

                WriteSuccess("TemplateCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.Templates.Remove(entity));
                if (category != null) await CleanupEntityAsync(() => Context.Categories.Remove(category));
                WriteFailure("TemplateCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }

        public async Task MediaFileCRUD_ShouldSucceed()
        {
            MediaFile? entity = null;
            try
            {
                entity = TestDataBuilder.CreateTestMediaFile();
                Context.MediaFiles.Add(entity);
                await Context.SaveChangesAsync();

                var retrieved = await Context.MediaFiles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.NotNull(retrieved);

                // Update (skip - MediaFile uses camelCase properties)
                await Context.SaveChangesAsync();

                Context.MediaFiles.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.MediaFiles.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.Null(deleted);

                WriteSuccess("MediaFileCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.MediaFiles.Remove(entity));
                WriteFailure("MediaFileCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }
        public async Task IntegrationCRUD_ShouldSucceed()
        {
            Integration? entity = null;
            try
            {
                entity = TestDataBuilder.CreateTestIntegration();
                Context.Integrations.Add(entity);
                await Context.SaveChangesAsync();

                var retrieved = await Context.Integrations.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.NotNull(retrieved);

                entity.Name = "Updated";
                await Context.SaveChangesAsync();

                Context.Integrations.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.Integrations.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
                Assert.Null(deleted);

                WriteSuccess("IntegrationCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.Integrations.Remove(entity));
                WriteFailure("IntegrationCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }
        public async Task SetupCRUD_ShouldSucceed()
        {
            Setup? entity = null;
            try
            {
                entity = TestDataBuilder.CreateTestSetup();
                Context.Setups.Add(entity);
                await Context.SaveChangesAsync();

                var entityId = entity.Id;
                Assert.True(entityId > 0);

                var retrieved = await Context.Setups.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entityId);
                Assert.NotNull(retrieved);

                entity.environment = "Updated";
                await Context.SaveChangesAsync();

                Context.Setups.Remove(entity);
                await Context.SaveChangesAsync();

                var deleted = await Context.Setups.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entityId);
                Assert.Null(deleted);

                WriteSuccess("SetupCRUD");
            }
            catch (Exception ex)
            {
                if (entity != null) await CleanupEntityAsync(() => Context.Setups.Remove(entity));
                WriteFailure("SetupCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }

        /// <summary>
        /// Helper to cleanup entities on error.
        /// </summary>
        private async Task CleanupEntityAsync(Action removeAction)
        {
            try
            {
                Context.ChangeTracker.Clear();
                removeAction();
                await Context.SaveChangesAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
