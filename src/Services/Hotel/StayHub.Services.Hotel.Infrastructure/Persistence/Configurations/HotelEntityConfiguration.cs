using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Hotel aggregate root.
/// Maps the entity, owned value objects, and navigation properties.
/// </summary>
public sealed class HotelEntityConfiguration : BaseEntityConfiguration<HotelEntity>
{
    public override void Configure(EntityTypeBuilder<HotelEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Hotels");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(h => h.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(h => h.StarRating)
            .IsRequired();

        builder.Property(h => h.OwnerId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(h => h.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(HotelStatus.Draft);

        builder.Property(h => h.StatusReason)
            .HasMaxLength(1000);

        builder.Property(h => h.CheckInTime)
            .IsRequired();

        builder.Property(h => h.CheckOutTime)
            .IsRequired();

        builder.Property(h => h.CoverImageUrl)
            .HasMaxLength(2048);

        // ── Photo gallery (JSON column) ────────────────────────────────

        builder.Property(h => h.PhotoUrls)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasColumnName("PhotoUrls");

        builder.Navigation(h => h.PhotoUrls)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ── Owned value object: Address ─────────────────────────────────

        builder.OwnsOne(h => h.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasMaxLength(300)
                .IsRequired()
                .HasColumnName("Address_Street");

            addressBuilder.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("Address_City");

            addressBuilder.Property(a => a.State)
                .HasMaxLength(100)
                .HasColumnName("Address_State");

            addressBuilder.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("Address_Country");

            addressBuilder.Property(a => a.ZipCode)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("Address_ZipCode");
        });

        // ── Owned value object: GeoLocation (optional) ──────────────────

        builder.OwnsOne(h => h.Location, locationBuilder =>
        {
            locationBuilder.Property(l => l.Latitude)
                .HasColumnName("Location_Latitude");

            locationBuilder.Property(l => l.Longitude)
                .HasColumnName("Location_Longitude");
        });

        // ── Owned value object: ContactInfo ─────────────────────────────

        builder.OwnsOne(h => h.ContactInfo, contactBuilder =>
        {
            contactBuilder.Property(c => c.Phone)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("Contact_Phone");

            contactBuilder.Property(c => c.Email)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("Contact_Email");

            contactBuilder.Property(c => c.Website)
                .HasMaxLength(500)
                .HasColumnName("Contact_Website");
        });

        // ── Owned value object: CancellationPolicy ──────────────────────

        builder.OwnsOne(h => h.CancellationPolicy, policyBuilder =>
        {
            policyBuilder.Property(p => p.PolicyType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("CancellationPolicy_Type");

            policyBuilder.Property(p => p.FreeCancellationDays)
                .IsRequired()
                .HasColumnName("CancellationPolicy_FreeDays");

            policyBuilder.Property(p => p.PartialRefundPercentage)
                .IsRequired()
                .HasColumnName("CancellationPolicy_PartialPct");

            policyBuilder.Property(p => p.PartialRefundDays)
                .IsRequired()
                .HasColumnName("CancellationPolicy_PartialDays");
        });

        // ── Navigation: Rooms ───────────────────────────────────────────

        builder.HasMany(h => h.Rooms)
            .WithOne()
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field for private collection
        builder.Navigation(h => h.Rooms)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(h => h.OwnerId)
            .HasDatabaseName("IX_Hotels_OwnerId");

        builder.HasIndex(h => h.Status)
            .HasDatabaseName("IX_Hotels_Status");

        builder.HasIndex(h => new { h.Name, h.OwnerId })
            .IsUnique()
            .HasDatabaseName("IX_Hotels_Name_OwnerId");
    }
}
