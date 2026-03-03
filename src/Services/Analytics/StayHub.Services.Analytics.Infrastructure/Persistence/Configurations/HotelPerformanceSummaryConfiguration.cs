using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Analytics.Domain.Entities;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.Configurations;

public sealed class HotelPerformanceSummaryConfiguration : IEntityTypeConfiguration<HotelPerformanceSummary>
{
    public void Configure(EntityTypeBuilder<HotelPerformanceSummary> builder)
    {
        builder.ToTable("HotelPerformanceSummaries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.HotelId)
            .IsRequired();

        builder.Property(e => e.HotelName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TotalRevenue)
            .HasPrecision(18, 2);

        builder.Property(e => e.AverageRating)
            .HasPrecision(3, 2);

        builder.Property(e => e.CancellationRate)
            .HasPrecision(5, 2);

        builder.Property(e => e.AverageOccupancyRate)
            .HasPrecision(5, 2);

        // Unique constraint: one summary per hotel
        builder.HasIndex(e => e.HotelId)
            .IsUnique();
    }
}
