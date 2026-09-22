using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixDeliveryMethodIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert delivery methods with IDENTITY_INSERT ON
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT DeliveryMethods ON;

                IF NOT EXISTS (SELECT 1 FROM DeliveryMethods WHERE Id = 1)
                INSERT INTO DeliveryMethods (Id, Name, Description, DeliveryTime, Price, LogoUrl)
                VALUES (1, 'DHL Express', 'Express delivery', '1-2 business days', 30, 'https://cdn.simpleicons.org/dhl/FFCC00');

                IF NOT EXISTS (SELECT 1 FROM DeliveryMethods WHERE Id = 2)
                INSERT INTO DeliveryMethods (Id, Name, Description, DeliveryTime, Price, LogoUrl)
                VALUES (2, 'FedEx', 'Reliable priority delivery', '2-3 business days', 20, 'https://cdn.simpleicons.org/fedex/4D148C');

                IF NOT EXISTS (SELECT 1 FROM DeliveryMethods WHERE Id = 3)
                INSERT INTO DeliveryMethods (Id, Name, Description, DeliveryTime, Price, LogoUrl)
                VALUES (3, 'Aramex', 'Fast local delivery', '3-5 business days', 15, 'https://cdn.simpleicons.org/aramex/D71920');

                IF NOT EXISTS (SELECT 1 FROM DeliveryMethods WHERE Id = 4)
                INSERT INTO DeliveryMethods (Id, Name, Description, DeliveryTime, Price, LogoUrl)
                VALUES (4, 'Bosta', 'Affordable doorstep delivery', '4-6 business days', 10, 'https://cdn.simpleicons.org/bosta/111827');

                IF NOT EXISTS (SELECT 1 FROM DeliveryMethods WHERE Id = 5)
                INSERT INTO DeliveryMethods (Id, Name, Description, DeliveryTime, Price, LogoUrl)
                VALUES (5, 'Egypt Post', 'Economy delivery', '5-7 business days', 5, 'https://cdn.simpleicons.org/egyptpost/0B5FA5');

                SET IDENTITY_INSERT DeliveryMethods OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM DeliveryMethods WHERE Id IN (1, 2, 3, 4, 5)");
        }
    }
}
