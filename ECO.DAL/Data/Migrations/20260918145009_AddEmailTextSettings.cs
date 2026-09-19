using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTextSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailCtaText",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailEyebrowText",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailGreetingText",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailIntroText",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCtaText",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailEyebrowText",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailGreetingText",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "EmailIntroText",
                table: "StoreSettings");
        }
    }
}
