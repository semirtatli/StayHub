using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Payment.Domain.Entities;
using StayHub.Services.Payment.Domain.Enums;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Payment aggregate root.
/// Maps the entity, owned value objects (Money), and indexes.
/// </summary>
public sealed class PaymentEntityConfiguration : BaseEntityConfiguration<PaymentEntity>
{
    public override void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Payments");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(p => p.BookingId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ProviderTransactionId)
            .HasMaxLength(256);

        builder.Property(p => p.ClientSecret)
            .HasMaxLength(512);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);

        builder.Property(p => p.PaidAt);
        builder.Property(p => p.FailedAt);
        builder.Property(p => p.CancelledAt);

        // ── Owned value object: Amount ──────────────────────────────────

        builder.OwnsOne(p => p.Amount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // ── Owned value object: RefundedAmount ──────────────────────────

        builder.OwnsOne(p => p.RefundedAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("Refunded_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("Refunded_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(p => p.BookingId)
            .HasDatabaseName("IX_Payments_BookingId");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Payments_UserId");

        builder.HasIndex(p => p.ProviderTransactionId)
            .IsUnique()
            .HasFilter("[ProviderTransactionId] IS NOT NULL")
            .HasDatabaseName("IX_Payments_ProviderTransactionId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");
    }
}
