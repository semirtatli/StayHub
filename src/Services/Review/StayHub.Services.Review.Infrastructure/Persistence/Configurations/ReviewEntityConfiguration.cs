using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Review.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Review aggregate root.
/// Maps the entity, owned Rating value object, and indexes.
/// </summary>
public sealed class ReviewEntityConfiguration : BaseEntityConfiguration<ReviewEntity>
{
    public override void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Reviews");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(r => r.HotelId)
            .IsRequired();

        builder.Property(r => r.BookingId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(r => r.GuestName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Body)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(r => r.StayedFrom)
            .IsRequired();

        builder.Property(r => r.StayedTo)
            .IsRequired();

        builder.Property(r => r.ManagementResponse)
            .HasMaxLength(2000);

        builder.Property(r => r.ManagementResponseAt);

        // ── Owned value object: Rating ──────────────────────────────────

        builder.OwnsOne(r => r.Rating, ratingBuilder =>
        {
            ratingBuilder.Property(rt => rt.Cleanliness)
                .HasColumnName("Rating_Cleanliness")
                .IsRequired();

            ratingBuilder.Property(rt => rt.Service)
                .HasColumnName("Rating_Service")
                .IsRequired();

            ratingBuilder.Property(rt => rt.Location)
                .HasColumnName("Rating_Location")
                .IsRequired();

            ratingBuilder.Property(rt => rt.Comfort)
                .HasColumnName("Rating_Comfort")
                .IsRequired();

            ratingBuilder.Property(rt => rt.ValueForMoney)
                .HasColumnName("Rating_ValueForMoney")
                .IsRequired();

            ratingBuilder.Property(rt => rt.Overall)
                .HasColumnName("Rating_Overall")
                .HasPrecision(3, 1)
                .IsRequired();
        });

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(r => r.HotelId)
            .HasDatabaseName("IX_Reviews_HotelId");

        builder.HasIndex(r => r.BookingId)
            .HasDatabaseName("IX_Reviews_BookingId");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_Reviews_UserId");

        // Unique constraint: one review per booking
        builder.HasIndex(r => new { r.UserId, r.BookingId })
            .IsUnique()
            .HasDatabaseName("IX_Reviews_UserId_BookingId");
    }
}
