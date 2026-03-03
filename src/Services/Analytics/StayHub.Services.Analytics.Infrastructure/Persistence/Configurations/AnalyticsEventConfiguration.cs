using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Analytics.Domain.Entities;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.Configurations;

public sealed class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("AnalyticsEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.HotelId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Metadata)
            .HasMaxLength(4000);

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(e => e.HotelId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => new { e.HotelId, e.OccurredAt });
    }
}
