using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Entities.Custom
{
    /// <summary>
    /// Tests for Tenant entity.
    /// </summary>
    public class TenantTests : IntegrationTestBase
    {
        public TenantTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        public async Task TenantCRUD_ShouldSucceed()
        {
            Tenant? tenant = null;

            try
            {
                // Arrange
                tenant = TestDataBuilder.CreateTestTenant();

                // Act - Create
                Context.Tenants.Add(tenant);
                await Context.SaveChangesAsync();
                var tenantId = tenant.Id;

                Assert.True(tenantId > 0, "Tenant ID should be generated");

                // Act - Read (GetAll)
                var allTenants = await Context.Tenants
                    .AsNoTracking()
                    .ToListAsync();
                Assert.NotEmpty(allTenants);

                // Act - Read (GetSingle)
                var retrievedTenant = await Context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);
                Assert.NotNull(retrievedTenant);
                Assert.Equal(tenant.Name, retrievedTenant.Name);

                // Act - Update
                tenant.Name = "Updated Test Tenant";
                await Context.SaveChangesAsync();

                var updatedTenant = await Context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);
                Assert.NotNull(updatedTenant);
                Assert.Equal("Updated Test Tenant", updatedTenant.Name);

                // Act - Delete
                Context.Tenants.Remove(tenant);
                await Context.SaveChangesAsync();

                // Assert - Verify deletion
                var deletedTenant = await Context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId);
                Assert.Null(deletedTenant);

                WriteSuccess("TenantCRUD");
            }
            catch (Exception ex)
            {
                // Cleanup on error
                if (tenant != null && tenant.Id > 0)
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.Tenants.FindAsync(tenant.Id);
                        if (entityToDelete != null)
                        {
                            Context.Tenants.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                WriteFailure("TenantCRUD", ex.Message);
                throw;
            }
            finally
            {
                Context.ChangeTracker.Clear();
            }
        }
    }
}
