using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Analytics.Domain.Entities;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.Configurations;

public sealed class DailyRevenueSnapshotConfiguration : IEntityTypeConfiguration<DailyRevenueSnapshot>
{
    public void Configure(EntityTypeBuilder<DailyRevenueSnapshot> builder)
    {
        builder.ToTable("DailyRevenueSnapshots");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.HotelId)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.TotalRevenue)
            .HasPrecision(18, 2);

        builder.Property(e => e.AverageBookingValue)
            .HasPrecision(18, 2);

        builder.Property(e => e.RefundAmount)
            .HasPrecision(18, 2);

        // Unique constraint: one snapshot per hotel per day
        builder.HasIndex(e => new { e.HotelId, e.Date })
            .IsUnique();

        builder.HasIndex(e => e.Date);
    }
}
