namespace MIT.Fwk.Core.Data
{
    /// <summary>
    /// Marker interface for DbContexts that should have migrations applied during startup.
    /// DbContexts implementing this interface will be included in the auto-migration process
    /// when EnableAutoMigrations is true in configuration.
    ///
    /// Generated DbContexts (via CodeGenerator) should NOT implement this interface,
    /// as they connect to external databases where migrations should not be applied.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// - JsonApiDbContext implements IMigrationDbContext (framework-managed, needs migrations)
    /// - OtherDbContext implements IMigrationDbContext (custom module, needs migrations)
    /// - NorthwindDbContext does NOT implement IMigrationDbContext (external DB, read-only)
    /// </remarks>
    public interface IMigrationDbContext
    {
        // Marker interface - no methods required
    }
}
