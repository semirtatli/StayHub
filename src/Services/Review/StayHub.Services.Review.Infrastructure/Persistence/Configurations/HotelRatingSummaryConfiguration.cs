using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Review.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the HotelRatingSummary entity.
/// Denormalized read model with aggregated rating data per hotel.
/// </summary>
public sealed class HotelRatingSummaryConfiguration : BaseEntityConfiguration<HotelRatingSummary>
{
    public override void Configure(EntityTypeBuilder<HotelRatingSummary> builder)
    {
        base.Configure(builder);

        builder.ToTable("HotelRatingSummaries");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(s => s.HotelId)
            .IsRequired();

        builder.Property(s => s.TotalReviews)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.AverageOverall)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(s => s.AverageCleanliness)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(s => s.AverageService)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(s => s.AverageLocation)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(s => s.AverageComfort)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(s => s.AverageValueForMoney)
            .HasPrecision(3, 1)
            .IsRequired();

        // ── Indexes ─────────────────────────────────────────────────────

        // Unique index — one summary per hotel
        builder.HasIndex(s => s.HotelId)
            .IsUnique()
            .HasDatabaseName("IX_HotelRatingSummaries_HotelId");
    }
}
