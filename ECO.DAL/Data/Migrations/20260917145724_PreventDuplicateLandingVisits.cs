using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateLandingVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH duplicate_visits AS
                (
                    SELECT Id,
                           ROW_NUMBER() OVER
                           (
                               PARTITION BY LandingPageId, EventType, SessionId
                               ORDER BY Id
                           ) AS duplicate_number
                    FROM LandingPageEvents
                    WHERE EventType = 'visit'
                )
                DELETE FROM LandingPageEvents
                WHERE Id IN
                (
                    SELECT Id
                    FROM duplicate_visits
                    WHERE duplicate_number > 1
                );
                """);

            migrationBuilder.DropIndex(
                name: "IX_LandingPageEvents_LandingPageId_EventType_SessionId",
                table: "LandingPageEvents");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageEvents_LandingPageId_EventType_SessionId",
                table: "LandingPageEvents",
                columns: new[] { "LandingPageId", "EventType", "SessionId" },
                unique: true,
                filter: "[EventType] = 'visit'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LandingPageEvents_LandingPageId_EventType_SessionId",
                table: "LandingPageEvents");

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageEvents_LandingPageId_EventType_SessionId",
                table: "LandingPageEvents",
                columns: new[] { "LandingPageId", "EventType", "SessionId" });
        }
    }
}
