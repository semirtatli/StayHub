using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayHub.Services.Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuestUserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    HotelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CheckIn = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOut = table.Column<DateOnly>(type: "date", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    Guest_FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Guest_LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Guest_Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Guest_Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NightlyRate_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NightlyRate_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Price_Nights = table.Column<int>(type: "int", nullable: false),
                    Subtotal_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Tax_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ServiceFee_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceFee_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Total_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    SpecialRequests = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RefundPercentage = table.Column<int>(type: "int", nullable: true),
                    Refund_Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Refund_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
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
                    table.PrimaryKey("PK_Bookings", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_BookingEntity_NotDeleted",
                table: "Bookings",
                column: "IsDeleted",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConfirmationNumber",
                table: "Bookings",
                column: "ConfirmationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestUserId",
                table: "Bookings",
                column: "GuestUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HotelId",
                table: "Bookings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId_Status",
                table: "Bookings",
                columns: new[] { "RoomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CreatedAtUtc",
                table: "OutboxMessages",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Unprocessed",
                table: "OutboxMessages",
                column: "ProcessedAtUtc",
                filter: "[ProcessedAtUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "OutboxMessages");
        }
    }
}
