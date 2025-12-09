using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace MIT.Fwk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    NormalizedName = table.Column<string>(type: "longtext", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Needful = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CopyInNewTenants = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    Typology = table.Column<string>(type: "longtext", nullable: true),
                    Initials = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "longtext", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "longtext", nullable: true),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserName = table.Column<string>(type: "longtext", nullable: true),
                    LastIp = table.Column<string>(type: "longtext", nullable: true),
                    FirstName = table.Column<string>(type: "longtext", nullable: true),
                    LastName = table.Column<string>(type: "longtext", nullable: true),
                    LastAccess = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PasswordLastChange = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsPasswordMd5 = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FakeEmail = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FreeFieldString1 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldString2 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldString3 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FreeFieldInt2 = table.Column<int>(type: "int", nullable: true),
                    FreeFieldInt1 = table.Column<int>(type: "int", nullable: true),
                    FreeFieldBoolean = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomSetups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Generic = table.Column<string>(type: "longtext", nullable: true),
                    Environment = table.Column<string>(type: "longtext", nullable: true),
                    MaintenanceAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSetups", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FwkAddons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    AddonsCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FwkAddons", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Translations = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoRegions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Integrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Code = table.Column<string>(type: "longtext", nullable: true),
                    EncryptionKey = table.Column<string>(type: "longtext", nullable: true),
                    Url = table.Column<string>(type: "longtext", nullable: true),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LegalTerms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: true),
                    Language = table.Column<string>(type: "longtext", nullable: true),
                    Version = table.Column<string>(type: "longtext", nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    DataActivation = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalTerms", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Setups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    environment = table.Column<string>(type: "longtext", nullable: true),
                    minAppVersion = table.Column<string>(type: "longtext", nullable: true),
                    maintenance = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    useRemoteFiles = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    disableLog = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    publicRegistration = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sliderPosition = table.Column<string>(type: "longtext", nullable: true),
                    sliderPics = table.Column<string>(type: "longtext", nullable: true),
                    availableLanguages = table.Column<string>(type: "longtext", nullable: true),
                    defaultLanguage = table.Column<string>(type: "longtext", nullable: true),
                    failedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    previousPasswordsStored = table.Column<int>(type: "int", nullable: false),
                    defaultUserPassword = table.Column<string>(type: "longtext", nullable: true),
                    languageSetup = table.Column<string>(type: "longtext", nullable: true),
                    passwordExpirationPeriod = table.Column<int>(type: "int", nullable: false),
                    blockingPeriodDuration = table.Column<int>(type: "int", nullable: false),
                    sliderRegistrationPosition = table.Column<string>(type: "longtext", nullable: true),
                    sliderTermsPosition = table.Column<string>(type: "longtext", nullable: true),
                    headerLight = table.Column<string>(type: "longtext", nullable: true),
                    sidebarLight = table.Column<string>(type: "longtext", nullable: true),
                    headerBackground = table.Column<string>(type: "longtext", nullable: true),
                    sidebarBackground = table.Column<string>(type: "longtext", nullable: true),
                    beLanguage = table.Column<string>(type: "longtext", nullable: true),
                    fixedSidebar = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fixedFooter = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fixedHeader = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    bodyTabsShadow = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    bodyTabsLine = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    appThemeWhite = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    headerShadow = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sidebarShadow = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    useUrlStaticFiles = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    entitiesList = table.Column<string>(type: "longtext", nullable: true),
                    routesList = table.Column<string>(type: "longtext", nullable: true),
                    defaultClaims = table.Column<string>(type: "longtext", nullable: true),
                    accessTokenExpiresIn = table.Column<int>(type: "int", nullable: false),
                    refreshTokenExpiresIn = table.Column<int>(type: "int", nullable: false),
                    canChangeTenants = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    internalChat = table.Column<string>(type: "longtext", nullable: true),
                    internalNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pushNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    canSearch = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    registrationFields = table.Column<string>(type: "longtext", nullable: true),
                    mailTokenExpiresIn = table.Column<int>(type: "int", nullable: false),
                    mailerUsesAltText = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    forceLoginRedirect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    needRequestAssociation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    maeUsers = table.Column<string>(type: "longtext", nullable: true),
                    thirdPartsAccesses = table.Column<string>(type: "longtext", nullable: true),
                    googleCredentials = table.Column<string>(type: "longtext", nullable: true),
                    useMD5 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    logicDelete = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    rolesForEditUsers = table.Column<string>(type: "longtext", nullable: true),
                    applicationName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setups", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Organization = table.Column<string>(type: "longtext", nullable: true),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentTenant = table.Column<int>(type: "int", nullable: false),
                    TenantVAT = table.Column<string>(type: "longtext", nullable: true),
                    TaxId = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    TenantPEC = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    WebSite = table.Column<string>(type: "longtext", nullable: true),
                    TenantSDI = table.Column<string>(type: "longtext", nullable: true),
                    TenantIBAN = table.Column<string>(type: "longtext", nullable: true),
                    Owner = table.Column<string>(type: "longtext", nullable: true),
                    Commercial = table.Column<string>(type: "longtext", nullable: true),
                    ShareCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RegisteredOfficeAddress = table.Column<string>(type: "longtext", nullable: true),
                    RegisteredOfficeCity = table.Column<string>(type: "longtext", nullable: true),
                    RegisteredOfficeProvince = table.Column<string>(type: "longtext", nullable: true),
                    RegisteredOfficeState = table.Column<string>(type: "longtext", nullable: true),
                    RegisteredOfficeRegion = table.Column<string>(type: "longtext", nullable: true),
                    RegisteredOfficeZIP = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressAddress = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressCity = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressProvince = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressState = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressRegion = table.Column<string>(type: "longtext", nullable: true),
                    BillingAddressZIP = table.Column<string>(type: "longtext", nullable: true),
                    matchBillingAddress = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    isErasable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    isRecovery = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FreeFieldString1 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldString2 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldString3 = table.Column<string>(type: "longtext", nullable: true),
                    FreeFieldDateTime1 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FreeFieldDateTime2 = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FreeFieldInt2 = table.Column<int>(type: "int", nullable: true),
                    FreeFieldInt1 = table.Column<int>(type: "int", nullable: true),
                    FreeFieldFloat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FreeFieldBoolean1 = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    FreeFieldBoolean2 = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketAreas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAreas", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketRelations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketRelations", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Tag = table.Column<string>(type: "varchar(255)", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTags", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    languageCode = table.Column<string>(type: "longtext", nullable: true),
                    translationWeb = table.Column<string>(type: "longtext", nullable: true),
                    translationApp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UploadFiles",
                columns: table => new
                {
                    UploadId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UploadUid = table.Column<string>(type: "longtext", nullable: true),
                    UploadType = table.Column<string>(type: "longtext", nullable: true),
                    UploadSrc = table.Column<string>(type: "longtext", nullable: true),
                    UploadFileName = table.Column<string>(type: "longtext", nullable: true),
                    UploadActive = table.Column<short>(type: "smallint", nullable: false),
                    UploadCreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UploadAlbum = table.Column<string>(type: "longtext", nullable: true),
                    UploadExtension = table.Column<string>(type: "longtext", nullable: true),
                    UploadFileSize = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadFiles", x => x.UploadId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserAudit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    AuditEvent = table.Column<short>(type: "smallint", nullable: false),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserAudit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserAudit_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    Body = table.Column<string>(type: "longtext", nullable: true),
                    Data = table.Column<string>(type: "longtext", nullable: true),
                    OnlyData = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MessageId = table.Column<string>(type: "longtext", nullable: true),
                    DateSent = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DateRead = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Erased = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PushType = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThirdPartsTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    OtpId = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    AccessToken = table.Column<string>(type: "longtext", nullable: true),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    AccessType = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartsTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdPartsTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "varchar(255)", nullable: true),
                    deviceHash = table.Column<string>(type: "longtext", nullable: true),
                    salt = table.Column<string>(type: "longtext", nullable: true),
                    lastAccess = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    PushToken = table.Column<string>(type: "longtext", nullable: true),
                    AppleToken = table.Column<string>(type: "longtext", nullable: true),
                    GoogleToken = table.Column<string>(type: "longtext", nullable: true),
                    FacebookToken = table.Column<string>(type: "longtext", nullable: true),
                    TwitterToken = table.Column<string>(type: "longtext", nullable: true),
                    Platform = table.Column<string>(type: "longtext", nullable: true),
                    DeviceName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPreference",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PrefKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    PrefValue = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Id = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreference", x => new { x.TenantId, x.UserId, x.PrefKey });
                    table.ForeignKey(
                        name: "FK_UserPreference_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserProfile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    FirstName = table.Column<string>(type: "longtext", nullable: true),
                    LastName = table.Column<string>(type: "longtext", nullable: true),
                    NickName = table.Column<string>(type: "longtext", nullable: true),
                    FixedPhone = table.Column<string>(type: "longtext", nullable: true),
                    MobilePhone = table.Column<string>(type: "longtext", nullable: true),
                    Sex = table.Column<string>(type: "longtext", nullable: true),
                    TaxId = table.Column<string>(type: "longtext", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BirthCity = table.Column<string>(type: "longtext", nullable: true),
                    BirthProvince = table.Column<string>(type: "longtext", nullable: true),
                    BirthZIP = table.Column<string>(type: "longtext", nullable: true),
                    BirthState = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceCity = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceProvince = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceZIP = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceState = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceAddress = table.Column<string>(type: "longtext", nullable: true),
                    ResidenceHouseNumber = table.Column<string>(type: "longtext", nullable: true),
                    Occupation = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    ContactEmail = table.Column<string>(type: "longtext", nullable: true),
                    ProfileImageId = table.Column<string>(type: "longtext", nullable: true),
                    ProfileFreeFieldString1 = table.Column<string>(type: "longtext", nullable: true),
                    ProfileFreeFieldString2 = table.Column<string>(type: "longtext", nullable: true),
                    ProfileFreeFieldString3 = table.Column<string>(type: "longtext", nullable: true),
                    AppleRefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    GoogleRefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    FacebookRefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    TwitterRefreshToken = table.Column<string>(type: "longtext", nullable: true),
                    UserLang = table.Column<string>(type: "longtext", nullable: true),
                    ProfileFreeFieldInt1 = table.Column<int>(type: "int", nullable: true),
                    ProfileFreeFieldInt2 = table.Column<int>(type: "int", nullable: true),
                    ProfileFreeFieldDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProfileFreeFieldBoolean = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    cookieAccepted = table.Column<string>(type: "longtext", nullable: true),
                    termsAccepted = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    termsAcceptanceDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfile_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoSubregions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Translations = table.Column<string>(type: "longtext", nullable: true),
                    GeoRegionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoSubregions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoSubregions_GeoRegions_GeoRegionId",
                        column: x => x.GeoRegionId,
                        principalTable: "GeoRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BannedUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    SupervisorId = table.Column<string>(type: "longtext", nullable: true),
                    LockStart = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CrossTenantBanned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LockDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannedUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BannedUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BlockNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PushBlock = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailBlock = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlockNotifications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Type = table.Column<string>(type: "longtext", nullable: true),
                    ParentCategory = table.Column<int>(type: "int", nullable: false),
                    Erasable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CopyInNewTenants = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpEmployee",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HiredDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TerminatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ContractType = table.Column<string>(type: "longtext", nullable: true),
                    EmployeeType = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpEmployee_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpEmployee_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpEmployeeWorkingHours",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    ErpEmployeeId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    StartFlexibility = table.Column<int>(type: "int", nullable: true),
                    EndFlexibility = table.Column<int>(type: "int", nullable: true),
                    MinimumBreakDuration = table.Column<int>(type: "int", nullable: true),
                    MaximumBreakDuration = table.Column<int>(type: "int", nullable: true),
                    DailyWorkingTime = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpEmployeeWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpEmployeeWorkingHours_AspNetUsers_ErpEmployeeId",
                        column: x => x.ErpEmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpEmployeeWorkingHours_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsManagerial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpRole_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpRole_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpShift",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    StandardWorkingTime = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpShift", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpShift_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpSite",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Address = table.Column<string>(type: "longtext", nullable: true),
                    AddressNumber = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "longtext", nullable: true),
                    City = table.Column<string>(type: "longtext", nullable: true),
                    Province = table.Column<string>(type: "longtext", nullable: true),
                    Zip = table.Column<string>(type: "longtext", nullable: true),
                    State = table.Column<string>(type: "longtext", nullable: true),
                    AdministrativeHeadquarters = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisteredOffice = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperationalHeadquarters = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentSiteId = table.Column<string>(type: "varchar(255)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpSite_ErpSite_ParentSiteId",
                        column: x => x.ParentSiteId,
                        principalTable: "ErpSite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpSite_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Code = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Erasable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: true),
                    ParentMediaCategory = table.Column<string>(type: "longtext", nullable: true),
                    copyInNewTenant = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaCategories_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Otps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    OtpValue = table.Column<string>(type: "longtext", nullable: true),
                    OtpSended = table.Column<string>(type: "longtext", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    IsValid = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Otps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Otps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Otps_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserTenants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Ip = table.Column<string>(type: "longtext", nullable: true),
                    State = table.Column<string>(type: "longtext", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTenants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketOperators",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TicketAreaId = table.Column<string>(type: "varchar(255)", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketOperators_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketOperators_TicketAreas_TicketAreaId",
                        column: x => x.TicketAreaId,
                        principalTable: "TicketAreas",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    Organization = table.Column<string>(type: "longtext", nullable: true),
                    Vat = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Message = table.Column<string>(type: "longtext", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AssignedId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Phone = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ClosedById = table.Column<string>(type: "longtext", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Answer = table.Column<string>(type: "longtext", nullable: true),
                    FirstName = table.Column<string>(type: "longtext", nullable: true),
                    LastName = table.Column<string>(type: "longtext", nullable: true),
                    OrganizationToBeConfirmed = table.Column<string>(type: "longtext", nullable: true),
                    Number = table.Column<int>(type: "int", nullable: true),
                    TicketTagId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_AssignedId",
                        column: x => x.AssignedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tickets_TicketTags_TicketTagId",
                        column: x => x.TicketTagId,
                        principalTable: "TicketTags",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Iso3 = table.Column<string>(type: "longtext", nullable: true),
                    NumericCode = table.Column<string>(type: "longtext", nullable: true),
                    Iso2 = table.Column<string>(type: "longtext", nullable: true),
                    PhoneCode = table.Column<string>(type: "longtext", nullable: true),
                    Capital = table.Column<string>(type: "longtext", nullable: true),
                    Currency = table.Column<string>(type: "longtext", nullable: true),
                    CurrencyName = table.Column<string>(type: "longtext", nullable: true),
                    CurrencySymbol = table.Column<string>(type: "longtext", nullable: true),
                    InternetDomain = table.Column<string>(type: "longtext", nullable: true),
                    Native = table.Column<string>(type: "longtext", nullable: true),
                    GeoRegionId = table.Column<int>(type: "int", nullable: true),
                    GeoSubregionId = table.Column<int>(type: "int", nullable: true),
                    TimeZones = table.Column<string>(type: "longtext", nullable: true),
                    Translations = table.Column<string>(type: "longtext", nullable: true),
                    NumberOfDivisions = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoCountries_GeoRegions_GeoRegionId",
                        column: x => x.GeoRegionId,
                        principalTable: "GeoRegions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GeoCountries_GeoSubregions_GeoSubregionId",
                        column: x => x.GeoSubregionId,
                        principalTable: "GeoSubregions",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Template",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    ContentNoHtml = table.Column<string>(type: "longtext", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Tags = table.Column<string>(type: "longtext", nullable: true),
                    Code = table.Column<string>(type: "longtext", nullable: true),
                    Language = table.Column<string>(type: "longtext", nullable: true),
                    ObjectText = table.Column<string>(type: "longtext", nullable: true),
                    FeaturedImage = table.Column<string>(type: "longtext", nullable: true),
                    Erasable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CopyInNewTenants = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Erased = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    FreeField = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Template", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Template_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpExternalWorkerDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "varchar(255)", nullable: true),
                    VatNumber = table.Column<string>(type: "longtext", nullable: true),
                    ContractDetails = table.Column<string>(type: "longtext", nullable: true),
                    PaymentFrequency = table.Column<string>(type: "longtext", nullable: true),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpExternalWorkerDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpExternalWorkerDetails_ErpEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "ErpEmployee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpExternalWorkerDetails_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpEmployeeRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "varchar(255)", nullable: true),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: true),
                    ManagerId = table.Column<string>(type: "varchar(255)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpEmployeeRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpEmployeeRole_ErpEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "ErpEmployee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpEmployeeRole_ErpEmployee_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "ErpEmployee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpEmployeeRole_ErpRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ErpRole",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpEmployeeRole_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpSiteUserMapping",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<string>(type: "varchar(255)", nullable: true),
                    EmployeeId = table.Column<string>(type: "longtext", nullable: true),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MappingStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MappingEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSiteUserMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpSiteUserMapping_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpSiteUserMapping_ErpSite_SiteId",
                        column: x => x.SiteId,
                        principalTable: "ErpSite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpSiteUserMapping_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ErpSiteWorkingTime",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<string>(type: "varchar(255)", nullable: true),
                    ShiftId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Day = table.Column<int>(type: "int", nullable: false),
                    StartFlexibility = table.Column<int>(type: "int", nullable: false),
                    EndFlexibility = table.Column<int>(type: "int", nullable: false),
                    MinimumBreakDuration = table.Column<int>(type: "int", nullable: false),
                    MaximumBreakDuration = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpSiteWorkingTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpSiteWorkingTime_ErpShift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "ErpShift",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpSiteWorkingTime_ErpSite_SiteId",
                        column: x => x.SiteId,
                        principalTable: "ErpSite",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ErpSiteWorkingTime_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    typologyArea = table.Column<string>(type: "varchar(255)", nullable: true),
                    category = table.Column<string>(type: "varchar(255)", nullable: true),
                    album = table.Column<string>(type: "varchar(255)", nullable: true),
                    originalFileName = table.Column<string>(type: "longtext", nullable: true),
                    extension = table.Column<string>(type: "longtext", nullable: true),
                    fileUrl = table.Column<string>(type: "longtext", nullable: true),
                    base64 = table.Column<string>(type: "longtext", nullable: true),
                    mongoGuid = table.Column<string>(type: "longtext", nullable: true),
                    alt = table.Column<string>(type: "longtext", nullable: true),
                    tag = table.Column<string>(type: "longtext", nullable: true),
                    tenantId = table.Column<int>(type: "int", nullable: false),
                    userGuid = table.Column<string>(type: "longtext", nullable: true),
                    uploadDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    primaryContentType = table.Column<string>(type: "longtext", nullable: true),
                    global = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaCategories_album",
                        column: x => x.album,
                        principalTable: "MediaCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaCategories_category",
                        column: x => x.category,
                        principalTable: "MediaCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediaFiles_MediaCategories_typologyArea",
                        column: x => x.typologyArea,
                        principalTable: "MediaCategories",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeadlineArchives",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Replaced = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OperatorId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Entity = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadlineArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeadlineArchives_TicketOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "TicketOperators",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Operation = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TicketOperatorId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Details = table.Column<string>(type: "longtext", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistory_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketHistory_TicketOperators_TicketOperatorId",
                        column: x => x.TicketOperatorId,
                        principalTable: "TicketOperators",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TicketId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Message = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TicketOperatorId = table.Column<string>(type: "varchar(255)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketMessages_TicketOperators_TicketOperatorId",
                        column: x => x.TicketOperatorId,
                        principalTable: "TicketOperators",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketMessages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketTicketRelations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    TicketRelationId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TicketId = table.Column<string>(type: "varchar(255)", nullable: true),
                    Typology = table.Column<string>(type: "longtext", nullable: true),
                    FatherTicketId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTicketRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTicketRelations_TicketRelations_TicketRelationId",
                        column: x => x.TicketRelationId,
                        principalTable: "TicketRelations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketTicketRelations_Tickets_FatherTicketId",
                        column: x => x.FatherTicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketTicketRelations_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    GeoCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoCities_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoFirstDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    GeoCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoFirstDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoFirstDivisions_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketAttachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Field = table.Column<string>(type: "longtext", nullable: true),
                    TicketMessageId = table.Column<string>(type: "varchar(255)", nullable: true),
                    TicketOperatorId = table.Column<string>(type: "varchar(255)", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAttachments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketAttachments_TicketMessages_TicketMessageId",
                        column: x => x.TicketMessageId,
                        principalTable: "TicketMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketAttachments_TicketOperators_TicketOperatorId",
                        column: x => x.TicketOperatorId,
                        principalTable: "TicketOperators",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoSecondDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    GeoFirstDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoSecondDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoSecondDivisions_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoSecondDivisions_GeoFirstDivisions_GeoFirstDivisionId",
                        column: x => x.GeoFirstDivisionId,
                        principalTable: "GeoFirstDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoThirdDivisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    GeoFirstDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoSecondDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoThirdDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoThirdDivisions_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoThirdDivisions_GeoFirstDivisions_GeoFirstDivisionId",
                        column: x => x.GeoFirstDivisionId,
                        principalTable: "GeoFirstDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoThirdDivisions_GeoSecondDivisions_GeoSecondDivisionId",
                        column: x => x.GeoSecondDivisionId,
                        principalTable: "GeoSecondDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Performers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    GeoCountryId = table.Column<int>(type: "int", nullable: true),
                    GeoFirstDivisionId = table.Column<int>(type: "int", nullable: true),
                    GeoSecondDivisionId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Performers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Performers_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Performers_GeoFirstDivisions_GeoFirstDivisionId",
                        column: x => x.GeoFirstDivisionId,
                        principalTable: "GeoFirstDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Performers_GeoSecondDivisions_GeoSecondDivisionId",
                        column: x => x.GeoSecondDivisionId,
                        principalTable: "GeoSecondDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GeoMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ZipCode = table.Column<string>(type: "longtext", nullable: true),
                    GeoFirstDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoSecondDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoThirdDivisionId = table.Column<int>(type: "int", nullable: false),
                    GeoCityId = table.Column<int>(type: "int", nullable: false),
                    GeoCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeoMappings_GeoCities_GeoCityId",
                        column: x => x.GeoCityId,
                        principalTable: "GeoCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoMappings_GeoCountries_GeoCountryId",
                        column: x => x.GeoCountryId,
                        principalTable: "GeoCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoMappings_GeoFirstDivisions_GeoFirstDivisionId",
                        column: x => x.GeoFirstDivisionId,
                        principalTable: "GeoFirstDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoMappings_GeoSecondDivisions_GeoSecondDivisionId",
                        column: x => x.GeoSecondDivisionId,
                        principalTable: "GeoSecondDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeoMappings_GeoThirdDivisions_GeoThirdDivisionId",
                        column: x => x.GeoThirdDivisionId,
                        principalTable: "GeoThirdDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Platform = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UsernameHandle = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ProfileLink = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerformerReviews",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ReviewText = table.Column<string>(type: "longtext", nullable: true),
                    IsVerifiedPurchase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformerReviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformerReviews_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerformerServices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ServiceType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Link = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformerServices_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PerformerViews",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformerViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformerViews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformerViews_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavorites_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChannelContentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelContentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelContentTypes_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChannelPricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<string>(type: "varchar(255)", nullable: false),
                    MonthlySubscriptionFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlySubscriptionTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PhotoSaleFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PhotoSaleTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VideoSaleFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VideoSaleTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LivePublicFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LivePublicTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LivePrivateFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LivePrivateTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClothingSalesFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClothingSalesTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExtraContentFrom = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExtraContentTo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelPricing_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChannelSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<string>(type: "varchar(255)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelSchedules_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserAudit_UserId",
                table: "AspNetUserAudit",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_TenantId",
                table: "AspNetUserRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BannedUsers_TenantId",
                table: "BannedUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BannedUsers_UserId",
                table: "BannedUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockNotifications_TenantId",
                table: "BlockNotifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockNotifications_UserId",
                table: "BlockNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_TenantId",
                table: "Category",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelContentTypes_ChannelId",
                table: "ChannelContentTypes",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPricing_ChannelId",
                table: "ChannelPricing",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_PerformerId",
                table: "Channels",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Platform",
                table: "Channels",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSchedules_ChannelId",
                table: "ChannelSchedules",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSchedules_DayOfWeek",
                table: "ChannelSchedules",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_DeadlineArchives_OperatorId",
                table: "DeadlineArchives",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployee_TenantId",
                table: "ErpEmployee",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployee_UserId",
                table: "ErpEmployee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeRole_EmployeeId",
                table: "ErpEmployeeRole",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeRole_ManagerId",
                table: "ErpEmployeeRole",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeRole_RoleId",
                table: "ErpEmployeeRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeRole_TenantId",
                table: "ErpEmployeeRole",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeWorkingHours_ErpEmployeeId",
                table: "ErpEmployeeWorkingHours",
                column: "ErpEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpEmployeeWorkingHours_TenantId",
                table: "ErpEmployeeWorkingHours",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpExternalWorkerDetails_EmployeeId",
                table: "ErpExternalWorkerDetails",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpExternalWorkerDetails_TenantId",
                table: "ErpExternalWorkerDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpRole_RoleId",
                table: "ErpRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpRole_TenantId",
                table: "ErpRole",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpShift_TenantId",
                table: "ErpShift",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSite_ParentSiteId",
                table: "ErpSite",
                column: "ParentSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSite_TenantId",
                table: "ErpSite",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteUserMapping_SiteId",
                table: "ErpSiteUserMapping",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteUserMapping_TenantId",
                table: "ErpSiteUserMapping",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteUserMapping_UserId",
                table: "ErpSiteUserMapping",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteWorkingTime_ShiftId",
                table: "ErpSiteWorkingTime",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteWorkingTime_SiteId",
                table: "ErpSiteWorkingTime",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpSiteWorkingTime_TenantId",
                table: "ErpSiteWorkingTime",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCities_GeoCountryId",
                table: "GeoCities",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCountries_GeoRegionId",
                table: "GeoCountries",
                column: "GeoRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCountries_GeoSubregionId",
                table: "GeoCountries",
                column: "GeoSubregionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoFirstDivisions_GeoCountryId",
                table: "GeoFirstDivisions",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoMappings_GeoCityId",
                table: "GeoMappings",
                column: "GeoCityId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoMappings_GeoCountryId",
                table: "GeoMappings",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoMappings_GeoFirstDivisionId",
                table: "GeoMappings",
                column: "GeoFirstDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoMappings_GeoSecondDivisionId",
                table: "GeoMappings",
                column: "GeoSecondDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoMappings_GeoThirdDivisionId",
                table: "GeoMappings",
                column: "GeoThirdDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoSecondDivisions_GeoCountryId",
                table: "GeoSecondDivisions",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoSecondDivisions_GeoFirstDivisionId",
                table: "GeoSecondDivisions",
                column: "GeoFirstDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoSubregions_GeoRegionId",
                table: "GeoSubregions",
                column: "GeoRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoThirdDivisions_GeoCountryId",
                table: "GeoThirdDivisions",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoThirdDivisions_GeoFirstDivisionId",
                table: "GeoThirdDivisions",
                column: "GeoFirstDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeoThirdDivisions_GeoSecondDivisionId",
                table: "GeoThirdDivisions",
                column: "GeoSecondDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCategories_TenantId",
                table: "MediaCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_album",
                table: "MediaFiles",
                column: "album");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_category",
                table: "MediaFiles",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_typologyArea",
                table: "MediaFiles",
                column: "typologyArea");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Otps_TenantId",
                table: "Otps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Otps_UserId",
                table: "Otps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId",
                table: "PasswordHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerReviews_PerformerId_UserId",
                table: "PerformerReviews",
                columns: new[] { "PerformerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformerReviews_Rating",
                table: "PerformerReviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerReviews_UserId",
                table: "PerformerReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_CreatedAt",
                table: "Performers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_GeoCountryId",
                table: "Performers",
                column: "GeoCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_GeoFirstDivisionId",
                table: "Performers",
                column: "GeoFirstDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_GeoSecondDivisionId",
                table: "Performers",
                column: "GeoSecondDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_IsActive",
                table: "Performers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Performers_UserId",
                table: "Performers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformerServices_PerformerId",
                table: "PerformerServices",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformerViews_PerformerId_UserId",
                table: "PerformerViews",
                columns: new[] { "PerformerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformerViews_UserId",
                table: "PerformerViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Template_CategoryId",
                table: "Template",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdPartsTokens_UserId",
                table: "ThirdPartsTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TicketMessageId",
                table: "TicketAttachments",
                column: "TicketMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_TicketOperatorId",
                table: "TicketAttachments",
                column: "TicketOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_UserId",
                table: "TicketAttachments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_TicketOperatorId",
                table: "TicketHistory",
                column: "TicketOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistory_UserId",
                table: "TicketHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketOperatorId",
                table: "TicketMessages",
                column: "TicketOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_UserId",
                table: "TicketMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketOperators_TicketAreaId",
                table: "TicketOperators",
                column: "TicketAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketOperators_UserId",
                table: "TicketOperators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedId",
                table: "Tickets",
                column: "AssignedId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketTagId",
                table: "Tickets",
                column: "TicketTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTags_Tag",
                table: "TicketTags",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTicketRelations_FatherTicketId",
                table: "TicketTicketRelations",
                column: "FatherTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTicketRelations_TicketId",
                table: "TicketTicketRelations",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTicketRelations_TicketRelationId",
                table: "TicketTicketRelations",
                column: "TicketRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_userId",
                table: "UserDevices",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_PerformerId",
                table: "UserFavorites",
                column: "PerformerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UserId_PerformerId",
                table: "UserFavorites",
                columns: new[] { "UserId", "PerformerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreference_UserId",
                table: "UserPreference",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_UserId",
                table: "UserProfile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_TenantId",
                table: "UserTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_UserId",
                table: "UserTenants",
                column: "UserId");
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName", "TenantId", "Needful", "Level", "CopyInNewTenants" },
                values: new object[,]
                {
                    { "3aa0263c-8627-47ca-2321-08db65c3e91c", "TRANSLATORCURRENCYSTAMP", "Translator", "TRANSLATOR", 1, true, 80, true },
                    { "413a0665-db4e-46c3-b061-897960aa0054", null, "Owner", null, 1, true, 0, true },
                    { "46d0aee0-258f-4936-b185-78429efcdc9f", "0a07d5c4-2bef-4dd2-a7a7-d1c995172fc7", "SuperAdmin", "SUPERADMIN", 1, true, 0, true },
                    { "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7", "4062ec3e-457b-4032-a026-a7c83fd331f6", "Admin", "ADMIN", 1, true, 10, true },
                    { "a17619b7-df13-441d-b590-41c11be55290", null, "Marketing", null, 1, true, 0, true },
                    { "b71caece-8f01-4637-806a-a90741429c44", "b71caece-8f01-4637-806a-a90741427j44", "Operatore", "OPERATORE", 1, false, 90, false },
                    { "d23d7002-50ff-4017-951b-cd2357365641", null, "Legal", null, 1, true, 0, true },
                    { "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g", "a9647576-c31e-4eac-88c9-7d320a7757bb", "Monitoraggio", "MONITORAGGIO", 1, true, 50, true },
                    { "d7516446-57ef-4e2a-bd4a-6816218a7dfb", null, "Developer", null, 1, true, 0, true },
                    { "deda3101-7acd-4f7c-b18d-571af08fe304", "d8855456-5f10-4cac-9bde-82df6533e074", "User", "USER", 1, true, 999, true }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName", "LastIp", "LastAccess", "TenantId", "PasswordLastChange", "FreeFieldString1", "FreeFieldString2", "FreeFieldString3", "FreeFieldDateTime", "FreeFieldInt2", "FreeFieldInt1", "FreeFieldBoolean", "IsPasswordMd5", "Deleted", "FakeEmail" },
                values: new object[,]
                {
                    {
                        "207d02ef-7baa-4dca-a118-320f3ab0e853", 0, "77579746-3a2f-4fa0-b67e-a93fcec317c3", "unit.test@maestrale.it", true, true,
                        new DateTime(2023, 9, 20, 14, 49, 42, 944, DateTimeKind.Utc),
                        "UNIT.TEST@MAESTRALE.IT", "UNIT.TEST@MAESTRALE.IT",
                        "AQAAAAEAACcQAAAAEK0RfXbmugblTgu0DB6i2MPm2f4Ii0wxQD70/rTSx3+mVcm66Hk1IDJv69/b6yubdw==", null, false,
                        "JJV4LCMSOQXE2QWYNBJTFOZHRYX2PYZ4", false, "unit.test@maestrale.it", null,
                        new DateTime(2024, 2, 2, 13, 33, 41, 303, DateTimeKind.Utc), 1,
                        new DateTime(2023, 9, 4, 10, 8, 36, 383, DateTimeKind.Utc), null, null, null, null, null, null, null, true, false, null
                    },
                    {
                        "36030431-6ef3-4f66-bf9e-5f1945bd9408", 0, "d1176d5f-490a-4819-adf3-a9f9521af6c8", "superadmin.seeders@maestrale.it", true, true,
                        null, "SUPERADMIN.SEEDERS@MAESTRALE.IT", "SUPERADMIN.SEEDERS@MAESTRALE.IT",
                        "AQAAAAEAACcQAAAAEK0RfXbmugblTgu0DB6i2MPm2f4Ii0wxQD70/rTSx3+mVcm66Hk1IDJv69/b6yubdw==", null, false,
                        "DQYWJDUGCGK5TK5UFVRMFKFROSSL7LAL", false, "superadmin.seeders@maestrale.it", null,
                        new DateTime(2024, 2, 5, 16, 45, 8, 477, DateTimeKind.Utc), 1,
                        new DateTime(2024, 1, 29, 13, 40, 13, 067, DateTimeKind.Utc), null, null, null, null, null, null, null, true, false, null
                    }
                });

            migrationBuilder.InsertData(
                table: "CustomSetups",
                columns: new[] { "Id", "Generic", "MaintenanceAdmin", "Environment" },
                values: new object[,]
                {
                    { "0b3bf9c6-bd99-4b63-951e-a34955e83978", "", false, "web" },
                    { "b0f65154-43c3-49f9-9f8c-2956ed3f9c42", "", false, "app" }
                });

            migrationBuilder.InsertData(
                table: "LegalTerms",
                columns: new[] { "Title", "Note", "Code", "Active", "Language", "Version", "Id", "DataActivation", "Content" },
                values: new object[,]
                {
                    {
                        "Informativa sulla Privacy",
                        "Inserire la privacy policy del sito. Se devi apportare modifiche che richiedono l'approvazione degli utenti, crea una nuova versione.",
                        "privacyPolicy",
                        true,
                        "en",
                        "1.1",
                        "0bc749a1-74e4-4f63-b183-efece67551d6",
                        new DateTime(2023, 9, 16, 12, 33, 49, 733),
                        "<p>work in progress…</p>"
                    },
                    {
                        "Terms and conditions",
                        "Enter the Terms and Conditions of use of the site. If you need to make changes that require user approval, create a new version.",
                        "termsEndConditions",
                        true,
                        "en",
                        "1.0",
                        "0c65439c-ec6a-4156-a3fe-20a730572ddb",
                        new DateTime(2023, 9, 16, 8, 33, 49, 207),
                        "<p>We are updating the document, please try again later...</p>"
                    },
                    {
                        "Cookie Acceptance",
                        "It appears to users who access the site for the first time. Limit its length to avoid display problems on apps (Android and iOS smartphones).",
                        "cookiesAcceptance",
                        true,
                        "en",
                        "1.0",
                        "62100e88-8810-435b-b8a9-d499d478acf5",
                        null,
                        "<p>Work in progress…</p>"
                    },
                    {
                        "Informativa sulla Privacy",
                        "Inserire la privacy policy del sito. Se devi apportare modifiche che richiedono l'approvazione degli utenti, crea una nuova versione.",
                        "privacyPolicy",
                        true,
                        "it",
                        "1.0",
                        "883c2486-5438-4f1c-99e8-936afae32563",
                        new DateTime(2023, 10, 14, 17, 1, 43, 147),
                        "<p><span style=\"color:rgb(73,80,87);\">Per noi la privacy dei nostri clienti è di primaria importanza. La presente Informativa sulla privacy definisce quali dati vengono raccolti e il modo in cui gli stessi vengono utilizzati, divulgati, trasferiti e/o archiviati dall’azienda.</span></p>"
                    },
                    {
                        "Accettazione dei cookie",
                        "Compare agli utenti che accedono al sito per la prima volta. Limitarne la lunghezza per evitare problemi di visualizzazione sulle app (smartphone Android e iOS).",
                        "cookiesAcceptance",
                        true,
                        "it",
                        "1.0",
                        "924962f4-0cc8-4db2-9ed2-499833d130a8",
                        null,
                        "<p><strong>Apprezziamo la tua privacy</strong></p><p>Noi e i nostri partner utilizziamo tecnologie come i Cookies o il targeting ed elaboriamo dati personali come l'indirizzo IP o le informazioni del browser al fine di personalizzare la navigazione del sito. Queste tecnologie possono accedere al tuo dispositivo e aiutarci a mostrarti contenuti più pertinenti e migliorare la tua esperienza su Internet. Lo utilizziamo anche per misurare i risultati o allineare i contenuti del nostro sito web. Poiché apprezziamo la tua privacy, ti chiediamo il permesso di utilizzare le seguenti tecnologie.</p>"
                    },
                    {
                        "Termini e Condizioni",
                        "Inserire i Termini e le Condizioni d'uso del sito. Se devi apportare modifiche che richiedono l'approvazione degli utenti, crea una nuova versione.",
                        "termsEndConditions",
                        true,
                        "it",
                        "1.0",
                        "e200f1be-11ab-44c1-b302-f378ea17a843",
                        new DateTime(2023, 9, 16, 8, 3, 3, 360),
                        "<p>Stiamo aggiornando il documento, riprova in un secondo momento…</p>"
                    }
                });

            migrationBuilder.InsertData(
                table: "Setups",
                columns: new[] { "environment", "maintenance", "useRemoteFiles", "disableLog", "publicRegistration", "sliderPosition", "sliderPics", "availableLanguages", "defaultLanguage", "failedLoginAttempts", "previousPasswordsStored", "defaultUserPassword", "languageSetup", "minAppVersion", "useUrlStaticFiles", "passwordExpirationPeriod", "blockingPeriodDuration", "sliderRegistrationPosition", "sliderTermsPosition", "fixedHeader", "fixedFooter", "fixedSidebar", "bodyTabsShadow", "bodyTabsLine", "appThemeWhite", "headerShadow", "sidebarShadow", "headerLight", "sidebarLight", "headerBackground", "sidebarBackground", "refreshTokenExpiresIn", "accessTokenExpiresIn", "entitiesList", "routesList", "canChangeTenants", "internalChat", "internalNotifications", "pushNotifications", "canSearch", "registrationFields", "mailTokenExpiresIn", "mailerUsesAltText", "forceLoginRedirect", "defaultClaims", "beLanguage", "needRequestAssociation", "maeUsers", "googleCredentials", "thirdPartsAccesses", "useMD5", "logicDelete", "rolesForEditUsers", "applicationName" },
                values: new object[,] {
                    {
                        "web",
                        false,
                        false,
                        false,
                        true,
                        "left",
                        "[{\"page\":\"login\",\"title\":\"Title 1\",\"titleKey\":\"other.slider.slider1Title\",\"description\":\"Description 1 for login and welcome page\",\"descriptionKey\":\"other.slider.slider1Description\",\"url\":\"assets/images/originals/city.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"registration\",\"title\":\"Title 2\",\"titleKey\":\"\",\"description\":\"Description 2 for registration page\",\"descriptionKey\":\"\",\"url\":\"assets/images/originals/city.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"terms\",\"title\":\"Title 1\",\"titleKey\":\"\",\"description\":\"Description for terms page\",\"descriptionKey\":\"\",\"url\":\"assets/images/originals/citynights.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"login\",\"title\":\"Title 2\",\"titleKey\":\"other.slider.slider2Title\",\"description\":\"Description 2 for login and welcome page\",\"descriptionKey\":\"other.slider.slider2Description\",\"url\":\"assets/images/originals/citydark.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"login\",\"title\":\"Title 3\",\"titleKey\":\"other.slider.slider3Title\",\"description\":\"Description 3 for login and welcome page\",\"descriptionKey\":\"other.slider.slider3Description\",\"url\":\"assets/images/originals/citynights.jpg\",\"active\":true,\"byUrl\":true}]",
                        "[{\"active\":true,\"code\":\"it\",\"label\":\"Italiano\"},{\"active\":true,\"code\":\"en\",\"label\":\"English\"}]",
                        "it",
                        5,
                        3,
                        "Maestrale2004!",
                        "{\"component\":{\"gdpr\":{\"accept\":\"Accetto tutto\",\"acceptPartial\":\"Accetto i soli cookie tecnici\",\"consult\":\"Consulta\",\"moreDetails\":\"per maggiori dettagli\",\"privacyPolicy\":\"l'Informativa sulla privacy\",\"refuse\":\"Rifiuto\",\"waitPlease\":\"Attendere prego...\"},\"generalCrud\":{\"choiceEntity\":\"SCEGLI L'ENTITÀ\",\"create\":\"CREA\",\"crudOnEntity\":\"CRUD SULL'ENTITÀ\",\"filters\":\"FILTRI DI\\nRICERCA\",\"find\":\"CERCA\",\"newRecord\":\"NUOVO\\nRECORD\",\"records\":\"record\",\"retry\":\"Riprova\",\"unableFetchRoutes\":\"Errore nel recupero delle Entità.\",\"wait\":\"Attendere prego...\"},\"loginForm\":{\"accessError\":\"si è verificato un errore; riprovare!\",\"accountTemporarilyBlocked\":\"Il tuo account è <strong>temporaneamente bloccato</strong><br />\\n            poichè hai superato il massimo numero di tentativi errati consecutivi.<br />\\n            Potrai riprovare l'accesso a partire dal:\",\"attemptsLeft\":\"tentativi\",\"attention\":\"ATTENZIONE\",\"banned\":\"questa utenza è bloccata, accesso negato.\",\"beforeBlock\":\"prima che il tuo account venga temporaneamente bloccato.\",\"cancel\":\"Annulla\",\"clickToUpdate\":\"Clicca qui per inserire una nuova password\",\"confirm\":\"Conferma\",\"email\":\"E-mail\",\"expiredPassword\":\"la password inserita è corretta ma scaduta\",\"fbLogin\":\"Accedi con Facebook\",\"googleLogin\":\"Accedi con Google\",\"incorrectCredentials\":\"E-mail o password errati.\",\"logIn\":\"Accedi\",\"mandatoryFields\":\"Email e password sono obbligatori\",\"oneAttemptRemaining\":\"Ti rimane <strong>1 tentativo</strong>\",\"password\":\"Password\",\"passwordToUpdate\":\"la password inserita è corretta ma è necessario aggiornarla\",\"remain\":\"Ti rimangono\",\"rulesAcceptance\":\"ACCETTAZIONE DELLA PRIVACY-POLICY\",\"rulesDetails\":\"Effettuando l'accesso dichiari di accettare la nostra Politica sulla Privacy e di consentire l'utilizzo dei cookie.<br />Confermi?\"},\"messagesNotifications\":{\"goToMessages\":\"Vai ai Messaggi\",\"goToNotifications\":\"Vai alle Notifiche\",\"messages\":\"Messaggi\",\"noNotices\":\"Non ci sono nuove notifiche o messaggi\",\"notifications\":\"Notifiche\",\"notificationsAndMessages\":\"Notifiche e Messaggi\",\"unreadedMessages\":\"messaggi non letti\",\"unreadedNotifications\":\"notifiche non lette\",\"waitPlease\":\"Attendere prego...\",\"youHave\":\"Hai\"},\"registrationForm\":{\"accept\":\"Accetto i\",\"acceptanceRequired\":\"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\"allRequired\":\"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\"alreadyExixts\":\"Già registrato\",\"alreadyExixtsInfo\":\"La tua e-mail risulta già registrata. Prova a recuperare la password\",\"alreadySign\":\"Già registrato?\",\"and\":\"e\",\"birthCity\":\"Città di nascita\",\"birthDate\":\"Data di nascita\",\"birthProvince\":\"Provincia di nascita\",\"birthState\":\"Stato di nascita\",\"birthZIP\":\"C.A.P. di nascita\",\"confirmPassword\":\"Conferma la password\",\"contactEmail\":\"E-mail di contatto\",\"description\":\"Descrizione\",\"email\":\"E-mail\",\"emailContactInfo\":\"Per future comunicazioni puoi impostare un'e-mail diversa\",\"emailInfo\":\"Ti invieremo un'email per confermare l'indirizzo\",\"error\":\"Si è verificato un errore. Riprovare!\",\"failureToAccept\":\"Mancata accettazione\",\"firstName\":\"Nome\",\"fixedPhone\":\"Telefono fisso\",\"lastName\":\"Cognome\",\"loginToComplete\":\"Effettua il login per confermare il tuo profilo\",\"man\":\"Uomo\",\"mandatoryFields\":\"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\"minLenghtName\":\"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\"minLengthNames\":\"Inserisci almeno due caratteri nei campi Nome e Cognome\",\"mismatchPassword\":\"Le due Password non corrispondono\",\"missingData\":\"Dati mancanti\",\"missionComplete\":\"Operazione riuscita\",\"mobilePhone\":\"Telefono cellulare\",\"needAccept\":\"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy!\",\"nickName\":\"Nick-name\",\"nickNameInfo\":\"Inserisci il tuo soprannome\",\"occupation\":\"Occupazione\",\"password\":\"Password\",\"privacyPolicy\":\"l'Informativa sulla Privacy\",\"prohibitedOperation\":\"Operazione non consentita\",\"recordingDisabled\":\"La registrazione a questo portale non è consentita\",\"residenceAddress\":\"Indirizzo di residenza\",\"residenceCity\":\"Città di residenza\",\"residenceHouseNumber\":\"N° civico\",\"residenceProvince\":\"Provincia di residenza\",\"residenceState\":\"Stato di residenza\",\"residenceZIP\":\"C.A.P. di residenza\",\"sex\":\"Sesso\",\"sigIn\":\"Accedi\",\"signIn\":\"Accedi\",\"signUp\":\"Registrati\",\"taxId\":\"Codice fiscale\",\"terms\":\"Termini e Condizioni\",\"unactiveRegistration\":\"Registrazione non attiva.\",\"uncorrectConfirmPassword\":\"Il formato del campo 'Conferma Password' non è corretto\",\"uncorrectContactEmail\":\"Il formato del campo 'E-mail di Contatto' non è corretto\",\"uncorrectData\":\"Dati incorretti\",\"uncorrectEmail\":\"Il formato del campo 'E-mail' non è corretto\",\"uncorrectFormat\":\"Formato non corretto\",\"uncorrectPassword\":\"Il formato del campo 'Password' non è corretto\",\"uncorrectTaxId\":\"Il formato del campo 'Codice Fiscale' non è corretto\",\"woman\":\"Donna\"},\"setupMaster\":{\"aspect\":\"ASPETTO\",\"communications\":\"COMUNICAZIONI\",\"custom\":\"CUSTOM\",\"entities\":\"ENTITÁ\",\"general\":\"GENERALE\",\"integrations\":\"INTEGRAZIONI\",\"multiLanguage\":\"MULTI-LINGUA\",\"operators\":\"OPERATORI\",\"routes\":\"ROTTE\",\"security\":\"SICUREZZA\",\"sliders\":\"SLIDERS\",\"userProfile\":\"PROFILO UTENTE\"},\"setupSocial\":{\"facebook\":\"FACEBOOK\",\"google\":\"GOOGLE\",\"thirdParts\":\"TERZE PARTI\"},\"termsConditions\":{\"inReview\":\"Il documento è in fase di revisione. Riprovare in un secondo momento!\",\"unknownDocument\":\"Il documento richiesto non esiste!\",\"version\":\"Versione\",\"waitPlease\":\"Recupero in corso, attendere prego...\"},\"tr\":{\"choice\":\"Scegli la lingua\"},\"translateStructure\":{\"add\":\"AGGIUNGI\",\"addMacro\":\"AGGIUNGI MACRO-AREA\",\"addObject\":\"AGGIUNGI OGGETTO\",\"addVoice\":\"AGGIUNGI VOCE\",\"defaultStructure\":\"STRUTTURA DI DEFAULT\",\"defaultValue\":\"VALORE PREDEFINITO\",\"errorGettingData\":\"Errore nel recupero dei dati.\",\"find\":\"cerca...\",\"key\":\"CHIAVE\",\"name\":\"NOME\",\"nameLower\":\"Nome\",\"notAvailableValue\":\"Non sono ancora presenti valoriu0021\",\"retry\":\"Riprova\",\"stringKey\":\"Chiave della stringa\",\"type\":\"Tipologia\",\"value\":\"VALORE\",\"valueLower\":\"Valore\",\"wait\":\"Recupero dati in corso, attendere prego...\"},\"usersMaster\":{\"add\":\"CREA NUOVO UTENTE\",\"banned\":\"UTENTI BANNATI\",\"requests\":\"RICHIESTE DI AUTORIZZAZIONE\",\"users\":\"ELENCO UTENTI\"}},\"controller\":{},\"model\":{},\"other\":{\"slider\":{\"slider1Description\":\"Descrizione 1 per pagine di Login e Welcome\",\"slider1Title\":\"Titolo 1\",\"slider2Description\":\"Descrizione 2 per pagine di Login e Welcome\",\"slider2Title\":\"Titolo 2\",\"slider3Description\":\"Descrizione 3 per pagine di Login e Welcome\",\"slider3Title\":\"Titolo 3\"},\"utilsIncompleteConfig\":{\"cookiesAcceptance\":\"Accettazione Cookie\",\"privacyPolicy\":\"Informativa sulla Privacy\",\"termsEndConditions\":\"Termini e Condizioni\"}},\"template\":{\"articles\":{\"subTitle\":\"Gestisci gli articoli del sito\",\"title\":\"Articoli\"},\"categories\":{\"subTitle\":\"Gestisci le Categorie del sito\",\"title\":\"Categorie\"},\"changePassword\":{\"subTitle\":\"Cambia la tua password di accesso\",\"title\":\"Cambia password\"},\"developerGuide\":{\"subTitle\":\"La guida sviluppatori del framework\",\"title\":\"Guida per Sviluppatori\"},\"devices\":{\"subTitle\":\"Gestisci i dispositivi con cui sei collegato\",\"title\":\"I tuoi dispositivi\"},\"emptyLoggedPage\":{\"subTitle\":\"Sottotitolo\",\"title\":\"Titolo\"},\"generalCrud\":{\"subTitle\":\"Gestisci i dati delle tue tabelle\",\"title\":\"CRUD JSON:API\"},\"home\":{\"subTitle\":\"Frontend ember-based framework\",\"title\":\"Home\"},\"legals\":{\"subTitle\":\"Gestione dei documenti legali\",\"title\":\"Documenti Legali\"},\"legalsDetails\":{\"subTitle\":\"Gestisci i dettagli del documento\",\"title\":\"Modifica Documento Legale\"},\"loading\":{\"loading\":\"CARICAMENTO IN CORSO...\"},\"loggedHome\":{\"subTitle\":\"Frontend ember-based framework\",\"title\":\"Home\"},\"login\":{\"enterCredentials\":\"Inserisci le tue credenziali d'accesso.\",\"forgotPassword\":\"Password dimenticata?\",\"registration\":\"Registrati\",\"unregistered\":\"Non sei registrato?\",\"welcome\":\"Bentornato,\"},\"media\":{\"subTitle\":\"Gestisci i media dell'applicativo\",\"title\":\"Media\"},\"notifications\":{\"subTitle\":\"Visualizza l'elenco delle notifiche ricevute\",\"title\":\"Notifiche\"},\"recoveryPassword\":{\"backwards\":\"indietro\",\"email\":\"Email\",\"howTo\":\"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\"return\":\"Torna\",\"start\":\"Avvia il cambio password\",\"title\":\"Recupero della password\"},\"registration\":{\"howTo1\":\"crea il tuo account in\",\"howTo2\":\"pochi secondi\",\"welcome\":\"Benvenuto,\"},\"rolePermissions\":{\"crud\":\"PERMESSI CRUD\",\"roles\":\"RUOLI\",\"rolesPermissionsMapping\":\"ASSOCIAZIONI R&divide;P\",\"routes\":\"ROTTE\",\"subTitle\":\"Definisci ruoli e permessi di accesso\",\"tenants\":\"TENANT\",\"tenantsUsersMapping\":\"ASSOCIAZIONI T&divide;U\",\"title\":\"Autorizzazioni\",\"userRoles\":\"RUOLI UTENTI\"},\"setup\":{\"subTitle\":\"Imposta i parametri di funzionamento dell'applicativo\",\"title\":\"Setup\"},\"template\":{\"subTitle\":\"Modifica il template\",\"title\":\"Template\"},\"templateDetails\":{\"subTitle\":\"Modifica i dettagli del template\",\"title\":\"Modifica Template\"},\"templates\":{\"subTitle\":\"Gestisci i template delle comunicazioni e-mail\",\"title\":\"Template\"},\"tenantFallback\":{\"subTitle\":\"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\"title\":\"Verifica della licenza\"},\"terms\":{\"backwards\":\"indietro\",\"return\":\"Torna\"},\"translateStructure\":{\"subTitle\":\"Imposta la struttura del file di traduzione\",\"title\":\"Struttura traduzioni\"},\"translations\":{\"subTitle\":\"Traduci il sito nelle lingue disponibili.\",\"title\":\"Traduzioni\"},\"updatePassword\":{\"howto\":\"Scegli la tua nuova password\",\"title\":\"Modifica della password\"},\"userAudit\":{\"subTitle\":\"Monitora le chiamate http e i dati relativi\",\"title\":\"Audit\"},\"userGuide\":{\"subTitle\":\"La guida per l'utilizzo ottimale del portale\",\"title\":\"Guida d'uso e manutenzione\"},\"userProfile\":{\"subTitle\":\"Gestisci i tuoi dati personali\",\"title\":\"Profilo personale\"},\"users\":{\"subTitle\":\"Gestisci gli utenti dell'azienda\",\"title\":\"Utenti\"}}}",
                        "0.0.0",
                        true,
                        30,
                        5,
                        "right",
                        "left",
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        "black",
                        "white",
                        "bg-warning",
                        "bg-dark",
                        129600,
                        43200,
                        "[{\"id\":\"c1fb3854-4fe8-44f7-9a00-dbc4e18ab205\",\"entity\":\"BannedUser\",\"title\":\"Utenti Bannati\",\"key\":\"\",\"description\":\"Gestione degli utenti bannati\",\"keyDescription\":\"\"},{\"id\":\"d0faf5b4-7a5a-4307-8522-c9c890ae5f84\",\"entity\":\"Category\",\"title\":\"Categorie\",\"key\":\"\",\"description\":\"Gestione delle Categorie\",\"keyDescription\":\"\"},{\"id\":\"b51f314e-f48b-43fe-8858-d794ca2e581a\",\"entity\":\"CustomSetup\",\"title\":\"Custom Setup\",\"key\":\"\",\"description\":\"Gestione dei Setup custom\",\"keyDescription\":\"\"},{\"id\":\"6ae25157-459a-4333-a65e-269e0f4bbe43\",\"entity\":\"Integration\",\"title\":\"Integrazioni\",\"key\":\"\",\"description\":\"Gestione degli applicativi esterni che si integrano con questo portale\",\"keyDescription\":\"\"},{\"id\":\"3fc55fcf-963e-47d2-b254-7fdaaeeddf44\",\"entity\":\"LegalTerm\",\"title\":\"Documenti Legali\",\"key\":\"\",\"description\":\"Gestione dei documenti legali\",\"keyDescription\":\"\"},{\"id\":\"ca9f7bfa-6033-4e0c-bab7-be42921414a1\",\"entity\":\"MediaFile\",\"title\":\"Media\",\"key\":\"\",\"description\":\"Gestione di immagini e documenti\",\"keyDescription\":\"\"},{\"id\":\"d3e78101-a626-4c66-8569-dd696137f910\",\"entity\":\"Notification\",\"title\":\"Notifiche\",\"key\":\"\",\"description\":\"Gestione delle notifiche\",\"keyDescription\":\"\"},{\"id\":\"5ca22567-1b3a-4ce6-aac3-58ba086e08b6\",\"entity\":\"Role\",\"title\":\"Ruoli\",\"key\":\"\",\"description\":\"Gestione dei ruoli\",\"keyDescription\":\"\"},{\"id\":\"dcdba35f-6a2e-4b29-a759-543a4084288b\",\"entity\":\"RoleClaim\",\"title\":\"Mapping Ruoli Permessi\",\"key\":\"\",\"description\":\"Gestione del Mapping tra Ruoli e Permessi\",\"keyDescription\":\"\"},{\"id\":\"03debe9a-6a1c-4e50-85b1-cf5646483ed9\",\"entity\":\"Setup\",\"title\":\"Setup\",\"key\":\"\",\"description\":\"Gestione della configurazione generale\",\"keyDescription\":\"\"},{\"id\":\"5d78f60f-a8f9-4153-9b88-2bea37bd44b2\",\"entity\":\"Template\",\"title\":\"Template\",\"key\":\"\",\"description\":\"Gestione dei Template\",\"keyDescription\":\"\"},{\"id\":\"d11d0d30-be90-4859-8072-52e115276cab\",\"entity\":\"Tenant\",\"title\":\"Tenant\",\"key\":\"\",\"description\":\"Gestione dei Tenant\",\"keyDescription\":\"\"},{\"id\":\"e50055b6-4b46-41aa-93b8-b7fd5135800a\",\"entity\":\"Translation\",\"title\":\"Traduzioni\",\"key\":\"\",\"description\":\"Gestione delle Traduzioni\",\"keyDescription\":\"\"},{\"id\":\"e53ad211-8bb7-4e18-8640-fbd4cd31302b\",\"entity\":\"User\",\"title\":\"Utenti\",\"key\":\"\",\"description\":\"Gestione degli Utenti\",\"keyDescription\":\"\"},{\"id\":\"8d7f0fd9-5b1b-4705-885e-2bb064ce0130\",\"entity\":\"UserAudit\",\"title\":\"Audit\",\"key\":\"\",\"description\":\"Monitoraggio dei log\",\"keyDescription\":\"\"},{\"id\":\"2e5ec9a8-2fba-4da3-9630-612313d4a8c1\",\"entity\":\"UserDevice\",\"title\":\"Dispositivi degli utenti\",\"key\":\"\",\"description\":\"Gestione dei Dispositivi degli Utenti\",\"keyDescription\":\"\"},{\"id\":\"7df65992-7f43-413d-8000-9c84ed2311a6\",\"entity\":\"UserProfile\",\"title\":\"Profilo personale\",\"key\":\"\",\"description\":\"Gestione dei dati utente\",\"keyDescription\":\"\"},{\"id\":\"345d3c69-429f-4da4-85de-0ba1ba1fe471\",\"entity\":\"UserRole\",\"title\":\"Mappping Utenti Ruoli\",\"key\":\"\",\"description\":\"Gestione del Mapping tra Utenti e Ruoli\",\"keyDescription\":\"\"},{\"id\":\"5621ca14-ded2-4464-86da-705f80900de1\",\"entity\":\"UserTenant\",\"title\":\"UserTenant\",\"key\":\"\",\"description\":\"Associazioni tra Utenti e Tenant\",\"keyDescription\":\"\"},{\"id\":\"9518ea73-2222-4fce-9530-40cebc72228b\",\"entity\":\"MediaCategory\",\"title\":\"Categorie dei Media\",\"key\":\"\",\"description\":\"Gestisci le categorie di caricamento file\",\"keyDescription\":\"\"},{\"id\":\"7fc904bb-65ac-4634-ae8e-8782757dc35c\",\"entity\":\"Ticket\",\"title\":\"Ticket\",\"key\":\"\",\"description\":\"Richieste di Assistenza Cliente\",\"keyDescription\":\"\"}]",
                        "[{\"id\":\"2af8f5f4-32bd-4c99-948e-3da312103e53\",\"route\":\"articles\",\"title\":\"Articoli\",\"description\":\"Gestione degli articoli\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"1e1fbf6f-9542-48c0-b4f6-7174e37fe4da\",\"route\":\"categories\",\"title\":\"Categorie\",\"key\":\"\",\"description\":\"Gestione delle Categorie\"},{\"id\":\"4a40082d-1aa4-4af4-88e2-5efd3f45e1af\",\"route\":\"developer-guide\",\"title\":\"Guida sviluppatori\",\"key\":\"\",\"description\":\"Guida per lo sviluppo all'interno del fwk\"},{\"id\":\"99f93e7c-00db-404d-81ec-bdcb1edf3839\",\"route\":\"devices\",\"title\":\"Dispositivi\",\"key\":\"\",\"description\":\"Elenco a gestione dei dispositivi personali\"},{\"id\":\"aff40914-eb34-4cec-a8b2-2e3bdd443786\",\"route\":\"general-crud\",\"title\":\"Crud\",\"description\":\"Gestisci i dati contenuti nelle tabelle\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"e3596d93-bc20-4d83-9f65-e911141ab00e\",\"route\":\"legals\",\"title\":\"Legals\",\"description\":\"Elenco dei documenti legali\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"18a2b760-5fee-4b1a-857d-f8d78960ccf7\",\"route\":\"media\",\"title\":\"Media\",\"key\":\"\",\"description\":\"Gestione di immagini e documenti\"},{\"id\":\"7a6e074c-39a1-4bc0-b1bb-86e3d01012ea\",\"route\":\"roles-permissions\",\"title\":\"Autorizzazioni\",\"key\":\"\",\"description\":\"Gestione di Tenant, Ruoli e Permessi\"},{\"id\":\"d1fc01a0-604d-403b-8723-b01b7e5f062d\",\"route\":\"setup\",\"title\":\"Setup\",\"key\":\"\",\"description\":\"Configurazione generale del sito\"},{\"id\":\"cb30357c-5b84-46ed-b9b0-ad91fd699e80\",\"route\":\"template-details\",\"title\":\"Modifica Template\",\"key\":\"\",\"description\":\"Modifica dei singoli template e-mail\"},{\"id\":\"797caf5e-6c05-4285-8798-2d4f3de0723b\",\"route\":\"templates\",\"title\":\"Elenco dei Template\",\"key\":\"\",\"description\":\"Elenco dei templates e-mail\"},{\"id\":\"4414a075-353b-43dc-b910-10b346f8b4e2\",\"route\":\"translate-structure\",\"title\":\"Struttura delle Traduzioni\",\"key\":\"\",\"description\":\"Definizione della struttura dei file di Traduzione\"},{\"id\":\"a472d210-d80d-4963-9f13-f4fbf1653ae4\",\"route\":\"translations\",\"title\":\"Traduzioni\",\"key\":\"\",\"description\":\"Gestione delle Traduzioni\"},{\"id\":\"32a9ecd3-5dc0-4dff-87d4-5e5e16d0c9a5\",\"route\":\"user-audit\",\"title\":\"Audit\",\"key\":\"\",\"description\":\"Accesso ai log di monitoraggio\"},{\"id\":\"21539ae0-de0a-43b3-98e1-4b40f062ae99\",\"route\":\"user-guide\",\"title\":\"Guida d'uso\",\"key\":\"\",\"description\":\"Guida per il corretto utilizzo del portale\"},{\"id\":\"92327e92-d49b-404b-9e30-8d6697487563\",\"route\":\"users\",\"title\":\"Utenti\",\"key\":\"\",\"description\":\"Gestione degli Utenti\"},{\"id\":\"4a6e7fb7-21b6-4e87-a598-d5e94c4bd380\",\"route\":\"help-desk-admin\",\"title\":\"Help Desk Admin\",\"description\":\"Gestione delle Richieste di Assistenza\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"ffbcc4c4-6645-4ff4-b0bc-ce007bc32f38\",\"route\":\"media-categories\",\"title\":\"Categorie dei Media\",\"description\":\"Gestisci l'elenco delle Categorie dei Media\",\"key\":\"\",\"keyDescription\":\"\"}]",
                        true,
                        "2",
                        true,
                        false,
                        false,
                        "{\"registration\":{\"firstName\":\"2\",\"lastName\":\"2\",\"email\":\"2\",\"contactEmail\":\"0\",\"nickName\":\"1\",\"sex\":\"1\",\"taxId\":\"1\",\"fixedPhone\":\"1\",\"mobilePhone\":\"1\",\"birthDate\":\"1\",\"birthProvince\":\"1\",\"birthZIP\":\"1\",\"birthState\":\"1\",\"residenceCity\":\"1\",\"residenceProvince\":\"1\",\"residenceZIP\":\"1\",\"residenceState\":\"1\",\"residenceAddress\":\"1\",\"residenceHouseNumber\":\"1\",\"occupation\":\"1\",\"description\":\"1\",\"birthCity\":\"1\"},\"profile\":{\"firstName\":\"2\",\"lastName\":\"2\",\"email\":\"2\",\"contactEmail\":\"0\",\"nickName\":\"0\",\"sex\":\"0\",\"taxId\":\"0\",\"fixedPhone\":\"0\",\"mobilePhone\":\"0\",\"birthDate\":\"0\",\"birthProvince\":\"0\",\"birthZIP\":\"0\",\"birthState\":\"0\",\"residenceCity\":\"0\",\"residenceProvince\":\"0\",\"residenceZIP\":\"0\",\"residenceState\":\"0\",\"residenceAddress\":\"0\",\"residenceHouseNumber\":\"0\",\"occupation\":\"0\",\"description\":\"1\"}}",
                        2880,
                        false,
                        true,
                        "[{\"name\":\"canBypassMaintenance\",\"description\":\"Accede anche quando il sito è in manutenzione\"},{\"name\":\"canCreateSelfCustomerUser\",\"description\":\"Può creare utenti CustomerEmployee nel proprio Tenant\"},{\"name\":\"canSeeAllTenants\",\"description\":\"Può vedere anche i Tenant che non sono associati al suo profilo\"},{\"name\":\"canSeeIncompleteConfigurations\",\"description\":\"Può vedere il menù delle configurazioni incomplete\"},{\"name\":\"canSeeRightBar\",\"description\":\"Può vedere il menù laterale destro\"},{\"name\":\"isAdmin\",\"description\":\"Contraddistingue l'Admin\"},{\"name\":\"isAudit\",\"description\":\"Contraddistingue gli utenti del gruppo Audit (monitoraggio)\"},{\"name\":\"isDeveloper\",\"description\":\"Contraddistingue gli utenti del gruppo Developer (sviluppatori)\"},{\"name\":\"isMarketing\",\"description\":\"Contraddistingue gli utenti del reparto Marketing\"},{\"name\":\"isOwner\",\"description\":\"Contraddistingue il titolare del sito/app\"},{\"name\":\"isSuperAdmin\",\"description\":\"Contraddistingue il super-admin\"},{\"name\":\"isTranslator\",\"description\":\"Contraddistingue i traduttori\"},{\"name\":\"isUser\",\"description\":\"Contraddistingue gli utenti generici\"}]",
                        "C#",
                        true,
                        "[{\"lastName\":\"Bellavigna\",\"firstName\":\"Gianluca\",\"email\":\"gianluca.bellavigna@maestrale.it\"},{\"lastName\":\"Biagetti\",\"firstName\":\"Sacha\",\"email\":\"sacha.biagetti@maestrale.it\"},{\"lastName\":\"Cipriani\",\"firstName\":\"Samuele\",\"email\":\"samuele.cipriani@maestrale.it\"},{\"lastName\":\"Gallorini\",\"firstName\":\"Veronica\",\"email\":\"veronica.gallorini@maestrale.it\"},{\"lastName\":\"Giannotta\",\"firstName\":\"Luigi\",\"email\":\"luigi.giannotta@maestrale.it\"},{\"lastName\":\"Marcelli\",\"firstName\":\"Andrea\",\"email\":\"andrea.marcelli@maestrale.it\"},{\"lastName\":\"Morganti\",\"firstName\":\"Emanulele\",\"email\":\"emanuele.morganti@maestrale.it\"},{\"lastName\":\"Pellerucci\",\"firstName\":\"Daniele\",\"email\":\"daniele.pellerucci@maestrale.it\"},{\"lastName\":\"Penzo\",\"firstName\":\"Stefano\",\"email\":\"stefano.penzo@maestrale.it\"},{\"lastName\":\"Perotti\",\"firstName\":\"Luca\",\"email\":\"luca.perotti@maestrale.it\"},{\"lastName\":\"Ratini\",\"firstName\":\"Riccardo\",\"email\":\"riccardo.ratini@maestrale.it\"},{\"lastName\":\"Sabatini\",\"firstName\":\"Fabio\",\"email\":\"fabio.sabatini@maestrale.it\"},{\"lastName\":\"Vitali\",\"firstName\":\"Roberto\",\"email\":\"roberto.vitali@maestrale.it\"}]",
                        "{\"googleIdSite\":\"\",\"redirectUriLogin\":\"\",\"redirectAfterGoogleLogin\":\"\",\"redirectAfterGoogleRegister\":\"\",\"redirectAfterGoogleError\":\"\",\"googleAPIKey\":\"\",\"googleSecret\":\"\",\"googleAPPId\":\"\",\"googleScopes\":\"\"}",
                        "{\"googleEnabled\":false,\"facebookEnabled\":false}",
                        true,
                        true,
                        "",
                        ""
                    },
                    {
                        "app",
                        false,
                        false,
                        false,
                        true,
                        "left",
                        "[{\"page\":\"login\",\"title\":\"Title 1\",\"titleKey\":\"other.slider.slider1Title\",\"description\":\"Description 1 for login and welcome page\",\"descriptionKey\":\"other.slider.slider1Description\",\"url\":\"assets/images/originals/city.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"registration\",\"title\":\"Title 2\",\"titleKey\":\"\",\"description\":\"Description 2 for registration page\",\"descriptionKey\":\"\",\"url\":\"assets/images/originals/city.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"terms\",\"title\":\"Title 1\",\"titleKey\":\"\",\"description\":\"Description for terms page\",\"descriptionKey\":\"\",\"url\":\"assets/images/originals/citynights.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"login\",\"title\":\"Title 2\",\"titleKey\":\"other.slider.slider2Title\",\"description\":\"Description 2 for login and welcome page\",\"descriptionKey\":\"other.slider.slider2Description\",\"url\":\"assets/images/originals/citydark.jpg\",\"active\":true,\"byUrl\":true},{\"page\":\"login\",\"title\":\"Title 3\",\"titleKey\":\"other.slider.slider3Title\",\"description\":\"Description 3 for login and welcome page\",\"descriptionKey\":\"other.slider.slider3Description\",\"url\":\"assets/images/originals/citynights.jpg\",\"active\":true,\"byUrl\":true}]",
                        "[{\"active\":true,\"code\":\"it\",\"label\":\"Italiano\"},{\"active\":true,\"code\":\"en\",\"label\":\"English\"}]",
                        "it",
                        5,
                        3,
                        "Maestrale2004!",
                        "{\"component\":{\"gdpr\":{\"accept\":\"Accetto tutto\",\"acceptPartial\":\"Accetto i soli cookie tecnici\",\"consult\":\"Consulta\",\"moreDetails\":\"per maggiori dettagli\",\"privacyPolicy\":\"l'Informativa sulla privacy\",\"refuse\":\"Rifiuto\",\"waitPlease\":\"Attendere prego...\"},\"generalCrud\":{\"choiceEntity\":\"SCEGLI L'ENTITÀ\",\"create\":\"CREA\",\"crudOnEntity\":\"CRUD SULL'ENTITÀ\",\"filters\":\"FILTRI DI<br />RICERCA\",\"find\":\"CERCA\",\"newRecord\":\"NUOVO<br />RECORD\",\"records\":\"record\",\"retry\":\"Riprova\",\"unableFetchRoutes\":\"Errore nel recupero delle Entità.\",\"wait\":\"Attendere prego...\"},\"loginForm\":{\"accessError\":\"si è verificato un errore; riprovare!\",\"accountTemporarilyBlocked\":\"Il tuo account è <strong>temporaneamente bloccato</strong><br />\\n            poichè hai superato il massimo numero di tentativi errati consecutivi.<br />\\n            Potrai riprovare l'accesso a partire dal:\",\"attemptsLeft\":\"tentativi\",\"attention\":\"ATTENZIONE\",\"banned\":\"questa utenza è bloccata, accesso negato.\",\"beforeBlock\":\"prima che il tuo account venga temporaneamente bloccato.\",\"cancel\":\"Annulla\",\"clickToUpdate\":\"Clicca qui per inserire una nuova password\",\"confirm\":\"Conferma\",\"email\":\"E-mail\",\"expiredPassword\":\"la password inserita è corretta ma scaduta\",\"fbLogin\":\"Accedi con Facebook\",\"googleLogin\":\"Accedi con Google\",\"incorrectCredentials\":\"E-mail o password errati.\",\"logIn\":\"Accedi\",\"mandatoryFields\":\"Email e password sono obbligatori\",\"oneAttemptRemaining\":\"Ti rimane <strong>1 tentativo</strong>\",\"password\":\"Password\",\"passwordToUpdate\":\"la password inserita è corretta ma è necessario aggiornarla\",\"remain\":\"Ti rimangono\",\"rulesAcceptance\":\"ACCETTAZIONE DELLA PRIVACY-POLICY\",\"rulesDetails\":\"Effettuando l'accesso dichiari di accettare la nostra Politica sulla Privacy e di consentire l'utilizzo dei cookie.<br />Confermi?\"},\"messagesNotifications\":{\"goToMessages\":\"Vai ai Messaggi\",\"goToNotifications\":\"Vai alle Notifiche\",\"messages\":\"Messaggi\",\"noNotices\":\"Non ci sono nuove notifiche o messaggi\",\"notifications\":\"Notifiche\",\"notificationsAndMessages\":\"Notifiche e Messaggi\",\"unreadedMessages\":\"messaggi non letti\",\"unreadedNotifications\":\"notifiche non lette\",\"waitPlease\":\"Attendere prego...\",\"youHave\":\"Hai\"},\"registrationForm\":{\"accept\":\"Accetto i\",\"acceptanceRequired\":\"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\"allRequired\":\"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\"alreadyExixts\":\"Già registrato\",\"alreadyExixtsInfo\":\"La tua e-mail risulta già registrata. Prova a recuperare la password\",\"alreadySign\":\"Già registrato?\",\"and\":\"e\",\"birthCity\":\"Città di nascita\",\"birthDate\":\"Data di nascita\",\"birthProvince\":\"Provincia di nascita\",\"birthState\":\"Stato di nascita\",\"birthZIP\":\"C.A.P. di nascita\",\"confirmPassword\":\"Conferma la password\",\"contactEmail\":\"E-mail di contatto\",\"description\":\"Descrizione\",\"email\":\"E-mail\",\"emailContactInfo\":\"Per future comunicazioni puoi impostare un'e-mail diversa\",\"emailInfo\":\"Ti invieremo un'email per confermare l'indirizzo\",\"error\":\"Si è verificato un errore. Riprovare!\",\"failureToAccept\":\"Mancata accettazione\",\"firstName\":\"Nome\",\"fixedPhone\":\"Telefono fisso\",\"lastName\":\"Cognome\",\"loginToComplete\":\"Effettua il login per confermare il tuo profilo\",\"man\":\"Uomo\",\"mandatoryFields\":\"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\"minLenghtName\":\"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\"minLengthNames\":\"Inserisci almeno due caratteri nei campi Nome e Cognome\",\"mismatchPassword\":\"Le due Password non corrispondono\",\"missingData\":\"Dati mancanti\",\"missionComplete\":\"Operazione riuscita\",\"mobilePhone\":\"Telefono cellulare\",\"needAccept\":\"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy!\",\"nickName\":\"Nick-name\",\"nickNameInfo\":\"Inserisci il tuo soprannome\",\"occupation\":\"Occupazione\",\"password\":\"Password\",\"privacyPolicy\":\"l'Informativa sulla Privacy\",\"prohibitedOperation\":\"Operazione non consentita\",\"recordingDisabled\":\"La registrazione a questo portale non è consentita\",\"residenceAddress\":\"Indirizzo di residenza\",\"residenceCity\":\"Città di residenza\",\"residenceHouseNumber\":\"N° civico\",\"residenceProvince\":\"Provincia di residenza\",\"residenceState\":\"Stato di residenza\",\"residenceZIP\":\"C.A.P. di residenza\",\"sex\":\"Sesso\",\"sigIn\":\"Accedi\",\"signIn\":\"Accedi\",\"signUp\":\"Registrati\",\"taxId\":\"Codice fiscale\",\"terms\":\"Termini e Condizioni\",\"unactiveRegistration\":\"Registrazione non attiva.\",\"uncorrectConfirmPassword\":\"Il formato del campo 'Conferma Password' non è corretto\",\"uncorrectContactEmail\":\"Il formato del campo 'E-mail di Contatto' non è corretto\",\"uncorrectData\":\"Dati incorretti\",\"uncorrectEmail\":\"Il formato del campo 'E-mail' non è corretto\",\"uncorrectFormat\":\"Formato non corretto\",\"uncorrectPassword\":\"Il formato del campo 'Password' non è corretto\",\"uncorrectTaxId\":\"Il formato del campo 'Codice Fiscale' non è corretto\",\"woman\":\"Donna\"},\"setupMaster\":{\"aspect\":\"ASPETTO\",\"communications\":\"COMUNICAZIONI\",\"custom\":\"CUSTOM\",\"entities\":\"ENTITÁ\",\"general\":\"GENERALE\",\"integrations\":\"INTEGRAZIONI\",\"multiLanguage\":\"MULTI-LINGUA\",\"operators\":\"OPERATORI\",\"routes\":\"ROTTE\",\"security\":\"SICUREZZA\",\"sliders\":\"SLIDERS\",\"userProfile\":\"PROFILO UTENTE\"},\"setupSocial\":{\"facebook\":\"FACEBOOK\",\"google\":\"GOOGLE\",\"thirdParts\":\"TERZE PARTI\"},\"termsConditions\":{\"inReview\":\"Il documento è in fase di revisione. Riprovare in un secondo momento!\",\"unknownDocument\":\"Il documento richiesto non esiste!\",\"version\":\"Versione\",\"waitPlease\":\"Recupero in corso, attendere prego...\"},\"tr\":{\"choice\":\"Scegli la lingua\"},\"translateStructure\":{\"add\":\"AGGIUNGI\",\"addMacro\":\"AGGIUNGI MACRO-AREA\",\"addObject\":\"AGGIUNGI OGGETTO\",\"addVoice\":\"AGGIUNGI VOCE\",\"defaultStructure\":\"STRUTTURA DI DEFAULT\",\"defaultValue\":\"VALORE PREDEFINITO\",\"errorGettingData\":\"Errore nel recupero dei dati.\",\"find\":\"cerca...\",\"key\":\"CHIAVE\",\"name\":\"NOME\",\"nameLower\":\"Nome\",\"notAvailableValue\":\"Non sono ancora presenti valoriu0021\",\"retry\":\"Riprova\",\"stringKey\":\"Chiave della stringa\",\"type\":\"Tipologia\",\"value\":\"VALORE\",\"valueLower\":\"Valore\",\"wait\":\"Recupero dati in corso, attendere prego...\"},\"usersMaster\":{\"add\":\"CREA NUOVO UTENTE\",\"banned\":\"UTENTI BANNATI\",\"requests\":\"RICHIESTE DI AUTORIZZAZIONE\",\"users\":\"ELENCO UTENTI\"}},\"controller\":{},\"model\":{},\"other\":{\"slider\":{\"slider1Description\":\"Descrizione 1 per pagine di Login e Welcome\",\"slider1Title\":\"Titolo 1\",\"slider2Description\":\"Descrizione 2 per pagine di Login e Welcome\",\"slider2Title\":\"Titolo 2\",\"slider3Description\":\"Descrizione 3 per pagine di Login e Welcome\",\"slider3Title\":\"Titolo 3\"},\"utilsIncompleteConfig\":{\"cookiesAcceptance\":\"Accettazione Cookie\",\"privacyPolicy\":\"Informativa sulla Privacy\",\"termsEndConditions\":\"Termini e Condizioni\"}},\"template\":{\"articles\":{\"subTitle\":\"Gestisci gli articoli del sito\",\"title\":\"Articoli\"},\"categories\":{\"subTitle\":\"Gestisci le Categorie del sito\",\"title\":\"Categorie\"},\"changePassword\":{\"subTitle\":\"Cambia la tua password di accesso\",\"title\":\"Cambia password\"},\"developerGuide\":{\"subTitle\":\"La guida sviluppatori del framework\",\"title\":\"Guida per Sviluppatori\"},\"devices\":{\"subTitle\":\"Gestisci i dispositivi con cui sei collegato\",\"title\":\"I tuoi dispositivi\"},\"emptyLoggedPage\":{\"subTitle\":\"Sottotitolo\",\"title\":\"Titolo\"},\"generalCrud\":{\"subTitle\":\"Gestisci i dati delle tue tabelle\",\"title\":\"CRUD JSON:API\"},\"home\":{\"subTitle\":\"Frontend ember-based framework\",\"title\":\"Home\"},\"legals\":{\"subTitle\":\"Gestione dei documenti legali\",\"title\":\"Documenti Legali\"},\"legalsDetails\":{\"subTitle\":\"Gestisci i dettagli del documento\",\"title\":\"Modifica Documento Legale\"},\"loading\":{\"loading\":\"CARICAMENTO IN CORSO...\"},\"loggedHome\":{\"subTitle\":\"Frontend ember-based framework\",\"title\":\"Home\"},\"login\":{\"enterCredentials\":\"Inserisci le tue credenziali d'accesso.\",\"forgotPassword\":\"Password dimenticata?\",\"registration\":\"Registrati\",\"unregistered\":\"Non sei registrato?\",\"welcome\":\"Bentornato,\"},\"media\":{\"subTitle\":\"Gestisci i media dell'applicativo\",\"title\":\"Media\"},\"notifications\":{\"subTitle\":\"Visualizza l'elenco delle notifiche ricevute\",\"title\":\"Notifiche\"},\"recoveryPassword\":{\"backwards\":\"indietro\",\"email\":\"Email\",\"howTo\":\"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\"return\":\"Torna\",\"start\":\"Avvia il cambio password\",\"title\":\"Recupero della password\"},\"registration\":{\"howTo1\":\"crea il tuo account in\",\"howTo2\":\"pochi secondi\",\"welcome\":\"Benvenuto,\"},\"rolePermissions\":{\"crud\":\"PERMESSI CRUD\",\"roles\":\"RUOLI\",\"rolesPermissionsMapping\":\"ASSOCIAZIONI R&divide;P\",\"routes\":\"ROTTE\",\"subTitle\":\"Definisci ruoli e permessi di accesso\",\"tenants\":\"TENANT\",\"tenantsUsersMapping\":\"ASSOCIAZIONI T&divide;U\",\"title\":\"Autorizzazioni\",\"userRoles\":\"RUOLI UTENTI\"},\"setup\":{\"subTitle\":\"Imposta i parametri di funzionamento dell'applicativo\",\"title\":\"Setup\"},\"template\":{\"subTitle\":\"Modifica il template\",\"title\":\"Template\"},\"templateDetails\":{\"subTitle\":\"Modifica i dettagli del template\",\"title\":\"Modifica Template\"},\"templates\":{\"subTitle\":\"Gestisci i template delle comunicazioni e-mail\",\"title\":\"Template\"},\"tenantFallback\":{\"subTitle\":\"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\"title\":\"Verifica della licenza\"},\"terms\":{\"backwards\":\"indietro\",\"return\":\"Torna\"},\"translateStructure\":{\"subTitle\":\"Imposta la struttura del file di traduzione\",\"title\":\"Struttura traduzioni\"},\"translations\":{\"subTitle\":\"Traduci il sito nelle lingue disponibili.\",\"title\":\"Traduzioni\"},\"updatePassword\":{\"howto\":\"Scegli la tua nuova password\",\"title\":\"Modifica della password\"},\"userAudit\":{\"subTitle\":\"Monitora le chiamate http e i dati relativi\",\"title\":\"Audit\"},\"userGuide\":{\"subTitle\":\"La guida per l'utilizzo ottimale del portale\",\"title\":\"Guida d'uso e manutenzione\"},\"userProfile\":{\"subTitle\":\"Gestisci i tuoi dati personali\",\"title\":\"Profilo personale\"},\"users\":{\"subTitle\":\"Gestisci gli utenti dell'azienda\",\"title\":\"Utenti\"}}}",
                        "0.0.0",
                        true,
                        30,
                        5,
                        "right",
                        "left",
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        true,
                        "black",
                        "white",
                        "bg-warning",
                        "bg-dark",
                        129600,
                        43200,
                        "[{\"id\":\"c1fb3854-4fe8-44f7-9a00-dbc4e18ab205\",\"entity\":\"BannedUser\",\"title\":\"Utenti Bannati\",\"key\":\"\",\"description\":\"Gestione degli utenti bannati\",\"keyDescription\":\"\"},{\"id\":\"d0faf5b4-7a5a-4307-8522-c9c890ae5f84\",\"entity\":\"Category\",\"title\":\"Categorie\",\"key\":\"\",\"description\":\"Gestione delle Categorie\",\"keyDescription\":\"\"},{\"id\":\"6ae25157-459a-4333-a65e-269e0f4bbe43\",\"entity\":\"Integration\",\"title\":\"Integrazioni\",\"key\":\"\",\"description\":\"Gestione degli applicativi esterni che si integrano con questo portale\",\"keyDescription\":\"\"},{\"id\":\"3fc55fcf-963e-47d2-b254-7fdaaeeddf44\",\"entity\":\"LegalTerm\",\"title\":\"Documenti Legali\",\"key\":\"\",\"description\":\"Gestione dei documenti legali\",\"keyDescription\":\"\"},{\"id\":\"ca9f7bfa-6033-4e0c-bab7-be42921414a1\",\"entity\":\"MediaFile\",\"title\":\"Media\",\"key\":\"\",\"description\":\"Gestione di immagini e documenti\",\"keyDescription\":\"\"},{\"id\":\"d3e78101-a626-4c66-8569-dd696137f910\",\"entity\":\"Notification\",\"title\":\"Notifiche\",\"key\":\"\",\"description\":\"Gestione delle notifiche\",\"keyDescription\":\"\"},{\"id\":\"5ca22567-1b3a-4ce6-aac3-58ba086e08b6\",\"entity\":\"Role\",\"title\":\"Ruoli\",\"key\":\"\",\"description\":\"Gestione dei ruoli\",\"keyDescription\":\"\"},{\"id\":\"dcdba35f-6a2e-4b29-a759-543a4084288b\",\"entity\":\"RoleClaim\",\"title\":\"Mapping Ruoli Permessi\",\"key\":\"\",\"description\":\"Gestione del Mapping tra Ruoli e Permessi\",\"keyDescription\":\"\"},{\"id\":\"03debe9a-6a1c-4e50-85b1-cf5646483ed9\",\"entity\":\"Setup\",\"title\":\"Setup\",\"key\":\"\",\"description\":\"Gestione della configurazione generale\",\"keyDescription\":\"\"},{\"id\":\"5d78f60f-a8f9-4153-9b88-2bea37bd44b2\",\"entity\":\"Template\",\"title\":\"Template\",\"key\":\"\",\"description\":\"Gestione dei Template\",\"keyDescription\":\"\"},{\"id\":\"d11d0d30-be90-4859-8072-52e115276cab\",\"entity\":\"Tenant\",\"title\":\"Tenant\",\"key\":\"\",\"description\":\"Gestione dei Tenant\",\"keyDescription\":\"\"},{\"id\":\"e50055b6-4b46-41aa-93b8-b7fd5135800a\",\"entity\":\"Translation\",\"title\":\"Traduzioni\",\"key\":\"\",\"description\":\"Gestione delle Traduzioni\",\"keyDescription\":\"\"},{\"id\":\"e53ad211-8bb7-4e18-8640-fbd4cd31302b\",\"entity\":\"User\",\"title\":\"Utenti\",\"key\":\"\",\"description\":\"Gestione degli Utenti\",\"keyDescription\":\"\"},{\"id\":\"8d7f0fd9-5b1b-4705-885e-2bb064ce0130\",\"entity\":\"UserAudit\",\"title\":\"Audit\",\"key\":\"\",\"description\":\"Monitoraggio dei log\",\"keyDescription\":\"\"},{\"id\":\"2e5ec9a8-2fba-4da3-9630-612313d4a8c1\",\"entity\":\"UserDevice\",\"title\":\"Dispositivi degli utenti\",\"key\":\"\",\"description\":\"Gestione dei Dispositivi degli Utenti\",\"keyDescription\":\"\"},{\"id\":\"7df65992-7f43-413d-8000-9c84ed2311a6\",\"entity\":\"UserProfile\",\"title\":\"Profilo personale\",\"key\":\"\",\"description\":\"Gestione dei dati utente\",\"keyDescription\":\"\"},{\"id\":\"345d3c69-429f-4da4-85de-0ba1ba1fe471\",\"entity\":\"UserRole\",\"title\":\"Mappping Utenti Ruoli\",\"key\":\"\",\"description\":\"Gestione del Mapping tra Utenti e Ruoli\",\"keyDescription\":\"\"},{\"id\":\"5621ca14-ded2-4464-86da-705f80900de1\",\"entity\":\"UserTenant\",\"title\":\"UserTenant\",\"key\":\"\",\"description\":\"Associazioni tra Utenti e Tenant\",\"keyDescription\":\"\"},{\"id\":\"b51f314e-f48b-43fe-8858-d794ca2e581a\",\"entity\":\"CustomSetup\",\"title\":\"Custom Setup\",\"key\":\"\",\"description\":\"Gestione dei Setup custom\",\"keyDescription\":\"\"},{\"id\":\"9518ea73-2222-4fce-9530-40cebc72228b\",\"entity\":\"MediaCategory\",\"title\":\"Categorie dei Media\",\"key\":\"\",\"description\":\"Gestisci le categorie di caricamento file\",\"keyDescription\":\"\"},{\"id\":\"7fc904bb-65ac-4634-ae8e-8782757dc35c\",\"entity\":\"Ticket\",\"title\":\"Ticket\",\"key\":\"\",\"description\":\"Richieste di Assistenza Cliente\",\"keyDescription\":\"\"}]",
                        "[{\"id\":\"2af8f5f4-32bd-4c99-948e-3da312103e53\",\"route\":\"articles\",\"title\":\"Articoli\",\"description\":\"Gestione degli articoli\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"1e1fbf6f-9542-48c0-b4f6-7174e37fe4da\",\"route\":\"categories\",\"title\":\"Categorie\",\"key\":\"\",\"description\":\"Gestione delle Categorie\"},{\"id\":\"4a40082d-1aa4-4af4-88e2-5efd3f45e1af\",\"route\":\"developer-guide\",\"title\":\"Guida sviluppatori\",\"key\":\"\",\"description\":\"Guida per lo sviluppo all'interno del fwk\"},{\"id\":\"99f93e7c-00db-404d-81ec-bdcb1edf3839\",\"route\":\"devices\",\"title\":\"Dispositivi\",\"key\":\"\",\"description\":\"Elenco a gestione dei dispositivi personali\"},{\"id\":\"aff40914-eb34-4cec-a8b2-2e3bdd443786\",\"route\":\"general-crud\",\"title\":\"Crud\",\"description\":\"Gestisci i dati contenuti nelle tabelle\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"e3596d93-bc20-4d83-9f65-e911141ab00e\",\"route\":\"legals\",\"title\":\"Legals\",\"description\":\"Elenco dei documenti legali\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"18a2b760-5fee-4b1a-857d-f8d78960ccf7\",\"route\":\"media\",\"title\":\"Media\",\"key\":\"\",\"description\":\"Gestione di immagini e documenti\"},{\"id\":\"7a6e074c-39a1-4bc0-b1bb-86e3d01012ea\",\"route\":\"roles-permissions\",\"title\":\"Autorizzazioni\",\"key\":\"\",\"description\":\"Gestione di Tenant, Ruoli e Permessi\"},{\"id\":\"d1fc01a0-604d-403b-8723-b01b7e5f062d\",\"route\":\"setup\",\"title\":\"Setup\",\"key\":\"\",\"description\":\"Configurazione generale del sito\"},{\"id\":\"cb30357c-5b84-46ed-b9b0-ad91fd699e80\",\"route\":\"template-details\",\"title\":\"Modifica Template\",\"key\":\"\",\"description\":\"Modifica dei singoli template e-mail\"},{\"id\":\"797caf5e-6c05-4285-8798-2d4f3de0723b\",\"route\":\"templates\",\"title\":\"Elenco dei Template\",\"key\":\"\",\"description\":\"Elenco dei templates e-mail\"},{\"id\":\"4414a075-353b-43dc-b910-10b346f8b4e2\",\"route\":\"translate-structure\",\"title\":\"Struttura delle Traduzioni\",\"key\":\"\",\"description\":\"Definizione della struttura dei file di Traduzione\"},{\"id\":\"a472d210-d80d-4963-9f13-f4fbf1653ae4\",\"route\":\"translations\",\"title\":\"Traduzioni\",\"key\":\"\",\"description\":\"Gestione delle Traduzioni\"},{\"id\":\"32a9ecd3-5dc0-4dff-87d4-5e5e16d0c9a5\",\"route\":\"user-audit\",\"title\":\"Audit\",\"key\":\"\",\"description\":\"Accesso ai log di monitoraggio\"},{\"id\":\"21539ae0-de0a-43b3-98e1-4b40f062ae99\",\"route\":\"user-guide\",\"title\":\"Guida d'uso\",\"key\":\"\",\"description\":\"Guida per il corretto utilizzo del portale\"},{\"id\":\"92327e92-d49b-404b-9e30-8d6697487563\",\"route\":\"users\",\"title\":\"Utenti\",\"key\":\"\",\"description\":\"Gestione degli Utenti\"},{\"id\":\"4a6e7fb7-21b6-4e87-a598-d5e94c4bd380\",\"route\":\"help-desk-admin\",\"title\":\"Help Desk Admin\",\"description\":\"Gestione delle Richieste di Assistenza\",\"key\":\"\",\"keyDescription\":\"\"},{\"id\":\"ffbcc4c4-6645-4ff4-b0bc-ce007bc32f38\",\"route\":\"media-categories\",\"title\":\"Categorie dei Media\",\"description\":\"Gestisci l'elenco delle Categorie dei Media\",\"key\":\"\",\"keyDescription\":\"\"}]",
                        true,
                        "1",
                        true,
                        false,
                        false,
                        "{\"registration\":{\"firstName\":\"2\",\"lastName\":\"2\",\"email\":\"2\",\"contactEmail\":\"0\",\"nickName\":\"1\",\"sex\":\"1\",\"taxId\":\"1\",\"fixedPhone\":\"1\",\"mobilePhone\":\"1\",\"birthDate\":\"1\",\"birthProvince\":\"1\",\"birthZIP\":\"1\",\"birthState\":\"1\",\"residenceCity\":\"1\",\"residenceProvince\":\"1\",\"residenceZIP\":\"1\",\"residenceState\":\"1\",\"residenceAddress\":\"1\",\"residenceHouseNumber\":\"1\",\"occupation\":\"1\",\"description\":\"1\",\"birthCity\":\"1\"},\"profile\":{\"firstName\":\"2\",\"lastName\":\"2\",\"email\":\"2\",\"contactEmail\":\"0\",\"nickName\":\"0\",\"sex\":\"0\",\"taxId\":\"0\",\"fixedPhone\":\"0\",\"mobilePhone\":\"0\",\"birthDate\":\"0\",\"birthProvince\":\"0\",\"birthZIP\":\"0\",\"birthState\":\"0\",\"residenceCity\":\"0\",\"residenceProvince\":\"0\",\"residenceZIP\":\"0\",\"residenceState\":\"0\",\"residenceAddress\":\"0\",\"residenceHouseNumber\":\"0\",\"occupation\":\"0\",\"description\":\"1\"}}",
                        2880,
                        false,
                        true,
                        "[{\"name\":\"canBypassMaintenance\",\"description\":\"Accede anche quando il sito è in manutenzione\"},{\"name\":\"canCreateSelfCustomerUser\",\"description\":\"Può creare utenti CustomerEmployee nel proprio Tenant\"},{\"name\":\"canSeeAllTenants\",\"description\":\"Può vedere anche i Tenant che non sono associati al suo profilo\"},{\"name\":\"canSeeIncompleteConfigurations\",\"description\":\"Può vedere il menù delle configurazioni incomplete\"},{\"name\":\"canSeeRightBar\",\"description\":\"Può vedere il menù laterale destro\"},{\"name\":\"isAdmin\",\"description\":\"Contraddistingue l'Admin\"},{\"name\":\"isAudit\",\"description\":\"Contraddistingue gli utenti del gruppo Audit (monitoraggio)\"},{\"name\":\"isDeveloper\",\"description\":\"Contraddistingue gli utenti del gruppo Developer (sviluppatori)\"},{\"name\":\"isMarketing\",\"description\":\"Contraddistingue gli utenti del reparto Marketing\"},{\"name\":\"isOwner\",\"description\":\"Contraddistingue il titolare del sito/app\"},{\"name\":\"isSuperAdmin\",\"description\":\"Contraddistingue il super-admin\"},{\"name\":\"isTranslator\",\"description\":\"Contraddistingue i traduttori\"},{\"name\":\"isUser\",\"description\":\"Contraddistingue gli utenti generici\"}]",
                        "C#",
                        true,
                        "[{\"lastName\":\"Bellavigna\",\"firstName\":\"Gianluca\",\"email\":\"gianluca.bellavigna@maestrale.it\"},{\"lastName\":\"Biagetti\",\"firstName\":\"Sacha\",\"email\":\"sacha.biagetti@maestrale.it\"},{\"lastName\":\"Cipriani\",\"firstName\":\"Samuele\",\"email\":\"samuele.cipriani@maestrale.it\"},{\"lastName\":\"Gallorini\",\"firstName\":\"Veronica\",\"email\":\"veronica.gallorini@maestrale.it\"},{\"lastName\":\"Giannotta\",\"firstName\":\"Luigi\",\"email\":\"luigi.giannotta@maestrale.it\"},{\"lastName\":\"Marcelli\",\"firstName\":\"Andrea\",\"email\":\"andrea.marcelli@maestrale.it\"},{\"lastName\":\"Morganti\",\"firstName\":\"Emanulele\",\"email\":\"emanuele.morganti@maestrale.it\"},{\"lastName\":\"Pellerucci\",\"firstName\":\"Daniele\",\"email\":\"daniele.pellerucci@maestrale.it\"},{\"lastName\":\"Penzo\",\"firstName\":\"Stefano\",\"email\":\"stefano.penzo@maestrale.it\"},{\"lastName\":\"Perotti\",\"firstName\":\"Luca\",\"email\":\"luca.perotti@maestrale.it\"},{\"lastName\":\"Ratini\",\"firstName\":\"Riccardo\",\"email\":\"riccardo.ratini@maestrale.it\"},{\"lastName\":\"Sabatini\",\"firstName\":\"Fabio\",\"email\":\"fabio.sabatini@maestrale.it\"},{\"lastName\":\"Vitali\",\"firstName\":\"Roberto\",\"email\":\"roberto.vitali@maestrale.it\"}]",
                        "{\"googleIdSite\":\"\",\"redirectUriLogin\":\"\",\"redirectAfterGoogleLogin\":\"\",\"redirectAfterGoogleRegister\":\"\",\"redirectAfterGoogleError\":\"\",\"googleAPIKey\":\"\",\"googleSecret\":\"\",\"googleAPPId\":\"\",\"googleScopes\":\"\"}",
                        "{\"googleEnabled\":false,\"facebookEnabled\":false}",
                        true,
                        true,
                        "",
                        ""
                    }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Name", "Description", "Organization", "Enabled", "ParentTenant", "TenantVAT", "TaxId", "Email", "PhoneNumber", "WebSite", "TenantSDI", "TenantIBAN", "Owner", "Commercial", "ShareCapital", "RegisteredOfficeAddress", "RegisteredOfficeCity", "RegisteredOfficeProvince", "RegisteredOfficeState", "RegisteredOfficeRegion", "RegisteredOfficeZIP", "matchBillingAddress", "BillingAddressAddress", "BillingAddressCity", "BillingAddressProvince", "BillingAddressState", "BillingAddressRegion", "BillingAddressZIP", "TenantPEC", "isErasable", "isRecovery" },
                values: new object[,] {
                    {
                        "MASTER TENANT",
                        "Maestrale Information Technology",
                        "Maestrale Group",
                        true,
                        0,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        true,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        false,
                        false
                    },
                    {
                        "Tenant di Recupero",
                        "",
                        "Maestrale support",
                        true,
                        0,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        true,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        false,
                        true
                    }
                });

            migrationBuilder.InsertData(
                table: "Translations",
                columns: new[] { "languageCode", "translationWeb", "translationApp" },
                values: new object[,]
                {
                    {
                        "it",
                        "{\r\n  \"component\": {\r\n    \"gdpr\": {\r\n      \"accept\": \"Accetto\",\r\n      \"acceptPartial\": \"Accetto i soli cookie tecnici\",\r\n      \"consult\": \"Consulta\",\r\n      \"moreDetails\": \"per maggiori dettagli\",\r\n      \"privacyPolicy\": \"l'Informativa sulla privacy\",\r\n      \"refuse\": \"Rifiuto\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"choiceEntity\": \"SCEGLI L'ENTITÀ\",\r\n      \"create\": \"CREA\",\r\n      \"crudOnEntity\": \"CRUD SULL'ENTITÀ\",\r\n      \"filters\": \"FILTRI DI<br />RICERCA\",\r\n      \"find\": \"CERCA\",\r\n      \"newRecord\": \"NUOVO<br />RECORD\",\r\n      \"records\": \"record\",\r\n      \"retry\": \"Riprova\",\r\n      \"unableFetchRoutes\": \"Errore nel recupero delle Entità.\",\r\n      \"wait\": \"Attendere prego...\"\r\n    },\r\n    \"loginForm\": {\r\n      \"accessError\": \"si è verificato un errore; riprovare!\",\r\n      \"accountTemporarilyBlocked\": \"Il tuo account è <strong>temporaneamente bloccato</strong><br />\\n            poichè hai superato il massimo numero di tentativi errati consecutivi.<br />\\n            Potrai riprovare l'accesso a partire dal:\",\r\n      \"attemptsLeft\": \"tentativi\",\r\n      \"attention\": \"ATTENZIONE\",\r\n      \"banned\": \"questa utenza è bloccata, accesso negato.\",\r\n      \"beforeBlock\": \"prima che il tuo account venga temporaneamente bloccato.\",\r\n      \"cancel\": \"Annulla\",\r\n      \"clickToUpdate\": \"Clicca qui per inserire una nuova password\",\r\n      \"confirm\": \"Conferma\",\r\n      \"email\": \"E-mail\",\r\n      \"expiredPassword\": \"la password inserita è corretta ma scaduta\",\r\n      \"fbLogin\": \"Accedi con Facebook\",\r\n      \"googleLogin\": \"Accedi con Google\",\r\n      \"incorrectCredentials\": \"E-mail o password errati.\",\r\n      \"logIn\": \"Accedi\",\r\n      \"mandatoryFields\": \"Email e password sono obbligatori\",\r\n      \"oneAttemptRemaining\": \"Ti rimane <strong>1 tentativo</strong>\",\r\n      \"password\": \"Password\",\r\n      \"passwordToUpdate\": \"la password inserita è corretta ma è necessario aggiornarla\",\r\n      \"remain\": \"Ti rimangono\",\r\n      \"rulesAcceptance\": \"ACCETTAZIONE DELLA PRIVACY-POLICY\",\r\n      \"rulesDetails\": \"Effettuando l'accesso dichiari di accettare la nostra Politica sulla Privacy e di consentire l'utilizzo dei cookie.<br />Confermi?\"\r\n    },\r\n    \"messagesNotifications\": {\r\n      \"goToMessages\": \"Vai ai Messaggi\",\r\n      \"goToNotifications\": \"Vai alle Notifiche\",\r\n      \"messages\": \"Messaggi\",\r\n      \"noNotices\": \"Non ci sono nuove notifiche o messaggi\",\r\n      \"notifications\": \"Notifiche\",\r\n      \"notificationsAndMessages\": \"Notifiche e Messaggi\",\r\n      \"unreadedMessages\": \"messaggi non letti\",\r\n      \"unreadedNotifications\": \"notifiche non lette\",\r\n      \"waitPlease\": \"Attendere prego...\",\r\n      \"youHave\": \"Hai\"\r\n    },\r\n    \"registrationForm\": {\r\n      \"accept\": \"Accetto i\",\r\n      \"acceptanceRequired\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\r\n      \"allRequired\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"alreadyExixts\": \"Già registrato\",\r\n      \"alreadyExixtsInfo\": \"La tua e-mail risulta già registrata. Prova a recuperare la password\",\r\n      \"alreadySign\": \"Già registrato?\",\r\n      \"and\": \"e\",\r\n      \"birthCity\": \"Città di nascita\",\r\n      \"birthDate\": \"Data di nascita\",\r\n      \"birthProvince\": \"Provincia di nascita\",\r\n      \"birthState\": \"Stato di nascita\",\r\n      \"birthZIP\": \"C.A.P. di nascita\",\r\n      \"confirmPassword\": \"Conferma la password\",\r\n      \"contactEmail\": \"E-mail di contatto\",\r\n      \"description\": \"Descrizione\",\r\n      \"email\": \"E-mail\",\r\n      \"emailContactInfo\": \"Per future comunicazioni puoi impostare un'e-mail diversa\",\r\n      \"emailInfo\": \"Ti invieremo un'email per confermare l'indirizzo\",\r\n      \"error\": \"Si è verificato un errore. Riprovare!\",\r\n      \"failureToAccept\": \"Mancata accettazione\",\r\n      \"firstName\": \"Nome\",\r\n      \"fixedPhone\": \"Telefono fisso\",\r\n      \"lastName\": \"Cognome\",\r\n      \"loginToComplete\": \"Effettua il login per confermare il tuo profilo\",\r\n      \"man\": \"Uomo\",\r\n      \"mandatoryFields\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"minLenghtName\": \"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\r\n      \"minLengthNames\": \"Inserisci almeno due caratteri nei campi Nome e Cognome\",\r\n      \"mismatchPassword\": \"Le due Password non corrispondono\",\r\n      \"missingData\": \"Dati mancanti\",\r\n      \"missionComplete\": \"Operazione riuscita\",\r\n      \"mobilePhone\": \"Telefono mobile\",\r\n      \"needAccept\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy!\",\r\n      \"nickName\": \"Nick-name\",\r\n      \"nickNameInfo\": \"Inserisci il tuo soprannome\",\r\n      \"occupation\": \"Occupazione\",\r\n      \"password\": \"Password\",\r\n      \"privacyPolicy\": \"l'Informativa sulla Privacy\",\r\n      \"prohibitedOperation\": \"Operazione non consentita\",\r\n      \"recordingDisabled\": \"La registrazione a questo portale non è consentita\",\r\n      \"residenceAddress\": \"Indirizzo di residenza\",\r\n      \"residenceCity\": \"Città di residenza\",\r\n      \"residenceHouseNumber\": \"N° civico\",\r\n      \"residenceProvince\": \"Provincia di residenza\",\r\n      \"residenceState\": \"Stato di residenza\",\r\n      \"residenceZIP\": \"C.A.P. di residenza\",\r\n      \"sex\": \"Sesso\",\r\n      \"sigIn\": \"Accedi\",\r\n      \"signIn\": \"Accedi\",\r\n      \"signUp\": \"Registrati\",\r\n      \"taxId\": \"Codice fiscale\",\r\n      \"terms\": \"Termini e Condizioni\",\r\n      \"unactiveRegistration\": \"Registrazione non attiva.\",\r\n      \"uncorrectConfirmPassword\": \"Il formato del campo 'Conferma Password' non è corretto\",\r\n      \"uncorrectContactEmail\": \"Il formato del campo 'E-mail di Contatto' non è corretto\",\r\n      \"uncorrectData\": \"Dati incorretti\",\r\n      \"uncorrectEmail\": \"Il formato del campo 'E-mail' non è corretto\",\r\n      \"uncorrectFormat\": \"Formato non corretto\",\r\n      \"uncorrectPassword\": \"Il formato del campo 'Password' non è corretto\",\r\n      \"uncorrectTaxId\": \"Il formato del campo 'Codice Fiscale' non è corretto\",\r\n      \"woman\": \"Donna\"\r\n    },\r\n    \"termsConditions\": {\r\n      \"inReview\": \"Il documento è in fase di revisione. Riprovare in un secondo momento!\",\r\n      \"unknownDocument\": \"Il documento richiesto non esiste!\",\r\n      \"version\": \"Versione\",\r\n      \"waitPlease\": \"Recupero in corso, attendere prego...\"\r\n    },\r\n    \"tr\": {\r\n      \"choice\": \"Scegli la lingua\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"add\": \"AGGIUNGI\",\r\n      \"addMacro\": \"AGGIUNGI MACRO-AREA\",\r\n      \"addObject\": \"AGGIUNGI OGGETTO\",\r\n      \"addVoice\": \"AGGIUNGI VOCE\",\r\n      \"defaultStructure\": \"STRUTTURA DI DEFAULT\",\r\n      \"defaultValue\": \"VALORE PREDEFINITO\",\r\n      \"errorGettingData\": \"Errore nel recupero dei dati.\",\r\n      \"find\": \"cerca...\",\r\n      \"key\": \"CHIAVE\",\r\n      \"name\": \"NOME\",\r\n      \"nameLower\": \"Nome\",\r\n      \"notAvailableValue\": \"Non sono ancora presenti valori!\",\r\n      \"retry\": \"Riprova\",\r\n      \"stringKey\": \"Chiave della stringa\",\r\n      \"type\": \"Tipologia\",\r\n      \"value\": \"VALORE\",\r\n      \"valueLower\": \"Valore\",\r\n      \"wait\": \"Recupero dati in corso, attendere prego...\"\r\n    },\r\n    \"usersMaster\": {\r\n      \"users\": \"ELENCO UTENTI\",\r\n      \"add\": \"CREA NUOVO UTENTE\",\r\n      \"requests\": \"RICHIESTE DI AUTORIZZAZIONE\",\r\n      \"banned\": \"UTENTI BANNATI\"\r\n    },\r\n    \"setupSocial\": {\r\n      \"google\": \"GOOGLE\",\r\n      \"facebook\": \"FACEBOOK\",\r\n      \"thirdParts\": \"TERZE PARTI\"\r\n    },\r\n    \"setupMaster\": {\r\n      \"general\": \"GENERALE\",\r\n      \"security\": \"SICUREZZA\",\r\n      \"aspect\": \"ASPETTO\",\r\n      \"sliders\": \"SLIDERS\",\r\n      \"integrations\": \"INTEGRAZIONI\",\r\n      \"multiLanguage\": \"MULTI-LINGUA\",\r\n      \"routes\": \"ROTTE\",\r\n      \"entities\": \"ENTITÁ\",\r\n      \"userProfile\": \"PROFILO UTENTE\",\r\n      \"communications\": \"COMUNICAZIONI\",\r\n      \"operators\": \"OPERATORI\",\r\n      \"custom\": \"CUSTOM\"\r\n    }\r\n  },\r\n  \"controller\": {},\r\n  \"model\": {},\r\n  \"other\": {\r\n    \"slider\": {\r\n      \"slider1Description\": \"Descrizione 1 per pagine di Login e Welcome\",\r\n      \"slider1Title\": \"Titolo 1\",\r\n      \"slider2Description\": \"Descrizione 2 per pagine di Login e Welcome\",\r\n      \"slider2Title\": \"Titolo 2\",\r\n      \"slider3Description\": \"Descrizione 3 per pagine di Login e Welcome\",\r\n      \"slider3Title\": \"Titolo 3\"\r\n    },\r\n    \"utilsIncompleteConfig\": {\r\n      \"cookiesAcceptance\": \"Accettazione Cookie\",\r\n      \"privacyPolicy\": \"Informativa sulla Privacy\",\r\n      \"termsEndConditions\": \"Termini e Condizioni\"\r\n    }\r\n  },\r\n  \"template\": {\r\n    \"articles\": {\r\n      \"subTitle\": \"Gestisci gli articoli del sito\",\r\n      \"title\": \"Articoli\"\r\n    },\r\n    \"categories\": {\r\n      \"subTitle\": \"Gestisci le Categorie del sito\",\r\n      \"title\": \"Categorie\"\r\n    },\r\n    \"changePassword\": {\r\n      \"subTitle\": \"Cambia la tua password di accesso\",\r\n      \"title\": \"Cambia password\"\r\n    },\r\n    \"developerGuide\": {\r\n      \"subTitle\": \"La guida sviluppatori del framework\",\r\n      \"title\": \"Guida all'uso\"\r\n    },\r\n    \"devices\": {\r\n      \"subTitle\": \"Gestisci i dispositivi con cui sei collegato\",\r\n      \"title\": \"I tuoi dispositivi\"\r\n    },\r\n    \"emptyLoggedPage\": {\r\n      \"subTitle\": \"Sottotitolo\",\r\n      \"title\": \"Titolo\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"subTitle\": \"Gestisci i dati delle tue tabelle\",\r\n      \"title\": \"CRUD JSON:API\"\r\n    },\r\n    \"home\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"legals\": {\r\n      \"subTitle\": \"Gestione dei documenti legali\",\r\n      \"title\": \"Documenti Legali\"\r\n    },\r\n    \"legalsDetails\": {\r\n      \"subTitle\": \"Gestisci i dettagli del documento\",\r\n      \"title\": \"Modifica Documento Legale\"\r\n    },\r\n    \"loading\": {\r\n      \"loading\": \"CARICAMENTO IN CORSO...\"\r\n    },\r\n    \"loggedHome\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"login\": {\r\n      \"enterCredentials\": \"Inserisci le tue credenziali d'accesso.\",\r\n      \"forgotPassword\": \"Password dimenticata?\",\r\n      \"registration\": \"Registrati\",\r\n      \"unregistered\": \"Non sei registrato?\",\r\n      \"welcome\": \"Bentornato,\"\r\n    },\r\n    \"media\": {\r\n      \"subTitle\": \"Gestisci i media dell'applicativo\",\r\n      \"title\": \"Media\"\r\n    },\r\n    \"notifications\": {\r\n      \"subTitle\": \"Visualizza l'elenco delle notifiche ricevute\",\r\n      \"title\": \"Notifiche\"\r\n    },\r\n    \"recoveryPassword\": {\r\n      \"backwards\": \"indietro\",\r\n      \"email\": \"Email...\",\r\n      \"howTo\": \"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\r\n      \"return\": \"Torna\",\r\n      \"start\": \"Avvia il cambio password\",\r\n      \"title\": \"Recupero della password\"\r\n    },\r\n    \"registration\": {\r\n      \"howTo1\": \"crea il tuo account in\",\r\n      \"howTo2\": \"pochi secondi\",\r\n      \"welcome\": \"Benvenuto,\"\r\n    },\r\n    \"rolePermissions\": {\r\n      \"crud\": \"PERMESSI CRUD\",\r\n      \"roles\": \"RUOLI\",\r\n      \"rolesPermissionsMapping\": \"ASSOCIAZIONI R&divide;P\",\r\n      \"routes\": \"ROTTE\",\r\n      \"subTitle\": \"Definisci ruoli e permessi di accesso\",\r\n      \"tenants\": \"TENANT\",\r\n      \"tenantsUsersMapping\": \"ASSOCIAZIONI T&divide;U\",\r\n      \"title\": \"Autorizzazioni\",\r\n      \"userRoles\": \"RUOLI UTENTI\"\r\n    },\r\n    \"setup\": {\r\n      \"subTitle\": \"Imposta i parametri di funzionamento dell'applicativo\",\r\n      \"title\": \"Setup\"\r\n    },\r\n    \"template\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"templateDetails\": {\r\n      \"subTitle\": \"Modifica i dettagli del template\",\r\n      \"title\": \"Modifica Template\"\r\n    },\r\n    \"templates\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"tenantFallback\": {\r\n      \"subTitle\": \"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\r\n      \"title\": \"Verifica della licenza\"\r\n    },\r\n    \"terms\": {\r\n      \"backwards\": \"indietro\",\r\n      \"return\": \"Torna\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"subTitle\": \"Imposta la struttura del file di traduzione\",\r\n      \"title\": \"Struttura traduzioni\"\r\n    },\r\n    \"translations\": {\r\n      \"subTitle\": \"Traduci il sito nelle lingue disponibili.\",\r\n      \"title\": \"Traduzioni\"\r\n    },\r\n    \"updatePassword\": {\r\n      \"howto\": \"Scegli la tua nuova password\",\r\n      \"title\": \"Modifica della password\"\r\n    },\r\n    \"userAudit\": {\r\n      \"subTitle\": \"Monitora le chiamate http e i dati relativi\",\r\n      \"title\": \"Audit\"\r\n    },\r\n    \"userGuide\": {\r\n      \"subTitle\": \"La guida per l'utilizzo ottimale del portale\",\r\n      \"title\": \"Guida d'uso e manutenzione\"\r\n    },\r\n    \"userProfile\": {\r\n      \"subTitle\": \"Gestisci i tuoi dati personali\",\r\n      \"title\": \"Profilo personale\"\r\n    },\r\n    \"users\": {\r\n      \"subTitle\": \"Gestisci gli utenti dell'azienda\",\r\n      \"title\": \"Utenti\"\r\n    }\r\n  }\r\n}",
                        "{\r\n  \"component\": {\r\n    \"gdpr\": {\r\n      \"accept\": \"Accetto\",\r\n      \"acceptPartial\": \"Accetto i soli cookie tecnici\",\r\n      \"consult\": \"Consulta\",\r\n      \"moreDetails\": \"per maggiori dettagli\",\r\n      \"privacyPolicy\": \"l'Informativa sulla privacy\",\r\n      \"refuse\": \"Rifiuto\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"choiceEntity\": \"SCEGLI L'ENTITÀ\",\r\n      \"create\": \"CREA\",\r\n      \"crudOnEntity\": \"CRUD SULL'ENTITÀ\",\r\n      \"filters\": \"FILTRI DI<br />RICERCA\",\r\n      \"find\": \"CERCA\",\r\n      \"newRecord\": \"NUOVO<br />RECORD\",\r\n      \"records\": \"record\",\r\n      \"retry\": \"Riprova\",\r\n      \"unableFetchRoutes\": \"Errore nel recupero delle Entità.\",\r\n      \"wait\": \"Attendere prego...\"\r\n    },\r\n    \"loginForm\": {\r\n      \"accessError\": \"si è verificato un errore; riprovare!\",\r\n      \"accountTemporarilyBlocked\": \"Il tuo account è <strong>temporaneamente bloccato</strong><br />\\n            poichè hai superato il massimo numero di tentativi errati consecutivi.<br />\\n            Potrai riprovare l'accesso a partire dal:\",\r\n      \"attemptsLeft\": \"tentativi\",\r\n      \"attention\": \"ATTENZIONE\",\r\n      \"banned\": \"questa utenza è bloccata, accesso negato.\",\r\n      \"beforeBlock\": \"prima che il tuo account venga temporaneamente bloccato.\",\r\n      \"cancel\": \"Annulla\",\r\n      \"clickToUpdate\": \"Clicca qui per inserire una nuova password\",\r\n      \"confirm\": \"Conferma\",\r\n      \"email\": \"E-mail\",\r\n      \"expiredPassword\": \"la password inserita è corretta ma scaduta\",\r\n      \"fbLogin\": \"Accedi con Facebook\",\r\n      \"googleLogin\": \"Accedi con Google\",\r\n      \"incorrectCredentials\": \"E-mail o password errati.\",\r\n      \"logIn\": \"Accedi\",\r\n      \"mandatoryFields\": \"Email e password sono obbligatori\",\r\n      \"oneAttemptRemaining\": \"Ti rimane <strong>1 tentativo</strong>\",\r\n      \"password\": \"Password\",\r\n      \"passwordToUpdate\": \"la password inserita è corretta ma è necessario aggiornarla\",\r\n      \"remain\": \"Ti rimangono\",\r\n      \"rulesAcceptance\": \"ACCETTAZIONE DELLA PRIVACY-POLICY\",\r\n      \"rulesDetails\": \"Effettuando l'accesso dichiari di accettare la nostra Politica sulla Privacy e di consentire l'utilizzo dei cookie.<br />Confermi?\"\r\n    },\r\n    \"messagesNotifications\": {\r\n      \"goToMessages\": \"Vai ai Messaggi\",\r\n      \"goToNotifications\": \"Vai alle Notifiche\",\r\n      \"messages\": \"Messaggi\",\r\n      \"noNotices\": \"Non ci sono nuove notifiche o messaggi\",\r\n      \"notifications\": \"Notifiche\",\r\n      \"notificationsAndMessages\": \"Notifiche e Messaggi\",\r\n      \"unreadedMessages\": \"messaggi non letti\",\r\n      \"unreadedNotifications\": \"notifiche non lette\",\r\n      \"waitPlease\": \"Attendere prego...\",\r\n      \"youHave\": \"Hai\"\r\n    },\r\n    \"registrationForm\": {\r\n      \"accept\": \"Accetto i\",\r\n      \"acceptanceRequired\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\r\n      \"allRequired\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"alreadyExixts\": \"Già registrato\",\r\n      \"alreadyExixtsInfo\": \"La tua e-mail risulta già registrata. Prova a recuperare la password\",\r\n      \"alreadySign\": \"Già registrato?\",\r\n      \"and\": \"e\",\r\n      \"birthCity\": \"Città di nascita\",\r\n      \"birthDate\": \"Data di nascita\",\r\n      \"birthProvince\": \"Provincia di nascita\",\r\n      \"birthState\": \"Stato di nascita\",\r\n      \"birthZIP\": \"C.A.P. di nascita\",\r\n      \"confirmPassword\": \"Conferma la password\",\r\n      \"contactEmail\": \"E-mail di contatto\",\r\n      \"description\": \"Descrizione\",\r\n      \"email\": \"E-mail\",\r\n      \"emailContactInfo\": \"Per future comunicazioni puoi impostare un'e-mail diversa\",\r\n      \"emailInfo\": \"Ti invieremo un'email per confermare l'indirizzo\",\r\n      \"error\": \"Si è verificato un errore. Riprovare!\",\r\n      \"failureToAccept\": \"Mancata accettazione\",\r\n      \"firstName\": \"Nome\",\r\n      \"fixedPhone\": \"Telefono fisso\",\r\n      \"lastName\": \"Cognome\",\r\n      \"loginToComplete\": \"Effettua il login per confermare il tuo profilo\",\r\n      \"man\": \"Uomo\",\r\n      \"mandatoryFields\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"minLenghtName\": \"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\r\n      \"minLengthNames\": \"Inserisci almeno due caratteri nei campi Nome e Cognome\",\r\n      \"mismatchPassword\": \"Le due Password non corrispondono\",\r\n      \"missingData\": \"Dati mancanti\",\r\n      \"missionComplete\": \"Operazione riuscita\",\r\n      \"mobilePhone\": \"Telefono mobile\",\r\n      \"needAccept\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy!\",\r\n      \"nickName\": \"Nick-name\",\r\n      \"nickNameInfo\": \"Inserisci il tuo soprannome\",\r\n      \"occupation\": \"Occupazione\",\r\n      \"password\": \"Password\",\r\n      \"privacyPolicy\": \"l'Informativa sulla Privacy\",\r\n      \"prohibitedOperation\": \"Operazione non consentita\",\r\n      \"recordingDisabled\": \"La registrazione a questo portale non è consentita\",\r\n      \"residenceAddress\": \"Indirizzo di residenza\",\r\n      \"residenceCity\": \"Città di residenza\",\r\n      \"residenceHouseNumber\": \"N° civico\",\r\n      \"residenceProvince\": \"Provincia di residenza\",\r\n      \"residenceState\": \"Stato di residenza\",\r\n      \"residenceZIP\": \"C.A.P. di residenza\",\r\n      \"sex\": \"Sesso\",\r\n      \"sigIn\": \"Accedi\",\r\n      \"signIn\": \"Accedi\",\r\n      \"signUp\": \"Registrati\",\r\n      \"taxId\": \"Codice fiscale\",\r\n      \"terms\": \"Termini e Condizioni\",\r\n      \"unactiveRegistration\": \"Registrazione non attiva.\",\r\n      \"uncorrectConfirmPassword\": \"Il formato del campo 'Conferma Password' non è corretto\",\r\n      \"uncorrectContactEmail\": \"Il formato del campo 'E-mail di Contatto' non è corretto\",\r\n      \"uncorrectData\": \"Dati incorretti\",\r\n      \"uncorrectEmail\": \"Il formato del campo 'E-mail' non è corretto\",\r\n      \"uncorrectFormat\": \"Formato non corretto\",\r\n      \"uncorrectPassword\": \"Il formato del campo 'Password' non è corretto\",\r\n      \"uncorrectTaxId\": \"Il formato del campo 'Codice Fiscale' non è corretto\",\r\n      \"woman\": \"Donna\"\r\n    },\r\n    \"termsConditions\": {\r\n      \"inReview\": \"Il documento è in fase di revisione. Riprovare in un secondo momento!\",\r\n      \"unknownDocument\": \"Il documento richiesto non esiste!\",\r\n      \"version\": \"Versione\",\r\n      \"waitPlease\": \"Recupero in corso, attendere prego...\"\r\n    },\r\n    \"tr\": {\r\n      \"choice\": \"Scegli la lingua\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"add\": \"AGGIUNGI\",\r\n      \"addMacro\": \"AGGIUNGI MACRO-AREA\",\r\n      \"addObject\": \"AGGIUNGI OGGETTO\",\r\n      \"addVoice\": \"AGGIUNGI VOCE\",\r\n      \"defaultStructure\": \"STRUTTURA DI DEFAULT\",\r\n      \"defaultValue\": \"VALORE PREDEFINITO\",\r\n      \"errorGettingData\": \"Errore nel recupero dei dati.\",\r\n      \"find\": \"cerca...\",\r\n      \"key\": \"CHIAVE\",\r\n      \"name\": \"NOME\",\r\n      \"nameLower\": \"Nome\",\r\n      \"notAvailableValue\": \"Non sono ancora presenti valori!\",\r\n      \"retry\": \"Riprova\",\r\n      \"stringKey\": \"Chiave della stringa\",\r\n      \"type\": \"Tipologia\",\r\n      \"value\": \"VALORE\",\r\n      \"valueLower\": \"Valore\",\r\n      \"wait\": \"Recupero dati in corso, attendere prego...\"\r\n    },\r\n    \"usersMaster\": {\r\n      \"users\": \"ELENCO UTENTI\",\r\n      \"add\": \"CREA NUOVO UTENTE\",\r\n      \"requests\": \"RICHIESTE DI AUTORIZZAZIONE\",\r\n      \"banned\": \"UTENTI BANNATI\"\r\n    },\r\n    \"setupSocial\": {\r\n      \"google\": \"GOOGLE\",\r\n      \"facebook\": \"FACEBOOK\",\r\n      \"thirdParts\": \"TERZE PARTI\"\r\n    },\r\n    \"setupMaster\": {\r\n      \"general\": \"GENERALE\",\r\n      \"security\": \"SICUREZZA\",\r\n      \"aspect\": \"ASPETTO\",\r\n      \"sliders\": \"SLIDERS\",\r\n      \"integrations\": \"INTEGRAZIONI\",\r\n      \"multiLanguage\": \"MULTI-LINGUA\",\r\n      \"routes\": \"ROTTE\",\r\n      \"entities\": \"ENTITÁ\",\r\n      \"userProfile\": \"PROFILO UTENTE\",\r\n      \"communications\": \"COMUNICAZIONI\",\r\n      \"operators\": \"OPERATORI\",\r\n      \"custom\": \"CUSTOM\"\r\n    }\r\n  },\r\n  \"controller\": {},\r\n  \"model\": {},\r\n  \"other\": {\r\n    \"slider\": {\r\n      \"slider1Description\": \"Descrizione 1 per pagine di Login e Welcome\",\r\n      \"slider1Title\": \"Titolo 1\",\r\n      \"slider2Description\": \"Descrizione 2 per pagine di Login e Welcome\",\r\n      \"slider2Title\": \"Titolo 2\",\r\n      \"slider3Description\": \"Descrizione 3 per pagine di Login e Welcome\",\r\n      \"slider3Title\": \"Titolo 3\"\r\n    },\r\n    \"utilsIncompleteConfig\": {\r\n      \"cookiesAcceptance\": \"Accettazione Cookie\",\r\n      \"privacyPolicy\": \"Informativa sulla Privacy\",\r\n      \"termsEndConditions\": \"Termini e Condizioni\"\r\n    }\r\n  },\r\n  \"template\": {\r\n    \"articles\": {\r\n      \"subTitle\": \"Gestisci gli articoli del sito\",\r\n      \"title\": \"Articoli\"\r\n    },\r\n    \"categories\": {\r\n      \"subTitle\": \"Gestisci le Categorie del sito\",\r\n      \"title\": \"Categorie\"\r\n    },\r\n    \"changePassword\": {\r\n      \"subTitle\": \"Cambia la tua password di accesso\",\r\n      \"title\": \"Cambia password\"\r\n    },\r\n    \"developerGuide\": {\r\n      \"subTitle\": \"La guida sviluppatori del framework\",\r\n      \"title\": \"Guida all'uso\"\r\n    },\r\n    \"devices\": {\r\n      \"subTitle\": \"Gestisci i dispositivi con cui sei collegato\",\r\n      \"title\": \"I tuoi dispositivi\"\r\n    },\r\n    \"emptyLoggedPage\": {\r\n      \"subTitle\": \"Sottotitolo\",\r\n      \"title\": \"Titolo\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"subTitle\": \"Gestisci i dati delle tue tabelle\",\r\n      \"title\": \"CRUD JSON:API\"\r\n    },\r\n    \"home\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"legals\": {\r\n      \"subTitle\": \"Gestione dei documenti legali\",\r\n      \"title\": \"Documenti Legali\"\r\n    },\r\n    \"legalsDetails\": {\r\n      \"subTitle\": \"Gestisci i dettagli del documento\",\r\n      \"title\": \"Modifica Documento Legale\"\r\n    },\r\n    \"loading\": {\r\n      \"loading\": \"CARICAMENTO IN CORSO...\"\r\n    },\r\n    \"loggedHome\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"login\": {\r\n      \"enterCredentials\": \"Inserisci le tue credenziali d'accesso.\",\r\n      \"forgotPassword\": \"Password dimenticata?\",\r\n      \"registration\": \"Registrati\",\r\n      \"unregistered\": \"Non sei registrato?\",\r\n      \"welcome\": \"Bentornato,\"\r\n    },\r\n    \"media\": {\r\n      \"subTitle\": \"Gestisci i media dell'applicativo\",\r\n      \"title\": \"Media\"\r\n    },\r\n    \"notifications\": {\r\n      \"subTitle\": \"Visualizza l'elenco delle notifiche ricevute\",\r\n      \"title\": \"Notifiche\"\r\n    },\r\n    \"recoveryPassword\": {\r\n      \"backwards\": \"indietro\",\r\n      \"email\": \"Email...\",\r\n      \"howTo\": \"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\r\n      \"return\": \"Torna\",\r\n      \"start\": \"Avvia il cambio password\",\r\n      \"title\": \"Recupero della password\"\r\n    },\r\n    \"registration\": {\r\n      \"howTo1\": \"crea il tuo account in\",\r\n      \"howTo2\": \"pochi secondi\",\r\n      \"welcome\": \"Benvenuto,\"\r\n    },\r\n    \"rolePermissions\": {\r\n      \"crud\": \"PERMESSI CRUD\",\r\n      \"roles\": \"RUOLI\",\r\n      \"rolesPermissionsMapping\": \"ASSOCIAZIONI R&divide;P\",\r\n      \"routes\": \"ROTTE\",\r\n      \"subTitle\": \"Definisci ruoli e permessi di accesso\",\r\n      \"tenants\": \"TENANT\",\r\n      \"tenantsUsersMapping\": \"ASSOCIAZIONI T&divide;U\",\r\n      \"title\": \"Autorizzazioni\",\r\n      \"userRoles\": \"RUOLI UTENTI\"\r\n    },\r\n    \"setup\": {\r\n      \"subTitle\": \"Imposta i parametri di funzionamento dell'applicativo\",\r\n      \"title\": \"Setup\"\r\n    },\r\n    \"template\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"templateDetails\": {\r\n      \"subTitle\": \"Modifica i dettagli del template\",\r\n      \"title\": \"Modifica Template\"\r\n    },\r\n    \"templates\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"tenantFallback\": {\r\n      \"subTitle\": \"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\r\n      \"title\": \"Verifica della licenza\"\r\n    },\r\n    \"terms\": {\r\n      \"backwards\": \"indietro\",\r\n      \"return\": \"Torna\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"subTitle\": \"Imposta la struttura del file di traduzione\",\r\n      \"title\": \"Struttura traduzioni\"\r\n    },\r\n    \"translations\": {\r\n      \"subTitle\": \"Traduci il sito nelle lingue disponibili.\",\r\n      \"title\": \"Traduzioni\"\r\n    },\r\n    \"updatePassword\": {\r\n      \"howto\": \"Scegli la tua nuova password\",\r\n      \"title\": \"Modifica della password\"\r\n    },\r\n    \"userAudit\": {\r\n      \"subTitle\": \"Monitora le chiamate http e i dati relativi\",\r\n      \"title\": \"Audit\"\r\n    },\r\n    \"userGuide\": {\r\n      \"subTitle\": \"La guida per l'utilizzo ottimale del portale\",\r\n      \"title\": \"Guida d'uso e manutenzione\"\r\n    },\r\n    \"userProfile\": {\r\n      \"subTitle\": \"Gestisci i tuoi dati personali\",\r\n      \"title\": \"Profilo personale\"\r\n    },\r\n    \"users\": {\r\n      \"subTitle\": \"Gestisci gli utenti dell'azienda\",\r\n      \"title\": \"Utenti\"\r\n    }\r\n  }\r\n}" },
                    {
                        "en",
                        "{\r\n  \"component\": {\r\n    \"gdpr\": {\r\n      \"accept\": \"Accetto\",\r\n      \"acceptPartial\": \"Accetto i soli cookie tecnici\",\r\n      \"consult\": \"Consulta\",\r\n      \"moreDetails\": \"per maggiori dettagli\",\r\n      \"privacyPolicy\": \"l'Informativa sulla privacy\",\r\n      \"refuse\": \"Rifiuto\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"loginForm\": {\r\n      \"accessError\": \"an error occurred; try again!\",\r\n      \"accountTemporarilyBlocked\": \"Your account is <strong>temporarily blocked</strong><br />because you have exceeded the maximum number of consecutive incorrect attempts.<br />You will be able to try logging in again starting from:\",\r\n      \"attemptsLeft\": \"attempts\",\r\n      \"attention\": \"ATTENTION\",\r\n      \"banned\": \"This user is blocked, access denied.\",\r\n      \"beforeBlock\": \"before your account is temporarily blocked.\",\r\n      \"cancel\": \"Cancel\",\r\n      \"clickToUpdate\": \"Click here to enter a new password\",\r\n      \"confirm\": \"Confirm\",\r\n      \"email\": \"E-mail\",\r\n      \"expiredPassword\": \"the password entered is correct but has expired\",\r\n      \"fbLogin\": \"Log in with Facebook\",\r\n      \"googleLogin\": \"Sign in with Google\",\r\n      \"incorrectCredentials\": \"Incorrect email or password.\",\r\n      \"logIn\": \"Sign in\",\r\n      \"mandatoryFields\": \"Email and password are required\",\r\n      \"oneAttemptRemaining\": \"You have <strong>1 attempt left</strong>\",\r\n      \"password\": \"Password\",\r\n      \"passwordToUpdate\": \"the password entered is correct but you need to update it\",\r\n      \"remain\": \"You have\",\r\n      \"rulesAcceptance\": \"ACCEPTANCE OF THE PRIVACY POLICY\",\r\n      \"rulesDetails\": \"By logging in you declare that you accept our Privacy Policy and allow the use of cookies.<br />Do you confirm?\"\r\n    },\r\n    \"registrationForm\": {\r\n      \"accept\": \"I accept the\",\r\n      \"acceptanceRequired\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\r\n      \"allRequired\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"alreadyExixts\": \"Already registered\",\r\n      \"alreadyExixtsInfo\": \"Your email is already registered. Try to recover your password\",\r\n      \"alreadySign\": \"Already registered?\",\r\n      \"and\": \"and\",\r\n      \"birthCity\": \"City of Birth\",\r\n      \"birthDate\": \"Date of birth\",\r\n      \"birthProvince\": \"County of birth\",\r\n      \"birthState\": \"State of birth\",\r\n      \"birthZIP\": \"Postal code of birth\",\r\n      \"confirmPassword\": \"Confirm Password\",\r\n      \"contactEmail\": \"Contact email\",\r\n      \"description\": \"Description\",\r\n      \"email\": \"E-mail\",\r\n      \"emailContactInfo\": \"For future communications you can set a different email\",\r\n      \"emailInfo\": \"We will send you an email to confirm the address\",\r\n      \"error\": \"An error occurred. Try again!\",\r\n      \"failureToAccept\": \"Mancata accettazione\",\r\n      \"firstName\": \"Name\",\r\n      \"fixedPhone\": \"Landline phone\",\r\n      \"lastName\": \"Surname\",\r\n      \"loginToComplete\": \"Log in to confirm your profile\",\r\n      \"man\": \"Man\",\r\n      \"mandatoryFields\": \"All fields marked with an asterisk are mandatory\",\r\n      \"minLenghtName\": \"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\r\n      \"minLengthNames\": \"Enter at least two characters in the Name and Surname fields\",\r\n      \"mismatchPassword\": \"The two passwords do not match\",\r\n      \"missingData\": \"Dati mancanti\",\r\n      \"missionComplete\": \"Mission accomplished\",\r\n      \"mobilePhone\": \"Mobile phone\",\r\n      \"needAccept\": \"You must accept the Terms, Conditions and Privacy Policy!\",\r\n      \"nickName\": \"Nick-name\",\r\n      \"nickNameInfo\": \"Enter your Nick name\",\r\n      \"occupation\": \"Occupation\",\r\n      \"password\": \"Password\",\r\n      \"privacyPolicy\": \"the Privacy Policy\",\r\n      \"prohibitedOperation\": \"Operazione non consentita\",\r\n      \"recordingDisabled\": \"La registrazione a questo portale non è consentita\",\r\n      \"residenceAddress\": \"Residence address\",\r\n      \"residenceCity\": \"City of residence\",\r\n      \"residenceHouseNumber\": \"House number\",\r\n      \"residenceProvince\": \"Province of residence\",\r\n      \"residenceState\": \"State of residence\",\r\n      \"residenceZIP\": \"Postal code of residence\",\r\n      \"sex\": \"Gender\",\r\n      \"sigIn\": \"Log in\",\r\n      \"signIn\": \"Log in\",\r\n      \"signUp\": \"Sign up\",\r\n      \"taxId\": \"Tax ID code\",\r\n      \"terms\": \"Terms and conditions\",\r\n      \"unactiveRegistration\": \"Registration not active.\",\r\n      \"uncorrectConfirmPassword\": \"The format of the 'Confirm Password' field is incorrect\",\r\n      \"uncorrectContactEmail\": \"The format of the 'Contact Email' field is incorrect\",\r\n      \"uncorrectData\": \"Dati incorretti\",\r\n      \"uncorrectEmail\": \"The format of the 'Email' field is incorrect\",\r\n      \"uncorrectFormat\": \"Incorrect format\",\r\n      \"uncorrectPassword\": \"The format of the 'Password' field is incorrect\",\r\n      \"uncorrectTaxId\": \"The format of the 'Tax ID Code' field is incorrect\",\r\n      \"woman\": \"Woman\"\r\n    },\r\n    \"termsConditions\": {\r\n      \"inReview\": \"The document is under review. Please try again later!\",\r\n      \"unknownDocument\": \"The requested document does not exist!\",\r\n      \"version\": \"Version\",\r\n      \"waitPlease\": \"Recovering, please wait...\"\r\n    },\r\n    \"tr\": {\r\n      \"choice\": \"Choose the language\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"add\": \"AGGIUNGI\",\r\n      \"addMacro\": \"AGGIUNGI MACRO-AREA\",\r\n      \"addObject\": \"AGGIUNGI OGGETTO\",\r\n      \"addVoice\": \"AGGIUNGI VOCE\",\r\n      \"defaultStructure\": \"STRUTTURA DI DEFAULT\",\r\n      \"defaultValue\": \"VALORE PREDEFINITO\",\r\n      \"errorGettingData\": \"Errore nel recupero dei dati.\",\r\n      \"find\": \"cerca...\",\r\n      \"key\": \"CHIAVE\",\r\n      \"name\": \"NOME\",\r\n      \"nameLower\": \"Nome\",\r\n      \"notAvailableValue\": \"Non sono ancora presenti valori!\",\r\n      \"retry\": \"Riprova\",\r\n      \"stringKey\": \"Chiave della stringa\",\r\n      \"type\": \"Tipologia\",\r\n      \"value\": \"VALORE\",\r\n      \"valueLower\": \"Valore\",\r\n      \"wait\": \"Recupero dati in corso, attendere prego...\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"retry\": \"Riprova\",\r\n      \"choiceEntity\": \"SCEGLI L'ENTITÀ\",\r\n      \"unableFetchRoutes\": \"Errore nel recupero delle Entità.\",\r\n      \"crudOnEntity\": \"CRUD SULL'ENTITÀ\",\r\n      \"records\": \"record\",\r\n      \"wait\": \"Attendere prego...\",\r\n      \"create\": \"CREA\",\r\n      \"newRecord\": \"NUOVO<br />RECORD\",\r\n      \"filters\": \"FILTRI DI<br />RICERCA\",\r\n      \"find\": \"CERCA\"\r\n    },\r\n    \"messagesNotifications\": {\r\n      \"youHave\": \"Hai\",\r\n      \"unreadedMessages\": \"messaggi non letti\",\r\n      \"unreadedNotifications\": \"notifiche non lette\",\r\n      \"notificationsAndMessages\": \"Notifiche e Messaggi\",\r\n      \"noNotices\": \"Non ci sono notifiche o messaggi\",\r\n      \"notifications\": \"Notifiche\",\r\n      \"messages\": \"Messaggi\",\r\n      \"goToNotifications\": \"Vai alle Notifiche\",\r\n      \"goToMessages\": \"Vai ai Messaggi\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"usersMaster\": {\r\n      \"users\": \"ELENCO UTENTI\",\r\n      \"add\": \"CREA NUOVO UTENTE\",\r\n      \"requests\": \"RICHIESTE DI AUTORIZZAZIONE\",\r\n      \"banned\": \"UTENTI BANNATI\"\r\n    },\r\n    \"setupSocial\": {\r\n      \"google\": \"GOOGLE\",\r\n      \"facebook\": \"FACEBOOK\",\r\n      \"thirdParts\": \"TERZE PARTI\"\r\n    },\r\n    \"setupMaster\": {\r\n      \"general\": \"GENERALE\",\r\n      \"security\": \"SICUREZZA\",\r\n      \"aspect\": \"ASPETTO\",\r\n      \"sliders\": \"SLIDERS\",\r\n      \"integrations\": \"INTEGRAZIONI\",\r\n      \"multiLanguage\": \"MULTI-LINGUA\",\r\n      \"routes\": \"ROTTE\",\r\n      \"entities\": \"ENTITÁ\",\r\n      \"userProfile\": \"PROFILO UTENTE\",\r\n      \"communications\": \"COMUNICAZIONI\",\r\n      \"operators\": \"OPERATORI\",\r\n      \"custom\": \"CUSTOM\"\r\n    }\r\n  },\r\n  \"controller\": {},\r\n  \"model\": {},\r\n  \"other\": {\r\n    \"slider\": {\r\n      \"slider1Description\": \"Description 1 for login and welcome page\",\r\n      \"slider1Title\": \"Title 1\",\r\n      \"slider2Description\": \"Description 2 for login and welcome page\",\r\n      \"slider2Title\": \"Title 2\",\r\n      \"slider3Description\": \"Description 3 for login and welcome page\",\r\n      \"slider3Title\": \"Title 3\"\r\n    },\r\n    \"utilsIncompleteConfig\": {\r\n      \"cookiesAcceptance\": \"Cookie Acceptance\",\r\n      \"privacyPolicy\": \"Privacy Policy\",\r\n      \"termsEndConditions\": \"Terms & Conditions\"\r\n    }\r\n  },\r\n  \"template\": {\r\n    \"articles\": {\r\n      \"subTitle\": \"Manage the site's articles\",\r\n      \"title\": \"Articles\"\r\n    },\r\n    \"categories\": {\r\n      \"subTitle\": \"Gestisci le Categorie del sito\",\r\n      \"title\": \"Categorie\"\r\n    },\r\n    \"changePassword\": {\r\n      \"subTitle\": \"Cambia la tua password di accesso\",\r\n      \"title\": \"Cambia password\"\r\n    },\r\n    \"developerGuide\": {\r\n      \"subTitle\": \"La guida sviluppatori del framework\",\r\n      \"title\": \"Guida all'uso\"\r\n    },\r\n    \"devices\": {\r\n      \"subTitle\": \"Gestisci i dispositivi con cui sei collegato\",\r\n      \"title\": \"I tuoi dispositivi\"\r\n    },\r\n    \"emptyLoggedPage\": {\r\n      \"subTitle\": \"Sottotitolo\",\r\n      \"title\": \"Titolo\"\r\n    },\r\n    \"home\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"legals\": {\r\n      \"subTitle\": \"Gestione dei documenti legali\",\r\n      \"title\": \"Documenti Legali\"\r\n    },\r\n    \"legalsDetails\": {\r\n      \"subTitle\": \"Gestisci i dettagli del documento\",\r\n      \"title\": \"Modifica Documento Legale\"\r\n    },\r\n    \"loading\": {\r\n      \"loading\": \"CARICAMENTO IN CORSO...\"\r\n    },\r\n    \"loggedHome\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"login\": {\r\n      \"enterCredentials\": \"Enter your login credentials.\",\r\n      \"forgotPassword\": \"Forgot password?\",\r\n      \"registration\": \"Sign in\",\r\n      \"unregistered\": \"Not registered?\",\r\n      \"welcome\": \"Welcome back,\"\r\n    },\r\n    \"media\": {\r\n      \"subTitle\": \"Gestisci i media dell'applicativo\",\r\n      \"title\": \"Media\"\r\n    },\r\n    \"recoveryPassword\": {\r\n      \"backwards\": \"indietro\",\r\n      \"email\": \"Email...\",\r\n      \"howTo\": \"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\r\n      \"return\": \"Torna\",\r\n      \"start\": \"Avvia il cambio password\",\r\n      \"title\": \"Recupero della password\"\r\n    },\r\n    \"registration\": {\r\n      \"howTo1\": \"create your account in\",\r\n      \"howTo2\": \"few seconds\",\r\n      \"welcome\": \"Welcome\"\r\n    },\r\n    \"rolePermissions\": {\r\n      \"crud\": \"PERMESSI CRUD\",\r\n      \"roles\": \"RUOLI\",\r\n      \"rolesPermissionsMapping\": \"ASSOCIAZIONI R&divide;P\",\r\n      \"routes\": \"ROTTE\",\r\n      \"subTitle\": \"Definisci ruoli e permessi di accesso\",\r\n      \"tenants\": \"TENANT\",\r\n      \"tenantsUsersMapping\": \"ASSOCIAZIONI T&divide;U\",\r\n      \"title\": \"Autorizzazioni\",\r\n      \"userRoles\": \"RUOLI UTENTI\"\r\n    },\r\n    \"setup\": {\r\n      \"subTitle\": \"Imposta i parametri di funzionamento dell'applicativo\",\r\n      \"title\": \"Setup\"\r\n    },\r\n    \"template\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"templateDetails\": {\r\n      \"subTitle\": \"Modifica i dettagli del template\",\r\n      \"title\": \"Modifica Template\"\r\n    },\r\n    \"templates\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"tenantFallback\": {\r\n      \"subTitle\": \"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\r\n      \"title\": \"Verifica della licenza\"\r\n    },\r\n    \"terms\": {\r\n      \"backwards\": \"back\",\r\n      \"return\": \"Come\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"subTitle\": \"Imposta la struttura del file di traduzione\",\r\n      \"title\": \"Struttura traduzioni\"\r\n    },\r\n    \"translations\": {\r\n      \"subTitle\": \"Traduci il sito nelle lingue disponibili.\",\r\n      \"title\": \"Traduzioni\"\r\n    },\r\n    \"updatePassword\": {\r\n      \"howto\": \"Scegli la tua nuova password\",\r\n      \"title\": \"Modifica della password\"\r\n    },\r\n    \"userAudit\": {\r\n      \"subTitle\": \"Monitora le chiamate http e i dati relativi\",\r\n      \"title\": \"Audit\"\r\n    },\r\n    \"userGuide\": {\r\n      \"subTitle\": \"La guida per l'utilizzo ottimale del portale\",\r\n      \"title\": \"Guida d'uso e manutenzione\"\r\n    },\r\n    \"userProfile\": {\r\n      \"subTitle\": \"Gestisci i tuoi dati personali\",\r\n      \"title\": \"Profilo personale\"\r\n    },\r\n    \"users\": {\r\n      \"subTitle\": \"Gestisci gli utenti dell'azienda\",\r\n      \"title\": \"Utenti\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"title\": \"CRUD\",\r\n      \"subTitle\": \"Gestisci i dati delle tue tabelle\"\r\n    },\r\n    \"notifications\": {\r\n      \"title\": \"Notifiche\",\r\n      \"subTitle\": \"Visualizza l'elenco delle notifiche ricevute\"\r\n    }\r\n  }\r\n}",
                        "{\r\n  \"component\": {\r\n    \"gdpr\": {\r\n      \"accept\": \"Accetto\",\r\n      \"acceptPartial\": \"Accetto i soli cookie tecnici\",\r\n      \"consult\": \"Consulta\",\r\n      \"moreDetails\": \"per maggiori dettagli\",\r\n      \"privacyPolicy\": \"l'Informativa sulla privacy\",\r\n      \"refuse\": \"Rifiuto\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"loginForm\": {\r\n      \"accessError\": \"an error occurred; try again!\",\r\n      \"accountTemporarilyBlocked\": \"Your account is <strong>temporarily blocked</strong><br />because you have exceeded the maximum number of consecutive incorrect attempts.<br />You will be able to try logging in again starting from:\",\r\n      \"attemptsLeft\": \"attempts\",\r\n      \"attention\": \"ATTENTION\",\r\n      \"banned\": \"This user is blocked, access denied.\",\r\n      \"beforeBlock\": \"before your account is temporarily blocked.\",\r\n      \"cancel\": \"Cancel\",\r\n      \"clickToUpdate\": \"Click here to enter a new password\",\r\n      \"confirm\": \"Confirm\",\r\n      \"email\": \"E-mail\",\r\n      \"expiredPassword\": \"the password entered is correct but has expired\",\r\n      \"fbLogin\": \"Log in with Facebook\",\r\n      \"googleLogin\": \"Sign in with Google\",\r\n      \"incorrectCredentials\": \"Incorrect email or password.\",\r\n      \"logIn\": \"Sign in\",\r\n      \"mandatoryFields\": \"Email and password are required\",\r\n      \"oneAttemptRemaining\": \"You have <strong>1 attempt left</strong>\",\r\n      \"password\": \"Password\",\r\n      \"passwordToUpdate\": \"the password entered is correct but you need to update it\",\r\n      \"remain\": \"You have\",\r\n      \"rulesAcceptance\": \"ACCEPTANCE OF THE PRIVACY POLICY\",\r\n      \"rulesDetails\": \"By logging in you declare that you accept our Privacy Policy and allow the use of cookies.<br />Do you confirm?\"\r\n    },\r\n    \"registrationForm\": {\r\n      \"accept\": \"I accept the\",\r\n      \"acceptanceRequired\": \"Devi accettare i Termini, le Condizioni e l'Informativa sulla Privacy\",\r\n      \"allRequired\": \"Tutti i campi contrassegnati dall'asterisco sono obbligatori\",\r\n      \"alreadyExixts\": \"Already registered\",\r\n      \"alreadyExixtsInfo\": \"Your email is already registered. Try to recover your password\",\r\n      \"alreadySign\": \"Already registered?\",\r\n      \"and\": \"and\",\r\n      \"birthCity\": \"City of Birth\",\r\n      \"birthDate\": \"Date of birth\",\r\n      \"birthProvince\": \"County of birth\",\r\n      \"birthState\": \"State of birth\",\r\n      \"birthZIP\": \"Postal code of birth\",\r\n      \"confirmPassword\": \"Confirm Password\",\r\n      \"contactEmail\": \"Contact email\",\r\n      \"description\": \"Description\",\r\n      \"email\": \"E-mail\",\r\n      \"emailContactInfo\": \"For future communications you can set a different email\",\r\n      \"emailInfo\": \"We will send you an email to confirm the address\",\r\n      \"error\": \"An error occurred. Try again!\",\r\n      \"failureToAccept\": \"Mancata accettazione\",\r\n      \"firstName\": \"Name\",\r\n      \"fixedPhone\": \"Landline phone\",\r\n      \"lastName\": \"Surname\",\r\n      \"loginToComplete\": \"Log in to confirm your profile\",\r\n      \"man\": \"Man\",\r\n      \"mandatoryFields\": \"All fields marked with an asterisk are mandatory\",\r\n      \"minLenghtName\": \"I campi Nome e Cognome devono contenere almeno 2 caratteri\",\r\n      \"minLengthNames\": \"Enter at least two characters in the Name and Surname fields\",\r\n      \"mismatchPassword\": \"The two passwords do not match\",\r\n      \"missingData\": \"Dati mancanti\",\r\n      \"missionComplete\": \"Mission accomplished\",\r\n      \"mobilePhone\": \"Mobile phone\",\r\n      \"needAccept\": \"You must accept the Terms, Conditions and Privacy Policy!\",\r\n      \"nickName\": \"Nick-name\",\r\n      \"nickNameInfo\": \"Enter your Nick name\",\r\n      \"occupation\": \"Occupation\",\r\n      \"password\": \"Password\",\r\n      \"privacyPolicy\": \"the Privacy Policy\",\r\n      \"prohibitedOperation\": \"Operazione non consentita\",\r\n      \"recordingDisabled\": \"La registrazione a questo portale non è consentita\",\r\n      \"residenceAddress\": \"Residence address\",\r\n      \"residenceCity\": \"City of residence\",\r\n      \"residenceHouseNumber\": \"House number\",\r\n      \"residenceProvince\": \"Province of residence\",\r\n      \"residenceState\": \"State of residence\",\r\n      \"residenceZIP\": \"Postal code of residence\",\r\n      \"sex\": \"Gender\",\r\n      \"sigIn\": \"Log in\",\r\n      \"signIn\": \"Log in\",\r\n      \"signUp\": \"Sign up\",\r\n      \"taxId\": \"Tax ID code\",\r\n      \"terms\": \"Terms and conditions\",\r\n      \"unactiveRegistration\": \"Registration not active.\",\r\n      \"uncorrectConfirmPassword\": \"The format of the 'Confirm Password' field is incorrect\",\r\n      \"uncorrectContactEmail\": \"The format of the 'Contact Email' field is incorrect\",\r\n      \"uncorrectData\": \"Dati incorretti\",\r\n      \"uncorrectEmail\": \"The format of the 'Email' field is incorrect\",\r\n      \"uncorrectFormat\": \"Incorrect format\",\r\n      \"uncorrectPassword\": \"The format of the 'Password' field is incorrect\",\r\n      \"uncorrectTaxId\": \"The format of the 'Tax ID Code' field is incorrect\",\r\n      \"woman\": \"Woman\"\r\n    },\r\n    \"termsConditions\": {\r\n      \"inReview\": \"The document is under review. Please try again later!\",\r\n      \"unknownDocument\": \"The requested document does not exist!\",\r\n      \"version\": \"Version\",\r\n      \"waitPlease\": \"Recovering, please wait...\"\r\n    },\r\n    \"tr\": {\r\n      \"choice\": \"Choose the language\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"add\": \"AGGIUNGI\",\r\n      \"addMacro\": \"AGGIUNGI MACRO-AREA\",\r\n      \"addObject\": \"AGGIUNGI OGGETTO\",\r\n      \"addVoice\": \"AGGIUNGI VOCE\",\r\n      \"defaultStructure\": \"STRUTTURA DI DEFAULT\",\r\n      \"defaultValue\": \"VALORE PREDEFINITO\",\r\n      \"errorGettingData\": \"Errore nel recupero dei dati.\",\r\n      \"find\": \"cerca...\",\r\n      \"key\": \"CHIAVE\",\r\n      \"name\": \"NOME\",\r\n      \"nameLower\": \"Nome\",\r\n      \"notAvailableValue\": \"Non sono ancora presenti valori!\",\r\n      \"retry\": \"Riprova\",\r\n      \"stringKey\": \"Chiave della stringa\",\r\n      \"type\": \"Tipologia\",\r\n      \"value\": \"VALORE\",\r\n      \"valueLower\": \"Valore\",\r\n      \"wait\": \"Recupero dati in corso, attendere prego...\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"retry\": \"Riprova\",\r\n      \"choiceEntity\": \"SCEGLI L'ENTITÀ\",\r\n      \"unableFetchRoutes\": \"Errore nel recupero delle Entità.\",\r\n      \"crudOnEntity\": \"CRUD SULL'ENTITÀ\",\r\n      \"records\": \"record\",\r\n      \"wait\": \"Attendere prego...\",\r\n      \"create\": \"CREA\",\r\n      \"newRecord\": \"NUOVO<br />RECORD\",\r\n      \"filters\": \"FILTRI DI<br />RICERCA\",\r\n      \"find\": \"CERCA\"\r\n    },\r\n    \"messagesNotifications\": {\r\n      \"youHave\": \"Hai\",\r\n      \"unreadedMessages\": \"messaggi non letti\",\r\n      \"unreadedNotifications\": \"notifiche non lette\",\r\n      \"notificationsAndMessages\": \"Notifiche e Messaggi\",\r\n      \"noNotices\": \"Non ci sono notifiche o messaggi\",\r\n      \"notifications\": \"Notifiche\",\r\n      \"messages\": \"Messaggi\",\r\n      \"goToNotifications\": \"Vai alle Notifiche\",\r\n      \"goToMessages\": \"Vai ai Messaggi\",\r\n      \"waitPlease\": \"Attendere prego...\"\r\n    },\r\n    \"usersMaster\": {\r\n      \"users\": \"ELENCO UTENTI\",\r\n      \"add\": \"CREA NUOVO UTENTE\",\r\n      \"requests\": \"RICHIESTE DI AUTORIZZAZIONE\",\r\n      \"banned\": \"UTENTI BANNATI\"\r\n    },\r\n    \"setupSocial\": {\r\n      \"google\": \"GOOGLE\",\r\n      \"facebook\": \"FACEBOOK\",\r\n      \"thirdParts\": \"TERZE PARTI\"\r\n    },\r\n    \"setupMaster\": {\r\n      \"general\": \"GENERALE\",\r\n      \"security\": \"SICUREZZA\",\r\n      \"aspect\": \"ASPETTO\",\r\n      \"sliders\": \"SLIDERS\",\r\n      \"integrations\": \"INTEGRAZIONI\",\r\n      \"multiLanguage\": \"MULTI-LINGUA\",\r\n      \"routes\": \"ROTTE\",\r\n      \"entities\": \"ENTITÁ\",\r\n      \"userProfile\": \"PROFILO UTENTE\",\r\n      \"communications\": \"COMUNICAZIONI\",\r\n      \"operators\": \"OPERATORI\",\r\n      \"custom\": \"CUSTOM\"\r\n    }\r\n  },\r\n  \"controller\": {},\r\n  \"model\": {},\r\n  \"other\": {\r\n    \"slider\": {\r\n      \"slider1Description\": \"Description 1 for login and welcome page\",\r\n      \"slider1Title\": \"Title 1\",\r\n      \"slider2Description\": \"Description 2 for login and welcome page\",\r\n      \"slider2Title\": \"Title 2\",\r\n      \"slider3Description\": \"Description 3 for login and welcome page\",\r\n      \"slider3Title\": \"Title 3\"\r\n    },\r\n    \"utilsIncompleteConfig\": {\r\n      \"cookiesAcceptance\": \"Cookie Acceptance\",\r\n      \"privacyPolicy\": \"Privacy Policy\",\r\n      \"termsEndConditions\": \"Terms & Conditions\"\r\n    }\r\n  },\r\n  \"template\": {\r\n    \"articles\": {\r\n      \"subTitle\": \"Manage the site's articles\",\r\n      \"title\": \"Articles\"\r\n    },\r\n    \"categories\": {\r\n      \"subTitle\": \"Gestisci le Categorie del sito\",\r\n      \"title\": \"Categorie\"\r\n    },\r\n    \"changePassword\": {\r\n      \"subTitle\": \"Cambia la tua password di accesso\",\r\n      \"title\": \"Cambia password\"\r\n    },\r\n    \"developerGuide\": {\r\n      \"subTitle\": \"La guida sviluppatori del framework\",\r\n      \"title\": \"Guida all'uso\"\r\n    },\r\n    \"devices\": {\r\n      \"subTitle\": \"Gestisci i dispositivi con cui sei collegato\",\r\n      \"title\": \"I tuoi dispositivi\"\r\n    },\r\n    \"emptyLoggedPage\": {\r\n      \"subTitle\": \"Sottotitolo\",\r\n      \"title\": \"Titolo\"\r\n    },\r\n    \"home\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"legals\": {\r\n      \"subTitle\": \"Gestione dei documenti legali\",\r\n      \"title\": \"Documenti Legali\"\r\n    },\r\n    \"legalsDetails\": {\r\n      \"subTitle\": \"Gestisci i dettagli del documento\",\r\n      \"title\": \"Modifica Documento Legale\"\r\n    },\r\n    \"loading\": {\r\n      \"loading\": \"CARICAMENTO IN CORSO...\"\r\n    },\r\n    \"loggedHome\": {\r\n      \"subTitle\": \"Frontend ember-based framework\",\r\n      \"title\": \"Home\"\r\n    },\r\n    \"login\": {\r\n      \"enterCredentials\": \"Enter your login credentials.\",\r\n      \"forgotPassword\": \"Forgot password?\",\r\n      \"registration\": \"Sign in\",\r\n      \"unregistered\": \"Not registered?\",\r\n      \"welcome\": \"Welcome back,\"\r\n    },\r\n    \"media\": {\r\n      \"subTitle\": \"Gestisci i media dell'applicativo\",\r\n      \"title\": \"Media\"\r\n    },\r\n    \"recoveryPassword\": {\r\n      \"backwards\": \"indietro\",\r\n      \"email\": \"Email...\",\r\n      \"howTo\": \"Inserisci la tua e-mail e ti invieremo le istruzioni per creare una nuova password\",\r\n      \"return\": \"Torna\",\r\n      \"start\": \"Avvia il cambio password\",\r\n      \"title\": \"Recupero della password\"\r\n    },\r\n    \"registration\": {\r\n      \"howTo1\": \"create your account in\",\r\n      \"howTo2\": \"few seconds\",\r\n      \"welcome\": \"Welcome\"\r\n    },\r\n    \"rolePermissions\": {\r\n      \"crud\": \"PERMESSI CRUD\",\r\n      \"roles\": \"RUOLI\",\r\n      \"rolesPermissionsMapping\": \"ASSOCIAZIONI R&divide;P\",\r\n      \"routes\": \"ROTTE\",\r\n      \"subTitle\": \"Definisci ruoli e permessi di accesso\",\r\n      \"tenants\": \"TENANT\",\r\n      \"tenantsUsersMapping\": \"ASSOCIAZIONI T&divide;U\",\r\n      \"title\": \"Autorizzazioni\",\r\n      \"userRoles\": \"RUOLI UTENTI\"\r\n    },\r\n    \"setup\": {\r\n      \"subTitle\": \"Imposta i parametri di funzionamento dell'applicativo\",\r\n      \"title\": \"Setup\"\r\n    },\r\n    \"template\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"templateDetails\": {\r\n      \"subTitle\": \"Modifica i dettagli del template\",\r\n      \"title\": \"Modifica Template\"\r\n    },\r\n    \"templates\": {\r\n      \"subTitle\": \"Gestisci i template delle comunicazioni e-mail\",\r\n      \"title\": \"Template\"\r\n    },\r\n    \"tenantFallback\": {\r\n      \"subTitle\": \"Potrebbe essersi verificato un problema con la licenza di utilizzo\",\r\n      \"title\": \"Verifica della licenza\"\r\n    },\r\n    \"terms\": {\r\n      \"backwards\": \"back\",\r\n      \"return\": \"Come\"\r\n    },\r\n    \"translateStructure\": {\r\n      \"subTitle\": \"Imposta la struttura del file di traduzione\",\r\n      \"title\": \"Struttura traduzioni\"\r\n    },\r\n    \"translations\": {\r\n      \"subTitle\": \"Traduci il sito nelle lingue disponibili.\",\r\n      \"title\": \"Traduzioni\"\r\n    },\r\n    \"updatePassword\": {\r\n      \"howto\": \"Scegli la tua nuova password\",\r\n      \"title\": \"Modifica della password\"\r\n    },\r\n    \"userAudit\": {\r\n      \"subTitle\": \"Monitora le chiamate http e i dati relativi\",\r\n      \"title\": \"Audit\"\r\n    },\r\n    \"userGuide\": {\r\n      \"subTitle\": \"La guida per l'utilizzo ottimale del portale\",\r\n      \"title\": \"Guida d'uso e manutenzione\"\r\n    },\r\n    \"userProfile\": {\r\n      \"subTitle\": \"Gestisci i tuoi dati personali\",\r\n      \"title\": \"Profilo personale\"\r\n    },\r\n    \"users\": {\r\n      \"subTitle\": \"Gestisci gli utenti dell'azienda\",\r\n      \"title\": \"Utenti\"\r\n    },\r\n    \"generalCrud\": {\r\n      \"title\": \"CRUD\",\r\n      \"subTitle\": \"Gestisci i dati delle tue tabelle\"\r\n    },\r\n    \"notifications\": {\r\n      \"title\": \"Notifiche\",\r\n      \"subTitle\": \"Visualizza l'elenco delle notifiche ricevute\"\r\n    }\r\n  }\r\n}" }
                });


            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { "crud",   "OldPassword.create",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "OldPassword.update",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "OldPassword.read", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "MediaFile.read",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "MediaFile.delete", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "MediaFile.create", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "MediaFile.update", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "Role.create",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "Role.read",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "RoleClaim.read",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "Setup.read",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-translations", "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "crud",   "Translation.read", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-media",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "Tenant.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "OldPassword.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "OldPassword.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "OldPassword.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "OldPassword.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Role.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Role.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Role.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Role.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "RoleClaim.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "RoleClaim.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "RoleClaim.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "RoleClaim.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Setup.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Setup.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Setup.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Setup.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Tenant.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Tenant.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Tenant.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Translation.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Translation.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Translation.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Translation.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "User.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "User.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "User.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "User.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserAudit.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserAudit.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserAudit.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserAudit.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserDevice.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserDevice.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserDevice.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserDevice.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserRole.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserRole.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserRole.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserRole.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserTenant.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserTenant.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserTenant.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserTenant.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Category.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Category.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Category.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Category.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Template.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Template.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Template.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Template.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserProfile.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserProfile.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserProfile.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "UserProfile.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-roles-permissions",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-setup",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-template-details", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-templates",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-translate-structure",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-translations", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-users",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Category.read",    "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "crud",   "MediaFile.read",   "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "User.read",    "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "Category.read",    "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "Notification.read",    "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "UserDevice.read",  "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "UserProfile.read", "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "MediaFile.create", "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "MediaFile.update", "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "MediaFile.delete", "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "Notification.update",  "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "User.update",  "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "UserProfile.update",   "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "Notification.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Notification.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Notification.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Notification.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "PasswordHistory.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "PasswordHistory.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "PasswordHistory.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "PasswordHistory.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.read",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "User.read",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "Category.read",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "Notification.read",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "UserDevice.read",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "UserProfile.read", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "MediaFile.create", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "MediaFile.update", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "MediaFile.delete", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "Notification.update",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "User.update",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "UserProfile.update",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "custom", "isAdmin",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "custom", "isOwner",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "custom", "isSuperAdmin", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "custom", "canBypassMaintenance", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "custom", "canSeeAllTenants", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "custom", "canBypassMaintenance", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "custom", "canSeeAllTenants", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "custom", "canBypassMaintenance", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-devices",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-user-audit",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-media",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-categories",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.read",   "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "User.read",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "Category.read",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "Notification.read",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "UserDevice.read",  "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "UserProfile.read", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "MediaFile.create", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "MediaFile.update", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "MediaFile.delete", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "Notification.update",  "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "User.update",  "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "UserProfile.update",   "a17619b7-df13-441d-b590-41c11be55290" },
                    { "crud",   "RefreshToken.read",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-devices",  "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "Notification.delete",  "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "crud",   "UserDevice.update",    "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "custom", "canSeeIncompleteConfigurations",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "custom", "canSeeIncompleteConfigurations",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "custom", "canBypassMaintenance", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "custom", "canSeeIncompleteConfigurations",   "a17619b7-df13-441d-b590-41c11be55290" },
                    { "custom", "isMarketing",  "a17619b7-df13-441d-b590-41c11be55290" },
                    { "custom", "isAudit",  "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "custom", "canSeeIncompleteConfigurations",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "custom", "isTranslator", "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "custom", "canBypassMaintenance", "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "custom", "canSeeIncompleteConfigurations",   "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "custom", "isUser",   "deda3101-7acd-4f7c-b18d-571af08fe304" },
                    { "custom", "canSeeRightBar",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "custom", "canSeeAllTenants", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "custom", "canSeeRightBar",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "custom", "canSeeRightBar",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "custom", "canSeeRightBar",   "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-developer-guide",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-categories",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-devices",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-roles-permissions",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-setup",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-template-details", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-templates",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-translate-structure",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-translations", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-user-audit",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-users",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-developer-guide",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-user-guide",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-categories",   "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-devices",  "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-media",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-template-details", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-templates",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-users",    "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-user-guide",   "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-devices",  "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "route",  "r-p-user-audit",   "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "route",  "r-p-users",    "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "route",  "r-p-user-guide",   "d5696bd0-fe6e-4ca1-9053-8fa6c8a0475g" },
                    { "route",  "r-p-categories",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-devices",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-media",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-roles-permissions",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-users",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-user-guide",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-devices",  "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-template-details", "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-templates",    "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-translations", "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-user-guide",   "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "crud",   "MediaFile.read",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "User.read",    "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "Category.read",    "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "Notification.read",    "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "UserDevice.read",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "UserProfile.read", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "MediaFile.create", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "MediaFile.update", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "MediaFile.delete", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "Notification.update",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "User.update",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "UserProfile.update",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "route",  "r-p-devices",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "Notification.delete",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "UserDevice.update",    "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "custom", "isLegal",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "custom", "canBypassMaintenance", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "custom", "canSeeIncompleteConfigurations",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "route",  "r-p-user-guide",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "route",  "r-p-legals",   "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-legals",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "route",  "r-p-legals",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Template.update",  "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "Template.read",    "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "route",  "r-p-user-guide",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaFile.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "User.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Category.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Notification.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserDevice.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserProfile.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaFile.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaFile.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaFile.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Notification.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "User.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserProfile.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-devices",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Notification.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserDevice.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "custom", "canBypassMaintenance", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "custom", "canSeeIncompleteConfigurations",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "custom", "canSeeRightBar",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "custom", "isDeveloper",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Category.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Category.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Category.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Notification.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "OldPassword.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "OldPassword.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "OldPassword.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "OldPassword.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Otp.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Otp.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Otp.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Otp.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "PasswordHistory.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "PasswordHistory.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "PasswordHistory.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "PasswordHistory.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Role.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Role.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Role.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Role.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "RoleClaim.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "RoleClaim.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "RoleClaim.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "RoleClaim.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Setup.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Setup.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Setup.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Setup.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Template.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Template.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Template.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Template.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Tenant.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Tenant.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Tenant.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Tenant.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Translation.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Translation.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Translation.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Translation.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "User.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "User.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserAudit.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserAudit.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserAudit.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserAudit.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserDevice.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserDevice.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserProfile.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserProfile.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserRole.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserRole.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserRole.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserRole.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserTenant.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserTenant.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserTenant.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "UserTenant.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Otp.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Otp.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Otp.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Otp.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-categories",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-developer-guide",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-legals",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-media",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-roles-permissions",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-setup",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-template-details", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-templates",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-translate-structure",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-translations", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-user-audit",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-user-guide",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-users",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "LegalTerm.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "LegalTerm.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "LegalTerm.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "LegalTerm.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "LegalTerm.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "LegalTerm.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "LegalTerm.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "LegalTerm.create", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "LegalTerm.update", "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "LegalTerm.read",   "d23d7002-50ff-4017-951b-cd2357365641" },
                    { "crud",   "LegalTerm.read",   "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "LegalTerm.read",   "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "crud",   "LegalTerm.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-articles", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-articles", "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "route",  "r-p-articles", "a17619b7-df13-441d-b590-41c11be55290" },
                    { "route",  "r-p-articles", "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "route",  "r-p-articles", "3aa0263c-8627-47ca-2321-08db65c3e91c" },
                    { "route",  "r-p-articles", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-general-crud", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-general-crud", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "BannedUser.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "BannedUser.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "BannedUser.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "BannedUser.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "BannedUser.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "BannedUser.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "BannedUser.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "BannedUser.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "BannedUser.create",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "BannedUser.update",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "BannedUser.read",  "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "BannedUser.delete",    "413a0665-db4e-46c3-b061-897960aa0054" },
                    { "crud",   "BannedUser.create",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "BannedUser.update",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "BannedUser.read",  "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "BannedUser.delete",    "6a0dd2bc-2c3b-43fe-bd44-f8592217dda7" },
                    { "crud",   "Integration.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Integration.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Integration.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Integration.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Integration.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Integration.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Integration.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Integration.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-help-desk-admin",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-media-categories", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "route",  "r-p-help-desk-admin",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "route",  "r-p-media-categories", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "CustomSetup.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "CustomSetup.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "CustomSetup.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "CustomSetup.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaCategory.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaCategory.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaCategory.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "MediaCategory.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "ThirdPartsToken.create",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "ThirdPartsToken.update",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "ThirdPartsToken.read", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "ThirdPartsToken.delete",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Ticket.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Ticket.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Ticket.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "Ticket.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketArea.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketArea.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketArea.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketArea.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketAttachment.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketAttachment.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketAttachment.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketAttachment.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketHistory.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketHistory.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketHistory.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketHistory.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketMessage.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketMessage.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketMessage.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketMessage.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketOperator.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketOperator.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketOperator.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketOperator.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketRelation.create",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketRelation.update",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketRelation.read",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketRelation.delete",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTag.create", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTag.update", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTag.read",   "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTag.delete", "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTicketRelation.create",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTicketRelation.update",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTicketRelation.read",    "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "TicketTicketRelation.delete",  "46d0aee0-258f-4936-b185-78429efcdc9f" },
                    { "crud",   "CustomSetup.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "CustomSetup.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "CustomSetup.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "CustomSetup.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaCategory.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaCategory.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaCategory.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "MediaCategory.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "ThirdPartsToken.create",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "ThirdPartsToken.update",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "ThirdPartsToken.read", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "ThirdPartsToken.delete",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Ticket.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Ticket.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Ticket.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "Ticket.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketArea.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketArea.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketArea.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketArea.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketAttachment.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketAttachment.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketAttachment.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketAttachment.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketHistory.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketHistory.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketHistory.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketHistory.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketMessage.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketMessage.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketMessage.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketMessage.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketOperator.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketOperator.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketOperator.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketOperator.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketRelation.create",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketRelation.update",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketRelation.read",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketRelation.delete",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTag.create", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTag.update", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTag.read",   "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTag.delete", "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTicketRelation.create",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTicketRelation.update",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTicketRelation.read",    "d7516446-57ef-4e2a-bd4a-6816218a7dfb" },
                    { "crud",   "TicketTicketRelation.delete",  "d7516446-57ef-4e2a-bd4a-6816218a7dfb" }
                });

            migrationBuilder.InsertData(
                table: "UserProfile",
                columns: new[] {
                    "Id", "UserId", "FirstName", "LastName", "NickName", "FixedPhone", "MobilePhone", "Sex", "TaxId", "BirthDate",
                    "BirthCity", "BirthProvince", "BirthZIP", "BirthState", "ResidenceCity", "ResidenceProvince", "ResidenceZIP",
                    "ResidenceState", "ResidenceAddress", "ResidenceHouseNumber", "Occupation", "Description", "ContactEmail",
                    "ProfileImageId", "ProfileFreeFieldString1", "ProfileFreeFieldString2", "ProfileFreeFieldString3",
                    "ProfileFreeFieldInt1", "ProfileFreeFieldInt2", "ProfileFreeFieldDateTime", "ProfileFreeFieldBoolean",
                    "GoogleRefreshToken", "AppleRefreshToken", "FacebookRefreshToken", "TwitterRefreshToken", "UserLang",
                    "cookieAccepted", "termsAccepted", "termsAcceptanceDate", "registrationDate"
                },
                values: new object[,]
                {
                    {
                        "7686c9d5-6f73-4f81-b764-075efd3ce609", "207d02ef-7baa-4dca-a118-320f3ab0e853", "Unit", "Test", "", "", "", "", "",
                        DateTime.Parse("2023-07-04 00:00:00.000"), "", "", "", "", "", "", "", "", "", "", "", "", "unit.test@maestrale.it",
                        "", "", "", "", 0, 0, DateTime.Parse("2023-07-04 08:07:40.367"), false, null, null, null, null, null, null, null,
                        DateTime.Parse("1900-01-01 00:00:00.000"), null
                    },
                    {
                        "e8e675ed-98ae-4679-a33f-3b1342fd81de", "36030431-6ef3-4f66-bf9e-5f1945bd9408", "Superadmin", "Seeders", "", "", "", "", "",
                        DateTime.Parse("2023-07-04 00:00:00.000"), "", "", "", "", "", "", "", "", "", "", "", "", "superadmin.seeders@maestrale.it",
                        "", "", "", "", 0, 0, DateTime.Parse("2023-07-04 08:07:40.367"), false, null, null, null, null, null, null, null,
                        DateTime.Parse("1900-01-01 00:00:00.000"), null
                    }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] {
                    "UserId", "RoleId", "Id", "TenantId"
                },
                values: new object[,]
                {
                    {
                        "207d02ef-7baa-4dca-a118-320f3ab0e853", "46d0aee0-258f-4936-b185-78429efcdc9f", "f1f91046-c186-403b-9276-dd2541b3f4ae", 1
                    },
                    {
                        "36030431-6ef3-4f66-bf9e-5f1945bd9408", "46d0aee0-258f-4936-b185-78429efcdc9f", "35e8a2c2-6af3-44b9-a946-f3f289f17c6d", 1
                    }
                });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] {
                    "Name", "Description", "Type", "ParentCategory", "Erasable", "TenantId", "CopyInNewTenants", "Code", "Order"
                },
                values: new object[,]
                {
                    { "Template email", "Template delle varie email", "email", 0, false, 1, true, "template", 0 },
                    { "Guida sviluppo", "Guide allo sviluppo del Framework", null, 0, false, 1, false, "developerGuide", 0 },
                    { "Legal", "Contiene documenti di carattere legale", null, 0, false, 1, false, "legal", 0 },
                    { "Guida d'uso", "Le istruzioni per il corretto utilizzo dell'applicativo", null, 0, false, 1, false, "userGuide", 0 }
                });

            migrationBuilder.InsertData(
                table: "MediaCategories",
                columns: new[] { "Id", "Name", "Code", "Description", "Order", "Erasable", "TenantId", "Type", "ParentMediaCategory", "copyInNewTenant" },
                values: new object[,]
                {
                    { "015480bf-fb45-4831-9beb-125b2256212c", "UnloggedArea", "unloggedArea", "", 2, false, 1, "album", "8700be0e-c95a-4d23-8e7c-27924e8379cc", false },
                    { "067ccf84-35d3-44fe-b4fa-250162d71027", "System", "system", "", 0, false, 1, "typology", null, false },
                    { "0aeb1bd5-21ae-4b06-9562-f2878d35aa39", "Template Email", "templateEmail", "", 3, false, 1, "album", "8700be0e-c95a-4d23-8e7c-27924e8379cc", false },
                    { "42954541-f21a-40fe-b97d-2c84e3ebc8f5", "Sliders", "sliders", "", 0, false, 1, "category", "067ccf84-35d3-44fe-b4fa-250162d71027", false },
                    { "4a045802-3c68-4277-9d3d-c2810cec29fb", "Welcome", "welcome", "", 0, false, 1, "album", "42954541-f21a-40fe-b97d-2c84e3ebc8f5", false },
                    { "61b6a04b-fe07-4a69-bebe-8779bad362cc", "Profile Pics", "profilePics", "", 0, false, 1, "album", "a8f5b85b-43cb-43ed-932d-31f5fec0d7b5", false },
                    { "8700be0e-c95a-4d23-8e7c-27924e8379cc", "Images", "images", "", 1, false, 1, "category", "067ccf84-35d3-44fe-b4fa-250162d71027", false },
                    { "9bef327f-3067-4da0-9286-360368e91097", "Replacements", "replacements", "", 1, false, 1, "album", "8700be0e-c95a-4d23-8e7c-27924e8379cc", false },
                    { "a8f5b85b-43cb-43ed-932d-31f5fec0d7b5", "User Pics", "userPics", "", 0, false, 1, "category", "f4a68bbd-4ccc-4d30-95bd-5f674e3c62f2", false },
                    { "dd211486-4f60-4b99-ac44-fd57636a9399", "WebSite pics", "webSitePics", "", 3, false, 1, "album", "8700be0e-c95a-4d23-8e7c-27924e8379cc", false },
                    { "f4a68bbd-4ccc-4d30-95bd-5f674e3c62f2", "User Pics", "userPics", "", 1, false, 1, "typology", null, false }
                });

            migrationBuilder.InsertData(
                table: "UserTenants",
                columns: new[] { "UserId", "TenantId", "Id", "Ip", "State", "AcceptedAt", "CreatedAt" },
                values: new object[,]
                {
                    {
                        "207d02ef-7baa-4dca-a118-320f3ab0e853", 1, "8984755f-9949-45b7-8e37-6b00deca1d93", "", "accepted", null, DateTime.Parse("2023-07-25 10:04:57.993")
                    },
                    {
                        "36030431-6ef3-4f66-bf9e-5f1945bd9408", 1, "b5fa8cbd-5a55-4472-9b9d-928542ee3928", "", "accepted", null, DateTime.Parse("2023-07-25 10:04:57.993")
                    }
                });

            migrationBuilder.InsertData(
                table: "Template",
                columns: new[] {
                    "Name", "Description", "Content", "ContentNoHtml", "CategoryId", "Active", "Tags",
                    "Language", "Code", "Erasable", "Id", "ObjectText", "CopyInNewTenants", "Order",
                    "Erased", "FeaturedImage", "FreeField"
                },
                values: new object[,]
                {
                    {
                        "Associazione accettata",
                        "Inviata all'utente che accetta una richiesta di associazione.",
                        "<p>Ti confermiamo l'avvenuta registrazione ad una delle aziende del portale.</p>",
                        "", 1, true, "", "it", "9afe9b8c-de32-490a-873f-41171bcc1818", false, "0ef45362-0d0d-4ef9-bb93-abc7446bd441",
                        "Registrazione confermata",
                        false, 9, false, null, ""
                    },
                    {
                        "Footer generale",
                        "",
                        "<p>CONTENUTO DEL FOOTER</p>",
                        "", 1, true, "", "it", "cc77d9e7-a6f0-4eae-b843-b98d95b9d7b9", false, "28A31488-BA5A-4083-803A-D506F1A5793B",
                        null,
                        true, 2, false, null, ""
                    },
                    {
                        "Avvenuta registrazione social",
                        "Inviato quando un utente si registra mediante un servizio di integrazione esterno (es: Google)",
                        "<p>Benvenuto!</p><p>Ti informiamo che la tua registrazione mediante social-network è avvenuta correttamente.</p>",
                        "", 1, true, "", "it", "4bb9814c-b2ed-4159-891f-46dd700905f3", false, "5a0c7904-8d3f-4ea8-b118-979e27c89917",
                        "Registrazione avvenuta",
                        false, 10, false, null, ""
                    },
                    {
                        "Richiesta di associazione",
                        "Inviata quando un tenant vuole associare un utente che è già registrato ma ad un tenant diverso. L'utente dovrà confermare o rifiutare l'associazione.",
                        "<p>L'azienda&nbsp;<span class=\"placeholder\">{TenantName}</span>&nbsp;vorrebbe accedere ai tuoi dati. Clicca <a href=\"{BaseEndpoint}/access-permissions?otp={OTP}\">qui</a> per confermare o bloccare l'autorizzazione.</p>",
                        "", 1, true, "", "it", "em.10", false, "736803DA-C834-4F43-906C-B7937F34C2EC",
                        "Richiesta di registrazione",
                        false, 8, false, null, ""
                    },
                    {
                        "Registrazione utente da interfaccia amministrativa",
                        "Inviata agli utenti che vengono registrati da un operatore mediante la pagina di amministrazione degli Utenti.",
                        "<p>Ti confermiamo l'avvenuta registrazione ad una delle aziende del portale.</p>",
                        "", 1, true, "", "it", "f4646e12-f847-4ba9-bbb3-d60bac1f473d", false, "89c18939-a174-4eeb-adbd-752c698b4da3",
                        "Registrazione avvenuta",
                        false, 6, false, null, ""
                    },
                    {
                        "Reset password",
                        "Email inviata all'utente che richiede il recupero della password",
                        "<p><strong>Hai dimenticato la password di accesso? Nessun problema!</strong></p><p>Clicca il pulsante sottostante entro le prossime 2 ore per scegliere una nuova password.</p><p><a href=\"{BaseEndpoint}/recovery-password?ph=2&amp;email={UserEmail}&amp;otp={OTP}\">CREA UNA NUOVA PASSWORD</a></p><p>&nbsp;</p><p><span style=\"color:rgb(34,34,34);\">Non hai richiesto tu il recupero della password? Clicca qui per annullare l'operazione!</span></p>",
                        "", 1, true, "", "it", "499e95f2-27f2-4e14-b1ee-99c8aefcb6a2", false, "a9c1244d-5ce6-4a39-8cec-b895ff88bf0f",
                        "Recupero password dimenticata",
                        false, 3, false, null, ""
                    },
                    {
                        "Reset password avvenuto con successo",
                        "Inviato quando un utente aggiorna la sua password",
                        "<p>Ti confermiamo che il cambio della tua password è avvenuto correttamente.</p>",
                        "", 1, true, "", "it", "5547dcaf-75de-493e-a301-cbdce43bc300", false, "bc16fc0e-29d7-4b24-a5d1-f9de8054c511",
                        "Reset password avvenuto con successo",
                        false, 4, false, null, ""
                    },
                    {
                        "Header generale",
                        "",
                        "<p>CONTENUTO DELL'HEADER</p>",
                        "", 1, true, "", "it", "5697fb91-35c1-42fa-8be3-b6099f8b2fe2", false, "CEFC15BE-D6E8-48C3-AEA2-C4CEDA517CAD",
                        null,
                        true, 1, false, null, ""
                    },
                    {
                        "Registrazione utente",
                        "Inviata quando un nuovo utente si auto-registra",
                        "<p>Benvenuto!</p><p>Clicca <a href=\"{BaseEndpoint}/confirm-registration?otp={OTP}\">qui</a> per confermare la registrazione.</p>",
                        "", 1, true, "", "it", "0a16c97f-b76e-4042-802e-7c72d6cc337c", false, "d755fd6b-bfa3-45da-86db-5f042200d7ee",
                        "Registrazione avvenuta",
                        false, 5, false, null, ""
                    },
                    {
                        "Associazione utente",
                        "Inviata all'utente che era già registrato al sito ma si è auto-associato ad un altro tenant dello stesso sito",
                        "<p>Ti confermiamo l'avvenuta registrazione ad una delle aziende del portale.</p>",
                        "", 1, true, "", "it", "eb3c00b6-ed30-414c-b908-dd542b8db87d", false, "f5757abd-1907-43ed-a503-486c15a896c4",
                        "Associazione avvenuta",
                        false, 7, false, null, ""
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserAudit");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "BannedUsers");

            migrationBuilder.DropTable(
                name: "BlockNotifications");

            migrationBuilder.DropTable(
                name: "ChannelContentTypes");

            migrationBuilder.DropTable(
                name: "ChannelPricing");

            migrationBuilder.DropTable(
                name: "ChannelSchedules");

            migrationBuilder.DropTable(
                name: "CustomSetups");

            migrationBuilder.DropTable(
                name: "DeadlineArchives");

            migrationBuilder.DropTable(
                name: "ErpEmployeeRole");

            migrationBuilder.DropTable(
                name: "ErpEmployeeWorkingHours");

            migrationBuilder.DropTable(
                name: "ErpExternalWorkerDetails");

            migrationBuilder.DropTable(
                name: "ErpSiteUserMapping");

            migrationBuilder.DropTable(
                name: "ErpSiteWorkingTime");

            migrationBuilder.DropTable(
                name: "FwkAddons");

            migrationBuilder.DropTable(
                name: "GeoMappings");

            migrationBuilder.DropTable(
                name: "Integrations");

            migrationBuilder.DropTable(
                name: "LegalTerms");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Otps");

            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "PerformerReviews");

            migrationBuilder.DropTable(
                name: "PerformerServices");

            migrationBuilder.DropTable(
                name: "PerformerViews");

            migrationBuilder.DropTable(
                name: "Setups");

            migrationBuilder.DropTable(
                name: "Template");

            migrationBuilder.DropTable(
                name: "ThirdPartsTokens");

            migrationBuilder.DropTable(
                name: "TicketAttachments");

            migrationBuilder.DropTable(
                name: "TicketHistory");

            migrationBuilder.DropTable(
                name: "TicketTicketRelations");

            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropTable(
                name: "UploadFiles");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "UserPreference");

            migrationBuilder.DropTable(
                name: "UserProfile");

            migrationBuilder.DropTable(
                name: "UserTenants");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "ErpRole");

            migrationBuilder.DropTable(
                name: "ErpEmployee");

            migrationBuilder.DropTable(
                name: "ErpShift");

            migrationBuilder.DropTable(
                name: "ErpSite");

            migrationBuilder.DropTable(
                name: "GeoCities");

            migrationBuilder.DropTable(
                name: "GeoThirdDivisions");

            migrationBuilder.DropTable(
                name: "MediaCategories");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "TicketRelations");

            migrationBuilder.DropTable(
                name: "Performers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TicketOperators");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "GeoSecondDivisions");

            migrationBuilder.DropTable(
                name: "TicketAreas");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "TicketTags");

            migrationBuilder.DropTable(
                name: "GeoFirstDivisions");

            migrationBuilder.DropTable(
                name: "GeoCountries");

            migrationBuilder.DropTable(
                name: "GeoSubregions");

            migrationBuilder.DropTable(
                name: "GeoRegions");
        }
    }
}
