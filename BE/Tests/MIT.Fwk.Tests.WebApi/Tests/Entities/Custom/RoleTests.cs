using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Tests.WebApi.Helpers;
using MIT.Fwk.Tests.WebApi.Infrastructure;
using Xunit.Abstractions;

namespace MIT.Fwk.Tests.WebApi.Tests.Entities.Custom
{
    /// <summary>
    /// Tests for Role entity (MITApplicationRole).
    /// </summary>
    public class RoleTests : IntegrationTestBase
    {
        public RoleTests(TestDatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        public async Task RoleCRUD_ShouldSucceed()
        {
            Role? role = null;

            try
            {
                // Arrange
                role = TestDataBuilder.CreateTestRole(tenantId: 1, level: 50);

                // Act - Create
                Context.Roles.Add(role);
                await Context.SaveChangesAsync();
                var roleId = role.Id;

                Assert.True(roleId != "", "Role ID should be generated");

                // Act - Read (GetAll)
                var allRoles = await Context.Roles 
                    .AsNoTracking()
                    .ToListAsync();
                Assert.NotEmpty(allRoles);

                // Act - Read (GetSingle)
                var retrievedRole = await Context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == roleId);
                Assert.NotNull(retrievedRole);
                Assert.Equal(role.Name, retrievedRole.Name);

                // Act - Update
                role.Name = "Updated Test Role";
                await Context.SaveChangesAsync();

                var updatedRole = await Context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == roleId);
                Assert.NotNull(updatedRole);
                Assert.Equal("Updated Test Role", updatedRole.Name);

                // Act - Delete
                // Note: Original test used custom delete with alternativeCategory parameter
                // In direct DbContext access, we just remove the entity
                Context.Roles.Remove(role);
                await Context.SaveChangesAsync();

                // Assert - Verify deletion
                var deletedRole = await Context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == roleId);
                Assert.Null(deletedRole);

                WriteSuccess("RoleCRUD");
            }
            catch (Exception ex)
            {
                // Cleanup on error
                if (role != null && role.Id != "")
                {
                    try
                    {
                        Context.ChangeTracker.Clear();
                        var entityToDelete = await Context.Roles.FindAsync(role.Id);
                        if (entityToDelete != null)
                        {
                            Context.Roles.Remove(entityToDelete);
                            await Context.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                WriteFailure("RoleCRUD", ex.Message);
                throw;
            }
        }
    }
}
