using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the RoomAvailability entity.
///
/// One record per Room × Date — the core table for the availability engine.
/// Unique index on (RoomId, Date) ensures no duplicate entries.
/// RowVersion enables optimistic concurrency (prevents double-booking races).
/// </summary>
public sealed class RoomAvailabilityConfiguration : BaseEntityConfiguration<RoomAvailability>
{
    public override void Configure(EntityTypeBuilder<RoomAvailability> builder)
    {
        base.Configure(builder);

        builder.ToTable("RoomAvailability");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(a => a.RoomId)
            .IsRequired();

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.TotalInventory)
            .IsRequired();

        builder.Property(a => a.BookedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(a => a.PriceOverride)
            .HasPrecision(18, 2);

        builder.Property(a => a.IsBlocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.BlockReason)
            .HasMaxLength(500);

        // ── Computed column (SQL Server) ────────────────────────────────
        // AvailableCount is a C# computed property — no DB column needed.
        builder.Ignore(a => a.AvailableCount);

        // ── Relationships ───────────────────────────────────────────────

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(a => a.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ─────────────────────────────────────────────────────

        // Primary lookup: availability for a room on a specific date
        builder.HasIndex(a => new { a.RoomId, a.Date })
            .IsUnique()
            .HasDatabaseName("IX_RoomAvailability_RoomId_Date");

        // Query: all availability for a date range (across rooms)
        builder.HasIndex(a => a.Date)
            .HasDatabaseName("IX_RoomAvailability_Date");

        // Query: available rooms on a date (filter by non-blocked, available > 0)
        builder.HasIndex(a => new { a.RoomId, a.Date, a.IsBlocked })
            .HasDatabaseName("IX_RoomAvailability_RoomId_Date_IsBlocked");
    }
}
