using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StayHub.Shared.Infrastructure.Outbox;

/// <summary>
/// EF Core configuration for the OutboxMessage entity.
/// Creates the OutboxMessages table in each service's database
/// (applied automatically by BaseDbContext.OnModelCreating).
///
/// Includes a filtered index on ProcessedAtUtc IS NULL for efficient polling
/// by the background processor — only unprocessed rows are scanned.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.Error)
            .HasMaxLength(2000);

        builder.Property(x => x.RetryCount)
            .HasDefaultValue(0);

        // Filtered index: only unprocessed messages are scanned during polling.
        // SQL: WHERE [ProcessedAtUtc] IS NULL ORDER BY [CreatedAtUtc]
        builder.HasIndex(x => x.ProcessedAtUtc)
            .HasFilter("[ProcessedAtUtc] IS NULL")
            .HasDatabaseName("IX_OutboxMessages_Unprocessed");

        // Ordering index for the polling query
        builder.HasIndex(x => x.CreatedAtUtc)
            .HasDatabaseName("IX_OutboxMessages_CreatedAtUtc");
    }
}
