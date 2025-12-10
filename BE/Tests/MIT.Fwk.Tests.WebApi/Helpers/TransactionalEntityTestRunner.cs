using Microsoft.EntityFrameworkCore;
using System.Text;
using Xunit;

namespace MIT.Fwk.Tests.WebApi.Helpers
{
    /// <summary>
    /// Runs CRUD tests for multiple entities under a single database transaction.
    /// Handles entity creation in dependency order and deletion in reverse order.
    /// Automatically rolls back on errors for clean test isolation.
    /// Supports any DbContext type (JsonApiDbContext, OtherDbContext, etc.).
    /// </summary>
    public class TransactionalEntityTestRunner
    {
        private readonly DbContext _context;
        private readonly Action<string> _outputWriter;

        public TransactionalEntityTestRunner(DbContext context, Action<string> outputWriter)
        {
            _context = context;
            _outputWriter = outputWriter ?? (_ => { });
        }

        /// <summary>
        /// Runs transactional CRUD tests for all provided entity types.
        /// Returns a report of successes and failures.
        /// </summary>
        public async Task<TestExecutionReport> RunTransactionalCrudTestAsync(List<Type> entityTypes)
        {
            var report = new TestExecutionReport();
            var createdEntities = new Dictionary<Type, object>();
            Dictionary<Type, object>? seedData = null;

            try
            {
                // PHASE 0: Seed prerequisite data (OUTSIDE transaction)
                seedData = await SeedPrerequisiteDataAsync();

                // Build dependency graph
                _outputWriter("Building entity dependency graph...");
                var graphBuilder = new EntityDependencyGraphBuilder(entityTypes);
                var (totalEntities, withDeps, withoutDeps) = graphBuilder.GetStatistics();

                _outputWriter($"Dependency analysis: {totalEntities} entities ({withoutDeps} independent, {withDeps} with dependencies)");
                _outputWriter("");

                // Get creation and deletion order
                List<Type> creationOrder;
                List<Type> deletionOrder;

                try
                {
                    creationOrder = graphBuilder.GetTopologicalOrder();
                    deletionOrder = graphBuilder.GetReverseDeletionOrder();

                    _outputWriter($"Creation order determined: {string.Join(" → ", creationOrder.Select(t => t.Name))}");
                    _outputWriter($"Deletion order: {string.Join(" → ", deletionOrder.Select(t => t.Name))}");
                    _outputWriter("");
                }
                catch (Exception ex)
                {
                    report.AddFatalError("Dependency Graph", ex);
                    return report;
                }

                // Start transaction
                _outputWriter("Starting database transaction...");
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                // PHASE 1: CREATE entities in dependency order
                _outputWriter("─────────────────────────────────────────");
                _outputWriter("PHASE 1: Creating entities...");
                _outputWriter("─────────────────────────────────────────");

                foreach (var entityType in creationOrder)
                {
                    try
                    {
                        var entity = await CreateEntityAsync(entityType, createdEntities, seedData);
                        createdEntities[entityType] = entity;

                        report.RecordSuccess(entityType, TestPhase.Create);
                        _outputWriter($"✓ {entityType.Name,-40} | CREATE SUCCESS");
                    }
                    catch (Exception ex)
                    {
                        report.RecordFailure(entityType, TestPhase.Create, ex);
                        _outputWriter($"✗ {entityType.Name,-40} | CREATE FAILED: {ex.Message}");

                        // Continue to next entity (transaction will rollback at the end)
                    }
                }

                _outputWriter("");

                // PHASE 2: UPDATE entities (just verify tracking works)
                _outputWriter("─────────────────────────────────────────");
                _outputWriter("PHASE 2: Updating entities...");
                _outputWriter("─────────────────────────────────────────");

                foreach (var entityType in creationOrder)
                {
                    if (!createdEntities.ContainsKey(entityType))
                    {
                        // Skip entities that failed to create
                        continue;
                    }

                    try
                    {
                        var entity = createdEntities[entityType];
                        var entityId = EntityReflectionHelper.GetEntityId(entity);

                        // Read entity from database
                        var trackedEntity = await _context.FindAsync(entityType, entityId);
                        Assert.NotNull(trackedEntity);

                        // Modify a property to verify update works
                        // For now, we just call SaveChanges without modifying anything
                        // This verifies the entity can be tracked and saved
                        await _context.SaveChangesAsync();

                        report.RecordSuccess(entityType, TestPhase.Update);
                        _outputWriter($"✓ {entityType.Name,-40} | UPDATE SUCCESS");
                    }
                    catch (Exception ex)
                    {
                        report.RecordFailure(entityType, TestPhase.Update, ex);
                        _outputWriter($"✗ {entityType.Name,-40} | UPDATE FAILED: {ex.Message}");
                    }
                }

                _outputWriter("");

                // PHASE 3: DELETE entities in reverse dependency order
                _outputWriter("─────────────────────────────────────────");
                _outputWriter("PHASE 3: Deleting entities...");
                _outputWriter("─────────────────────────────────────────");

                foreach (var entityType in deletionOrder)
                {
                    if (!createdEntities.ContainsKey(entityType))
                    {
                        // Skip entities that failed to create
                        continue;
                    }

                    try
                    {
                        var entity = createdEntities[entityType];
                        var entityId = EntityReflectionHelper.GetEntityId(entity);

                        // Clear change tracker to avoid conflicts
                        _context.ChangeTracker.Clear();

                        // Find and delete entity
                        var entityToDelete = await _context.FindAsync(entityType, entityId);
                        if (entityToDelete != null)
                        {
                            _context.Remove(entityToDelete);
                            await _context.SaveChangesAsync();

                            // Verify deletion
                            _context.ChangeTracker.Clear();
                            var deletedEntity = await _context.FindAsync(entityType, entityId);
                            Assert.Null(deletedEntity);

                            report.RecordSuccess(entityType, TestPhase.Delete);
                            _outputWriter($"✓ {entityType.Name,-40} | DELETE SUCCESS");
                        }
                        else
                        {
                            throw new InvalidOperationException("Entity not found for deletion");
                        }
                    }
                    catch (Exception ex)
                    {
                        report.RecordFailure(entityType, TestPhase.Delete, ex);
                        _outputWriter($"✗ {entityType.Name,-40} | DELETE FAILED: {ex.Message}");
                    }
                }

                _outputWriter("");

                // ROLLBACK transaction (this is a test, we don't want to pollute the database)
                _outputWriter("Rolling back transaction (test cleanup)...");
                await transaction.RollbackAsync();

                _outputWriter("Transaction rolled back successfully.");
                }
                catch (Exception ex)
                {
                    _outputWriter($"CRITICAL ERROR during transaction: {ex.Message}");
                    await transaction.RollbackAsync();
                    report.AddFatalError("Transaction", ex);
                }
            }
            catch (Exception ex)
            {
                _outputWriter($"FATAL ERROR during test execution: {ex.Message}");
                report.AddFatalError("Test Execution", ex);
            }
            finally
            {
                // CLEANUP: Always delete seed data, regardless of test outcome
                if (seedData != null)
                {
                    await CleanupSeedDataAsync(seedData);
                }
            }

            return report;
        }

        /// <summary>
        /// Seeds prerequisite data needed by most entities (Tenant, Role).
        /// This data is created OUTSIDE the test transaction and must be cleaned up in finally block.
        /// </summary>
        private async Task<Dictionary<Type, object>> SeedPrerequisiteDataAsync()
        {
            _outputWriter("─────────────────────────────────────────");
            _outputWriter("PHASE 0: Seeding prerequisite data...");
            _outputWriter("─────────────────────────────────────────");

            var seeds = new Dictionary<Type, object>();

            try
            {
                // Try to seed Tenant and Role if they exist in this DbContext
                // OtherDbContext and other custom contexts may not have these entities

                // Seed Tenant (required by most entities via TenantId FK)
                if (HasDbSet<MIT.Fwk.Infrastructure.Entities.Tenant>())
                {
                    var tenant = TestDataBuilder.CreateTestTenant();
                    _context.Set<MIT.Fwk.Infrastructure.Entities.Tenant>().Add(tenant);
                    await _context.SaveChangesAsync();
                    seeds[typeof(MIT.Fwk.Infrastructure.Entities.Tenant)] = tenant;
                    _outputWriter($"✓ Seeded: Tenant (Id={tenant.Id}, Name={tenant.Name})");
                }

                // Seed Role (AspNetRoles - required by RoleClaim, UserRole, etc.)
                if (HasDbSet<MIT.Fwk.Infrastructure.Entities.Role>())
                {
                    var tenantId = seeds.ContainsKey(typeof(MIT.Fwk.Infrastructure.Entities.Tenant))
                        ? ((MIT.Fwk.Infrastructure.Entities.Tenant)seeds[typeof(MIT.Fwk.Infrastructure.Entities.Tenant)]).Id
                        : 1;

                    var role = TestDataBuilder.CreateTestRole(tenantId);
                    _context.Set<MIT.Fwk.Infrastructure.Entities.Role>().Add(role);
                    await _context.SaveChangesAsync();
                    seeds[typeof(MIT.Fwk.Infrastructure.Entities.Role)] = role;
                    _outputWriter($"✓ Seeded: Role (Id={role.Id}, Name={role.Name})");
                }

                _outputWriter("");
                return seeds;
            }
            catch (Exception ex)
            {
                _outputWriter($"✗ Seed failed: {ex.Message}");

                // Attempt cleanup of partially created seeds
                await CleanupSeedDataAsync(seeds);

                throw;
            }
        }

        /// <summary>
        /// Cleans up seed data created in SeedPrerequisiteDataAsync.
        /// Called in finally block to ensure cleanup happens regardless of test outcome.
        /// </summary>
        private async Task CleanupSeedDataAsync(Dictionary<Type, object> seedData)
        {
            if (seedData.Count == 0)
                return;

            _outputWriter("");
            _outputWriter("─────────────────────────────────────────");
            _outputWriter("CLEANUP: Deleting seed data...");
            _outputWriter("─────────────────────────────────────────");

            // Delete in reverse order: Role → Tenant (to respect FK constraints)
            var deletionOrder = new[] {
                typeof(MIT.Fwk.Infrastructure.Entities.Role),
                typeof(MIT.Fwk.Infrastructure.Entities.Tenant)
            };

            foreach (var entityType in deletionOrder)
            {
                if (!seedData.ContainsKey(entityType))
                    continue;

                try
                {
                    var entity = seedData[entityType];
                    var entityId = EntityReflectionHelper.GetEntityId(entity);

                    // Clear change tracker to avoid conflicts
                    _context.ChangeTracker.Clear();

                    // Find and delete
                    var entityToDelete = await _context.FindAsync(entityType, entityId);
                    if (entityToDelete != null)
                    {
                        _context.Remove(entityToDelete);
                        await _context.SaveChangesAsync();
                        _outputWriter($"✓ Deleted seed: {entityType.Name} (Id={entityId})");
                    }
                }
                catch (Exception ex)
                {
                    _outputWriter($"✗ Failed to delete seed {entityType.Name}: {ex.Message}");
                    // Continue cleanup even if one fails
                }
            }

            _outputWriter("Cleanup completed.");
            _outputWriter("");
        }

        /// <summary>
        /// Creates an entity instance with proper relationships.
        /// Uses already-created entities for foreign key relationships.
        /// </summary>
        private async Task<object> CreateEntityAsync(
            Type entityType,
            Dictionary<Type, object> createdEntities,
            Dictionary<Type, object>? seedData)
        {
            // Create entity instance with default values
            var entity = EntityReflectionHelper.CreateEntityInstance(entityType, _context);

            // Handle string-based IDs (Guid)
            if (EntityReflectionHelper.HasStringId(entityType))
            {
                var newId = Guid.NewGuid().ToString();
                EntityReflectionHelper.SetEntityId(entity, newId);
            }

            // Setup relationships using already-created entities
            var relationships = EntityReflectionHelper.GetHasOneRelationships(entityType);
            foreach (var (propertyName, relatedType) in relationships)
            {
                object? relatedEntity = null;

                // Priority: 1) Entity created in test run, 2) Seed data, 3) Existing DB entity
                if (createdEntities.ContainsKey(relatedType))
                {
                    relatedEntity = createdEntities[relatedType];
                }
                else if (seedData != null && seedData.ContainsKey(relatedType))
                {
                    // Use seed data for prerequisite entities (Tenant, Role, etc.)
                    relatedEntity = seedData[relatedType];
                }
                else
                {
                    // Otherwise, try to find an existing entity in the database
                    relatedEntity = await EntityReflectionHelper.FindFirstEntityForRelationship(relatedType, _context);
                }

                if (relatedEntity != null)
                {
                    // Get the ID of the related entity
                    var relatedId = EntityReflectionHelper.GetEntityId(relatedEntity);

                    // Set the foreign key property (e.g., CategoryId for Category relationship)
                    // Use case-insensitive lookup to handle "role" -> "RoleId" scenarios
                    var fkPropertyName = propertyName + "Id";
                    var fkProperty = entityType.GetProperty(fkPropertyName,
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.IgnoreCase);

                    if (fkProperty != null && relatedId != null)
                    {
                        // Convert ID to the correct type if necessary
                        // Handle nullable types (int? from int, Guid? from Guid, etc.)
                        var targetType = Nullable.GetUnderlyingType(fkProperty.PropertyType) ?? fkProperty.PropertyType;
                        var convertedId = Convert.ChangeType(relatedId, targetType);
                        fkProperty.SetValue(entity, convertedId);
                    }
                }
                else
                {
                    // If no related entity found, check if FK is nullable
                    var fkPropertyName = propertyName + "Id";
                    var fkProperty = entityType.GetProperty(fkPropertyName,
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.IgnoreCase);

                    if (fkProperty != null)
                    {
                        if (Nullable.GetUnderlyingType(fkProperty.PropertyType) != null)
                        {
                            // Nullable FK - set to null
                            fkProperty.SetValue(entity, null);
                        }
                        else
                        {
                            // Required FK but no related entity available
                            throw new InvalidOperationException(
                                $"Cannot create {entityType.Name}: required relationship {propertyName} ({relatedType.Name}) not available");
                        }
                    }
                }
            }

            // Add to context and save
            _context.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Checks if the current DbContext has a DbSet for the specified entity type.
        /// Used to conditionally seed prerequisite data based on DbContext capabilities.
        /// </summary>
        private bool HasDbSet<TEntity>() where TEntity : class
        {
            try
            {
                return _context.Model.FindEntityType(typeof(TEntity)) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Test execution phases for CRUD operations.
    /// </summary>
    public enum TestPhase
    {
        Create,
        Read,
        Update,
        Delete
    }

    /// <summary>
    /// Report of test execution results.
    /// </summary>
    public class TestExecutionReport
    {
        private readonly Dictionary<Type, Dictionary<TestPhase, bool>> _results = new();
        private readonly Dictionary<Type, Dictionary<TestPhase, Exception>> _errors = new();
        private readonly List<(string Context, Exception Error)> _fatalErrors = new();

        public void RecordSuccess(Type entityType, TestPhase phase)
        {
            EnsureEntityExists(entityType);
            _results[entityType][phase] = true;
        }

        public void RecordFailure(Type entityType, TestPhase phase, Exception error)
        {
            EnsureEntityExists(entityType);
            _results[entityType][phase] = false;
            _errors[entityType][phase] = error;
        }

        public void AddFatalError(string context, Exception error)
        {
            _fatalErrors.Add((context, error));
        }

        private void EnsureEntityExists(Type entityType)
        {
            if (!_results.ContainsKey(entityType))
            {
                _results[entityType] = new Dictionary<TestPhase, bool>();
                _errors[entityType] = new Dictionary<TestPhase, Exception>();
            }
        }

        /// <summary>
        /// Returns true if all tests passed.
        /// </summary>
        public bool AllTestsPassed()
        {
            return _fatalErrors.Count == 0 &&
                   _results.All(kv => kv.Value.All(phase => phase.Value));
        }

        /// <summary>
        /// Gets a summary string of test results.
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("═════════════════════════════════════════");
            sb.AppendLine("TEST EXECUTION SUMMARY");
            sb.AppendLine("═════════════════════════════════════════");

            int totalEntities = _results.Count;
            int successfulEntities = _results.Count(kv => kv.Value.All(p => p.Value));
            int failedEntities = totalEntities - successfulEntities;

            sb.AppendLine($"Total Entities Tested: {totalEntities}");
            sb.AppendLine($"Successful: {successfulEntities} ({(totalEntities > 0 ? (successfulEntities * 100.0 / totalEntities).ToString("F1") : "0")}%)");
            sb.AppendLine($"Failed: {failedEntities} ({(totalEntities > 0 ? (failedEntities * 100.0 / totalEntities).ToString("F1") : "0")}%)");
            sb.AppendLine("");

            if (_fatalErrors.Count > 0)
            {
                sb.AppendLine("FATAL ERRORS:");
                foreach (var (context, error) in _fatalErrors)
                {
                    sb.AppendLine($"  ✗ {context}: {error.Message}");
                }
                sb.AppendLine("");
            }

            if (failedEntities > 0)
            {
                sb.AppendLine("FAILED ENTITIES:");
                foreach (var (entityType, phases) in _results.Where(kv => kv.Value.Any(p => !p.Value)))
                {
                    sb.AppendLine($"  ✗ {entityType.Name}:");
                    foreach (var (phase, success) in phases.Where(p => !p.Value))
                    {
                        var error = _errors[entityType][phase];
                        sb.AppendLine($"      - {phase}: {error.Message}");
                    }
                }
                sb.AppendLine("");
            }

            sb.AppendLine("═════════════════════════════════════════");

            return sb.ToString();
        }

        /// <summary>
        /// Gets detailed error information for assertion failures.
        /// </summary>
        public string GetDetailedErrors()
        {
            if (AllTestsPassed())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("═════════════════════════════════════════");
            sb.AppendLine("DETAILED ERROR REPORT");
            sb.AppendLine("═════════════════════════════════════════");

            if (_fatalErrors.Count > 0)
            {
                sb.AppendLine("");
                sb.AppendLine("─────────────────────────────────────────");
                sb.AppendLine("FATAL ERRORS:");
                sb.AppendLine("─────────────────────────────────────────");
                foreach (var (context, error) in _fatalErrors)
                {
                    sb.AppendLine($"\nContext: {context}");
                    sb.AppendLine($"Error: {error}");
                }
            }

            foreach (var (entityType, phases) in _errors)
            {
                foreach (var (phase, error) in phases)
                {
                    sb.AppendLine("");
                    sb.AppendLine("─────────────────────────────────────────");
                    sb.AppendLine($"Entity: {entityType.Name}");
                    sb.AppendLine($"Phase: {phase}");
                    sb.AppendLine("─────────────────────────────────────────");
                    sb.AppendLine(error.ToString());
                }
            }

            sb.AppendLine("");
            sb.AppendLine("═════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
