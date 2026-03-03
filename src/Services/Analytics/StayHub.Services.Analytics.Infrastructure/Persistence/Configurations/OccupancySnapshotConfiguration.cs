using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Analytics.Domain.Entities;

namespace StayHub.Services.Analytics.Infrastructure.Persistence.Configurations;

public sealed class OccupancySnapshotConfiguration : IEntityTypeConfiguration<OccupancySnapshot>
{
    public void Configure(EntityTypeBuilder<OccupancySnapshot> builder)
    {
        builder.ToTable("OccupancySnapshots");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.HotelId)
            .IsRequired();

        builder.Property(e => e.Date)
            .IsRequired();

        builder.Property(e => e.OccupancyRate)
            .HasPrecision(5, 2);

        // Unique constraint: one snapshot per hotel per day
        builder.HasIndex(e => new { e.HotelId, e.Date })
            .IsUnique();

        builder.HasIndex(e => e.Date);
    }
}
