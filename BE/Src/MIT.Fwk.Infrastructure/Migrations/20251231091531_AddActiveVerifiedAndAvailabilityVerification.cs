using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIT.Fwk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveVerifiedAndAvailabilityVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Performers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Verified",
                table: "Performers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvailabilityVerifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    PerformerId = table.Column<string>(type: "varchar(255)", nullable: false),
                    From = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    To = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Chosen = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Canceled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Missing = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailabilityVerifications_Performers_PerformerId",
                        column: x => x.PerformerId,
                        principalTable: "Performers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityVerifications_PerformerId",
                table: "AvailabilityVerifications",
                column: "PerformerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityVerifications");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Performers");
        }
    }
}
