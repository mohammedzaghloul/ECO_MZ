using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryCompanyLogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "LogoUrl",
                value: "https://cdn.simpleicons.org/dhl/FFCC00");

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "LogoUrl",
                value: "https://cdn.simpleicons.org/fedex/4D148C");

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 3,
                column: "LogoUrl",
                value: "https://cdn.simpleicons.org/aramex/D71920");

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 4,
                column: "LogoUrl",
                value: "https://cdn.simpleicons.org/bosta/111827");

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 5,
                column: "LogoUrl",
                value: "https://cdn.simpleicons.org/egyptpost/0B5FA5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "DeliveryMethods");
        }
    }
}
