using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayHub.Services.Review.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HotelRatingSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageOverall = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    AverageCleanliness = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    AverageService = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    AverageLocation = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    AverageComfort = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    AverageValueForMoney = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelRatingSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Rating_Cleanliness = table.Column<int>(type: "int", nullable: false),
                    Rating_Service = table.Column<int>(type: "int", nullable: false),
                    Rating_Location = table.Column<int>(type: "int", nullable: false),
                    Rating_Comfort = table.Column<int>(type: "int", nullable: false),
                    Rating_ValueForMoney = table.Column<int>(type: "int", nullable: false),
                    Rating_Overall = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    StayedFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    StayedTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ManagementResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ManagementResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelRatingSummaries_HotelId",
                table: "HotelRatingSummaries",
                column: "HotelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotelRatingSummary_NotDeleted",
                table: "HotelRatingSummaries",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedAtUtc",
                table: "OutboxMessages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Unprocessed",
                table: "OutboxMessages",
                column: "ProcessedAtUtc",
                filter: "[ProcessedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEntity_NotDeleted",
                table: "Reviews",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_HotelId",
                table: "Reviews",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_BookingId",
                table: "Reviews",
                columns: new[] { "UserId", "BookingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotelRatingSummaries");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
