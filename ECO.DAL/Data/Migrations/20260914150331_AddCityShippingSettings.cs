using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCityShippingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryDays",
                table: "LocationCities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShippingAvailable",
                table: "LocationCities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingPrice",
                table: "LocationCities",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE LocationCities SET ShippingPrice = 50, DeliveryDays = 2, ShippingAvailable = 1 WHERE Value IN ('Nasr City', 'Heliopolis', 'Haram');
                UPDATE LocationCities SET ShippingPrice = 55, DeliveryDays = 2, ShippingAvailable = 1 WHERE Value = 'New Cairo';
                UPDATE LocationCities SET ShippingPrice = 45, DeliveryDays = 2, ShippingAvailable = 1 WHERE Value = 'Dokki';
                UPDATE LocationCities SET ShippingPrice = 60, DeliveryDays = 3, ShippingAvailable = 1 WHERE Value IN ('6th of October', 'Mansoura', 'Talkha');
                UPDATE LocationCities SET ShippingPrice = 70, DeliveryDays = 5, ShippingAvailable = 0 WHERE Value = 'Mit Ghamr';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDays",
                table: "LocationCities");

            migrationBuilder.DropColumn(
                name: "ShippingAvailable",
                table: "LocationCities");

            migrationBuilder.DropColumn(
                name: "ShippingPrice",
                table: "LocationCities");
        }
    }
}
