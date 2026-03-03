using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Notification.Domain.Entities;
using StayHub.Services.Notification.Domain.Enums;
using StayHub.Shared.Infrastructure.Persistence.Configuration;

namespace StayHub.Services.Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Notification aggregate root.
/// </summary>
public sealed class NotificationEntityConfiguration : BaseEntityConfiguration<NotificationEntity>
{
    public override void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Notifications");

        // ── Scalar properties ───────────────────────────────────────────

        builder.Property(n => n.UserId)
            .HasMaxLength(256);

        builder.Property(n => n.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Recipient)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Body)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(NotificationStatus.Pending);

        builder.Property(n => n.SentAt);
        builder.Property(n => n.FailedAt);

        builder.Property(n => n.FailureReason)
            .HasMaxLength(2000);

        builder.Property(n => n.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.CorrelationId);

        // ── Indexes ─────────────────────────────────────────────────────

        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("IX_Notifications_Status");

        builder.HasIndex(n => n.CorrelationId)
            .HasFilter("[CorrelationId] IS NOT NULL")
            .HasDatabaseName("IX_Notifications_CorrelationId");

        builder.HasIndex(n => n.Type)
            .HasDatabaseName("IX_Notifications_Type");

        // Composite index for retry processing
        builder.HasIndex(n => new { n.Status, n.RetryCount })
            .HasDatabaseName("IX_Notifications_Status_RetryCount");
    }
}
