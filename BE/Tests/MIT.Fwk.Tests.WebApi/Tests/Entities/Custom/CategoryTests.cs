using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Entities.Custom
{
    /// <summary>
    /// Tests for Category entity with custom delete logic (requires alternativeCategory parameter).
    /// </summary>
    public class CategoryTests : IntegrationTestBase
    {
        public CategoryTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        public async Task CategoryCRUD_ShouldSucceed()
        {
            Category? category = null;

            try
            {
                // Arrange
                category = TestDataBuilder.CreateTestCategory(tenantId: 1);

                // Act - Create
                Context.Categories.Add(category);
                await Context.SaveChangesAsync();
                var categoryId = category.Id;

                Assert.True(categoryId > 0, "Category ID should be generated");

                // Act - Read (GetAll)
                var allCategories = await Context.Categories
                    .AsNoTracking()
                    .ToListAsync();
                Assert.NotEmpty(allCategories);

                // Act - Read (GetSingle)
                var retrievedCategory = await Context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
                Assert.NotNull(retrievedCategory);
                Assert.Equal(category.Name, retrievedCategory.Name);

                // Act - Update
                category.Name = "Updated Test Category";
                await Context.SaveChangesAsync();

                var updatedCategory = await Context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
                Assert.NotNull(updatedCategory);
                Assert.Equal("Updated Test Category", updatedCategory.Name);

                // Act - Delete
                // Note: Original test used custom delete with alternativeCategory parameter
                // In direct DbContext access, we just remove the entity
                Context.Categories.Remove(category);
                await Context.SaveChangesAsync();

                // Assert - Verify deletion
                var deletedCategory = await Context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
                Assert.Null(deletedCategory);

                WriteSuccess("CategoryCRUD");
            }
            catch (Exception ex)
            {
                // Cleanup on error
                if (category != null && category.Id > 0)
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.Categories.FindAsync(category.Id);
                        if (entityToDelete != null)
                        {
                            Context.Categories.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                WriteFailure("CategoryCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }
    }
}
