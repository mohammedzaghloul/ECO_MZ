using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeliveryMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeliveryTime", "Description", "Name" },
                values: new object[] { "1-2 business days", "Express delivery", "DHL Express" });

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeliveryTime", "Description", "Name" },
                values: new object[] { "2-3 business days", "Reliable priority delivery", "FedEx" });

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DeliveryTime", "Description", "Name", "Price" },
                values: new object[] { "3-5 business days", "Fast local delivery", "Aramex", 15m });

            migrationBuilder.InsertData(
                table: "DeliveryMethods",
                columns: new[] { "Id", "DeliveryTime", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { 4, "4-6 business days", "Affordable doorstep delivery", "Bosta", 10m },
                    { 5, "5-7 business days", "Economy delivery", "Egypt Post", 5m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeliveryTime", "Description", "Name" },
                values: new object[] { "Only 1 Week", "The Fast Delivery in World", "DHL" });

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeliveryTime", "Description", "Name" },
                values: new object[] { "Only 2 Week", "The Fast Delivery in World", "DHL" });

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DeliveryTime", "Description", "Name", "Price" },
                values: new object[] { "Only 3 Week", "The Fast Delivery in World", "DHL", 110m });
        }
    }
}
