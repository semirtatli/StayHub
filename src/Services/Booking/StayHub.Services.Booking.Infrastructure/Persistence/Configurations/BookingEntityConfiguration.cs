using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Booking.Domain.Entities;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Booking.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Booking aggregate root.
/// Maps the entity, owned value objects, and indexes.
/// </summary>
public sealed class BookingEntityConfiguration : BaseEntityConfiguration<BookingEntity>
{
    public override void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Bookings");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(b => b.HotelId)
            .IsRequired();

        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.Property(b => b.GuestUserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(b => b.HotelName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.RoomName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.ConfirmationNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(b => b.NumberOfGuests)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(BookingStatus.Pending);

        builder.Property(b => b.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(b => b.SpecialRequests)
            .HasMaxLength(2000);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(b => b.CancelledAt);
        builder.Property(b => b.CheckedInAt);
        builder.Property(b => b.CompletedAt);

        builder.Property(b => b.PaymentIntentId)
            .HasMaxLength(256);

        builder.Property(b => b.RefundPercentage);

        // ── Owned value object: RefundAmount (optional) ─────────────────

        builder.OwnsOne(b => b.RefundAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("Refund_Amount")
                .HasPrecision(18, 2);

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("Refund_Currency")
                .HasMaxLength(3);
        });

        // ── Owned value object: StayPeriod ──────────────────────────────

        builder.OwnsOne(b => b.StayPeriod, stayBuilder =>
        {
            stayBuilder.Property(s => s.CheckIn)
                .IsRequired()
                .HasColumnName("CheckIn");

            stayBuilder.Property(s => s.CheckOut)
                .IsRequired()
                .HasColumnName("CheckOut");
        });

        // ── Owned value object: GuestInfo ──────────────────────────────

        builder.OwnsOne(b => b.GuestInfo, guestBuilder =>
        {
            guestBuilder.Property(g => g.FirstName)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("Guest_FirstName");

            guestBuilder.Property(g => g.LastName)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("Guest_LastName");

            guestBuilder.Property(g => g.Email)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("Guest_Email");

            guestBuilder.Property(g => g.Phone)
                .HasMaxLength(30)
                .HasColumnName("Guest_Phone");
        });

        // ── Owned value object: PriceBreakdown ──────────────────────────

        builder.OwnsOne(b => b.PriceBreakdown, priceBuilder =>
        {
            priceBuilder.OwnsOne(p => p.NightlyRate, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("NightlyRate_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("NightlyRate_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            priceBuilder.Property(p => p.Nights)
                .HasColumnName("Price_Nights")
                .IsRequired();

            priceBuilder.OwnsOne(p => p.Subtotal, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("Subtotal_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("Subtotal_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            priceBuilder.OwnsOne(p => p.TaxAmount, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("Tax_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("Tax_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            priceBuilder.OwnsOne(p => p.ServiceFee, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("ServiceFee_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("ServiceFee_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            priceBuilder.OwnsOne(p => p.Total, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("Total_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("Total_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(b => b.ConfirmationNumber)
            .IsUnique()
            .HasDatabaseName("IX_Bookings_ConfirmationNumber");

        builder.HasIndex(b => b.GuestUserId)
            .HasDatabaseName("IX_Bookings_GuestUserId");

        builder.HasIndex(b => b.HotelId)
            .HasDatabaseName("IX_Bookings_HotelId");

        builder.HasIndex(b => new { b.RoomId, b.Status })
            .HasDatabaseName("IX_Bookings_RoomId_Status");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Bookings_Status");
    }
}
