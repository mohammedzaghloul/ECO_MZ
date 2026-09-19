using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RepairStoreSettingsSmtpColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpFrom",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPasswordProtected",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "StoreSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUseSsl",
                table: "StoreSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpFrom",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPasswordProtected",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUseSsl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "StoreSettings");
        }
    }
}
