using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIT.Fwk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSocialToChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSocial",
                table: "Channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSocial",
                table: "Channels");
        }
    }
}
