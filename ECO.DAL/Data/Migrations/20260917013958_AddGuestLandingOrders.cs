using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestLandingOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "Orders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LandingPageId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_LandingPageId",
                table: "Orders",
                column: "LandingPageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_LandingPages_LandingPageId",
                table: "Orders",
                column: "LandingPageId",
                principalTable: "LandingPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_LandingPages_LandingPageId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_LandingPageId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LandingPageId",
                table: "Orders");
        }
    }
}
