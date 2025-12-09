using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MIT.Fwk.Infrastructure.Entities;
using MySql.EntityFrameworkCore.Extensions;
using System;

namespace MIT.Fwk.Infrastructure.Data
{
    /// <summary>
    /// DbContext dedicato per ASP.NET Core Identity entities.
    /// NON implementa IJsonApiDbContext - no auto-discovery.
    /// NON partecipa alle migrations - tabelle già create da JsonApiDbContext/Identity.
    /// Usa stessa connection string di JsonApiDbContext.
    /// </summary>
    public class MITApplicationDbContext : IdentityDbContext<
        MITApplicationUser,
        MITApplicationRole,
        string>
    {
        public static bool _UseSqlServer = true;

        public MITApplicationDbContext(DbContextOptions<MITApplicationDbContext> options)
            : base(options)
        {
        }

        // Parameterless constructor for design-time (needed by tests)
        public MITApplicationDbContext() : this(CreateDesignTimeOptions())
        {
        }

        private static DbContextOptions<MITApplicationDbContext> CreateDesignTimeOptions()
        {
            var config = GetConfiguration();
            string connectionString = config.GetConnectionString("JsonApiDbContext")
                ?? config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found for MITApplicationDbContext");

            if (_UseSqlServer)
            {
                return new DbContextOptionsBuilder<MITApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
            }
            else
            {
                return new DbContextOptionsBuilder<MITApplicationDbContext>()
                    .UseMySQL(connectionString)
                    .Options;
            }
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("dbconnections.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity configuration

            // Map to existing AspNet* tables (already created by JsonApiDbContext migrations)
            modelBuilder.Entity<MITApplicationUser>()
                .ToTable("AspNetUsers");

            modelBuilder.Entity<MITApplicationRole>()
                .ToTable("AspNetRoles");

            modelBuilder.Entity<IdentityUserRole<string>>()
                .ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserClaim<string>>()
                .ToTable("AspNetUserClaims");

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .ToTable("AspNetUserLogins");

            modelBuilder.Entity<IdentityUserToken<string>>()
                .ToTable("AspNetUserTokens");

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .ToTable("AspNetRoleClaims");
        }
    }
}
