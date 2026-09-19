using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingPageEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LandingPageId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandingPageEvents_LandingPages_LandingPageId",
                        column: x => x.LandingPageId,
                        principalTable: "LandingPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageEvents_LandingPageId_CreatedAt",
                table: "LandingPageEvents",
                columns: new[] { "LandingPageId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LandingPageEvents_LandingPageId_EventType_SessionId",
                table: "LandingPageEvents",
                columns: new[] { "LandingPageId", "EventType", "SessionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageEvents");
        }
    }
}
