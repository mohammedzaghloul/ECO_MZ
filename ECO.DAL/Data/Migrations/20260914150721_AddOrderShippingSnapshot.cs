using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LocationGovernorates",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE Orders
                SET ShippingPrice = DeliveryMethods.Price
                FROM Orders
                INNER JOIN DeliveryMethods ON DeliveryMethods.Id = Orders.DeliveryMethodId
                WHERE Orders.ShippingPrice = 0;
                """);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LocationCities",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LocationGovernorates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LocationCities");
        }
    }
}
