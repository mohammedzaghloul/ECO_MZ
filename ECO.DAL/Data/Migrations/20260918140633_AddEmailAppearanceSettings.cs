using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAppearanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAccentColor",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailBrandName",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailFooterNote",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailHeaderStyle",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAccentColor",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailBrandName",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailFooterNote",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailHeaderStyle",
                table: "StoreSettings");
        }
    }
}
