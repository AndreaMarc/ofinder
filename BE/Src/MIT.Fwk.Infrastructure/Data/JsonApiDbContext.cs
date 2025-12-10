using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Services;
using System.Linq;
using System.Linq.Expressions;
using MIT.Fwk.Infrastructure.Entities;

namespace MIT.Fwk.Infrastructure.Data
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public partial class JsonApiDbContext(DbContextOptions<JsonApiDbContext> options, ITenantProvider tenantProvider)
        : DbContext(options), IJsonApiDbContext, IMigrationDbContext
    {
        private readonly DbContextOptions<JsonApiDbContext> _dbContextOptions;
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

        public JsonApiDbContext() : this(_UseSqlServer ? new DbContextOptionsBuilder<JsonApiDbContext>()
                .UseSqlServer(GetConfiguration().GetConnectionString(nameof(JsonApiDbContext)))
                .Options : new DbContextOptionsBuilder<JsonApiDbContext>()
                .UseMySQL(GetConfiguration().GetConnectionString(nameof(JsonApiDbContext)))
                .Options)
        {
        }


        private JsonApiDbContext(DbContextOptions<JsonApiDbContext> dbContextOptions) : this(dbContextOptions, null)
        {
            _dbContextOptions = dbContextOptions;
        }

        // OnConfiguring removed - not needed when using DI with pre-configured options
        // Design-time migrations use parametrless constructor which already configures options
        // Runtime uses DI from Startup.cs with IConnectionStringProvider

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPreference>()
                .HasKey(e => new { e.TenantId, e.UserId, e.PrefKey });

            // Configure UploadFile primary key (UploadId is nullable, so must be explicit)
            modelBuilder.Entity<UploadFile>()
                .HasKey(e => e.UploadId);

            modelBuilder.Entity<MediaFile>()
                .HasOne(file => file.TypologyAreaRel)
                .WithMany(mediaCategories => mediaCategories.TypologyMediaFiles)
                .HasForeignKey("typologyArea");

            modelBuilder.Entity<MediaFile>()
                .HasOne(file => file.CategoryRel)
                .WithMany(mediaCategories => mediaCategories.CategoryMediaFiles)
                .HasForeignKey("category");

            modelBuilder.Entity<MediaFile>()
                .HasOne(file => file.AlbumRel)
                .WithMany(mediaCategories => mediaCategories.AlbumMediaFiles)
                .HasForeignKey("album");

            modelBuilder.Entity<Ticket>()
                .Property(i => i.Number)
                .UseIdentityColumn();

            modelBuilder.Entity<TicketTag>()
                .HasIndex(i => i.Tag)
                .IsUnique();

            // Aggiungi HasQueryFilter a tutte le entità
            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
                {
                    System.Reflection.MethodInfo method = typeof(ModelBuilder).GetMethods()
                        .First(m => m.Name == nameof(ModelBuilder.Entity) && m.IsGenericMethod)
                        .MakeGenericMethod(entityType.ClrType);

                    object entityBuilder = method.Invoke(modelBuilder, null);
                    System.Reflection.MethodInfo hasQueryFilterMethod = entityBuilder.GetType().GetMethods()
                        .First(m => m.Name == nameof(EntityTypeBuilder.HasQueryFilter) && m.GetParameters().First().ParameterType == typeof(LambdaExpression));
                    ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
                    LambdaExpression filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(IHasTenant.TenantId)),
                            Expression.Constant(tenantProvider.TenantId)),
                        parameter);

                    hasQueryFilterMethod.Invoke(entityBuilder, new object[] { filter });
                }
            }

            // ===== Performer Management System Configuration =====

            // Performer
            modelBuilder.Entity<Performer>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Timestamps with MySQL default values
                // CreatedAt: No DEFAULT (set by application code)
                // Removed .ValueGeneratedOnAdd() to prevent EF Core from generating DEFAULT CURRENT_TIMESTAMP

                // UpdatedAt: DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.GeoCountry)
                    .WithMany()
                    .HasForeignKey(e => e.GeoCountryId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.GeoFirstDivision)
                    .WithMany()
                    .HasForeignKey(e => e.GeoFirstDivisionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.GeoSecondDivision)
                    .WithMany()
                    .HasForeignKey(e => e.GeoSecondDivisionId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Channel
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                // Convert enum to string
                entity.Property(e => e.Platform)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasOne(e => e.Performer)
                    .WithMany(p => p.Channels)
                    .HasForeignKey(e => e.PerformerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PerformerId);
                entity.HasIndex(e => e.Platform);
            });

            // ChannelSchedule
            modelBuilder.Entity<ChannelSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(e => e.Channel)
                    .WithMany(c => c.ChannelSchedules)
                    .HasForeignKey(e => e.ChannelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ChannelId);
                entity.HasIndex(e => e.DayOfWeek);
            });

            // ChannelContentType
            modelBuilder.Entity<ChannelContentType>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(e => e.Channel)
                    .WithMany(c => c.ChannelContentTypes)
                    .HasForeignKey(e => e.ChannelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ChannelId);
            });

            // ChannelPricing
            modelBuilder.Entity<ChannelPricing>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(e => e.Channel)
                    .WithOne(c => c.ChannelPricing)
                    .HasForeignKey<ChannelPricing>(e => e.ChannelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ChannelId).IsUnique();
            });

            // PerformerReview
            modelBuilder.Entity<PerformerReview>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(e => e.Performer)
                    .WithMany(p => p.PerformerReviews)
                    .HasForeignKey(e => e.PerformerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // UNIQUE constraint: one review per user per performer
                entity.HasIndex(e => new { e.PerformerId, e.UserId }).IsUnique();
                entity.HasIndex(e => e.Rating);
            });

            // PerformerService
            modelBuilder.Entity<PerformerService>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ServiceType)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasOne(e => e.Performer)
                    .WithMany(p => p.PerformerServices)
                    .HasForeignKey(e => e.PerformerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PerformerId);
            });

            // PerformerView
            modelBuilder.Entity<PerformerView>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ViewedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Performer)
                    .WithMany(p => p.PerformerViews)
                    .HasForeignKey(e => e.PerformerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // UNIQUE constraint: one view record per user per performer
                entity.HasIndex(e => new { e.PerformerId, e.UserId }).IsUnique();
            });

            // UserFavorite
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Performer)
                    .WithMany(p => p.UserFavorites)
                    .HasForeignKey(e => e.PerformerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // UNIQUE constraint: one favorite per user per performer
                entity.HasIndex(e => new { e.UserId, e.PerformerId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<AspNetUserClaim> AspNetUserClaims => Set<AspNetUserClaim>();
        public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
        public DbSet<Setup> Setups => Set<Setup>();
        public DbSet<Translation> Translations => Set<Translation>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<UserAudit> UserAudits => Set<UserAudit>();
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();
        public DbSet<UserTenant> UserTenants => Set<UserTenant>();
        public DbSet<Template> Templates => Set<Template>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Otp> Otps => Set<Otp>();
        public DbSet<LegalTerm> LegalTerms => Set<LegalTerm>();
        public DbSet<ThirdPartsToken> ThirdPartsTokens => Set<ThirdPartsToken>();
        public DbSet<Integration> Integrations => Set<Integration>();
        public DbSet<BannedUser> BannedUsers => Set<BannedUser>();
        public DbSet<MediaCategory> MediaCategories => Set<MediaCategory>();
        public DbSet<CustomSetup> CustomSetups => Set<CustomSetup>();
        public DbSet<UploadFile> UploadFiles => Set<UploadFile>();

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketArea> TicketAreas => Set<TicketArea>();
        public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
        public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();
        public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
        public DbSet<TicketOperator> TicketOperators => Set<TicketOperator>();
        public DbSet<TicketRelation> TicketRelations => Set<TicketRelation>();
        public DbSet<TicketTag> TicketTags => Set<TicketTag>();
        public DbSet<GeoCity> GeoCities => Set<GeoCity>();
        public DbSet<GeoCountry> GeoCountries => Set<GeoCountry>();
        public DbSet<GeoMapping> GeoMappings => Set<GeoMapping>();
        public DbSet<GeoRegion> GeoRegions => Set<GeoRegion>();
        public DbSet<GeoSecondDivision> GeoSecondDivisions => Set<GeoSecondDivision>();
        public DbSet<GeoSubregion> GeoSubregions => Set<GeoSubregion>();
        public DbSet<GeoThirdDivision> GeoThirdDivisions => Set<GeoThirdDivision>();
        public DbSet<GeoFirstDivision> GeoFirstDivisions => Set<GeoFirstDivision>();
        public DbSet<FwkAddon> FwkAddons => Set<FwkAddon>();
        public DbSet<DeadlineArchive> DeadlineArchives => Set<DeadlineArchive>();
        public DbSet<BlockNotification> BlockNotifications => Set<BlockNotification>();

        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

        //---------- campi per ERP -------------------
        public DbSet<ErpEmployee> ErpEmployees { get; set; }
        public DbSet<ErpExternalWorkerDetails> ErpExternalWorkerDetails { get; set; }
        public DbSet<ErpRole> ErpRoles { get; set; }
        public DbSet<ErpEmployeeRole> ErpEmployeeRoles { get; set; }
        public DbSet<ErpSite> ErpSites { get; set; }
        public DbSet<ErpSiteUserMapping> ErpSiteUserMappings { get; set; }
        public DbSet<ErpShift> ErpShifts { get; set; }
        public DbSet<ErpSiteWorkingTime> ErpSiteWorkingTimes { get; set; }
        public DbSet<ErpEmployeeWorkingHours> ErpEmployeeWorkingHours { get; set; }

        //---------- Performer Management System -------------------
        public DbSet<Performer> Performers => Set<Performer>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<ChannelSchedule> ChannelSchedules => Set<ChannelSchedule>();
        public DbSet<ChannelContentType> ChannelContentTypes => Set<ChannelContentType>();
        public DbSet<ChannelPricing> ChannelPricing => Set<ChannelPricing>();
        public DbSet<PerformerReview> PerformerReviews => Set<PerformerReview>();
        public DbSet<PerformerService> PerformerServices => Set<PerformerService>();
        public DbSet<PerformerView> PerformerViews => Set<PerformerView>();
        public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

    }
}
