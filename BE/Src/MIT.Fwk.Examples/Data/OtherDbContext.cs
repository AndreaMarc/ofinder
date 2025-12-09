using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Examples.Entities;

namespace MIT.Fwk.Examples.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public partial class OtherDbContext(DbContextOptions<OtherDbContext> options)
        : DbContext(options), IJsonApiDbContext, IMigrationDbContext
    {
        public static bool _UseSqlServer = false;

        // FASE 7: Helper method to get configuration for design-time scenarios (EF migrations)
        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

        public OtherDbContext() : this(_UseSqlServer ? new DbContextOptionsBuilder<OtherDbContext>()
                .UseSqlServer(GetConfiguration().GetConnectionString(nameof(OtherDbContext)))
                .Options : new DbContextOptionsBuilder<OtherDbContext>()
                .UseMySQL(GetConfiguration().GetConnectionString(nameof(OtherDbContext)))
                .Options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_UseSqlServer)
            {
                optionsBuilder.UseSqlServer(
                    GetConfiguration().GetConnectionString(nameof(OtherDbContext)),
                    options => options.MigrationsHistoryTable($"__EFMigrationsHistory_{nameof(OtherDbContext)}"));
            }
            else
            {
                optionsBuilder.UseMySQL(
                    GetConfiguration().GetConnectionString(nameof(OtherDbContext)),
                    options => options.MigrationsHistoryTable($"__EFMigrationsHistory_{nameof(OtherDbContext)}"));
            }
        }

        public DbSet<ExampleCategory> ExampleCategories => Set<ExampleCategory>();

        public DbSet<ExampleProduct> ExampleProducts => Set<ExampleProduct>();

    }
}
