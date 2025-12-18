using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIT.Fwk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesAddRegionsAndGeneralInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Performers_GeoCountries_GeoCountryId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_GeoFirstDivisions_GeoFirstDivisionId",
                table: "Performers");

            migrationBuilder.DropForeignKey(
                name: "FK_Performers_GeoSecondDivisions_GeoSecondDivisionId",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_GeoCountryId",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_GeoFirstDivisionId",
                table: "Performers");

            migrationBuilder.DropIndex(
                name: "IX_Performers_GeoSecondDivisionId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "GeoCountryId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "GeoFirstDivisionId",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "GeoSecondDivisionId",
                table: "Performers");

            migrationBuilder.AddColumn<string>(
                name: "birthRegion",
                table: "UserProfile",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "residenceRegion",
                table: "UserProfile",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "generalInformation",
                table: "Setups",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthRegion",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "residenceRegion",
                table: "UserProfile");

            migrationBuilder.DropColumn(
                name: "generalInformation",
                table: "Setups");

            migrationBuilder.AddColumn<int>(
                name: "GeoCountryId",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeoFirstDivisionId",
                table: "Performers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeoSecondDivisionId",
                table: "Performers",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_GeoCountries_GeoCountryId",
                table: "Performers",
                column: "GeoCountryId",
                principalTable: "GeoCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_GeoFirstDivisions_GeoFirstDivisionId",
                table: "Performers",
                column: "GeoFirstDivisionId",
                principalTable: "GeoFirstDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Performers_GeoSecondDivisions_GeoSecondDivisionId",
                table: "Performers",
                column: "GeoSecondDivisionId",
                principalTable: "GeoSecondDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
