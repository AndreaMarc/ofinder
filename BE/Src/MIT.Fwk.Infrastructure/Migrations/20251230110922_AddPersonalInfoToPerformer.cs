using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIT.Fwk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalInfoToPerformer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Performers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Performers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BirthState",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceProvince",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceRegion",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceState",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Performers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Channels",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSocial",
                table: "Channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Channels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Channels",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "BirthState",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "ResidenceProvince",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "ResidenceRegion",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "ResidenceState",
                table: "Performers");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Performers");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Performers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Channels",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSocial",
                table: "Channels",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Channels",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Channels",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");
        }
    }
}
