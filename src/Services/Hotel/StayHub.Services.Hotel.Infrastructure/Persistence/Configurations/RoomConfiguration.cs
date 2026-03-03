using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Room entity.
/// Maps scalar properties, owned Money value object, and JSON columns for collections.
/// </summary>
public sealed class RoomConfiguration : BaseEntityConfiguration<Room>
{
    public override void Configure(EntityTypeBuilder<Room> builder)
    {
        base.Configure(builder);

        builder.ToTable("Rooms");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.RoomType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.MaxOccupancy)
            .IsRequired();

        builder.Property(r => r.TotalInventory)
            .IsRequired();

        builder.Property(r => r.SizeInSquareMeters)
            .HasPrecision(8, 2);

        builder.Property(r => r.BedConfiguration)
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Owned value object: Money (BasePrice) ───────────────────────

        builder.OwnsOne(r => r.BasePrice, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .IsRequired()
                .HasColumnName("BasePrice_Amount");

            priceBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasColumnName("BasePrice_Currency");
        });

        // ── JSON columns for collections (EF Core 7+ / SQL Server) ─────

        builder.Property(r => r.Amenities)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasColumnName("Amenities")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(r => r.PhotoUrls)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)")
            .HasColumnName("PhotoUrls")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(r => r.HotelId)
            .HasDatabaseName("IX_Rooms_HotelId");

        builder.HasIndex(r => new { r.HotelId, r.Name })
            .IsUnique()
            .HasDatabaseName("IX_Rooms_HotelId_Name");

        builder.HasIndex(r => r.RoomType)
            .HasDatabaseName("IX_Rooms_RoomType");
    }
}
