namespace StayHub.Shared.Infrastructure.Outbox;

/// <summary>
/// Represents a domain event persisted in the outbox table.
///
/// Written atomically with entity changes in SaveChangesAsync, solving the
/// dual-write problem: if the DB write succeeds, the event will eventually
/// be published (at-least-once delivery guarantee).
///
/// Flow:
/// 1. Handler mutates aggregate → domain events raised
/// 2. TransactionBehavior calls SaveChangesAsync
/// 3. BaseDbContext serializes domain events as OutboxMessages in the SAME transaction
/// 4. Background processor polls for unprocessed messages and publishes to message broker
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Unique identifier for the outbox message.</summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Fully qualified type name of the domain event (without version info).
    /// Used for deserialization when the background processor publishes.
    /// Example: "StayHub.Services.Booking.Domain.Events.BookingCreatedEvent, StayHub.Services.Booking.Domain"
    /// </summary>
    public string EventType { get; private set; } = null!;

    /// <summary>JSON-serialized event payload.</summary>
    public string Payload { get; private set; } = null!;

    /// <summary>When the outbox message was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// When the message was successfully published (UTC).
    /// Null means unprocessed — the background processor queries for these.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; private set; }

    /// <summary>Last error message if a publish attempt failed.</summary>
    public string? Error { get; private set; }

    /// <summary>Number of publish attempts. Capped at MaxRetryCount in the processor.</summary>
    public int RetryCount { get; private set; }

    // EF Core requires a parameterless constructor
    private OutboxMessage() { }

    /// <summary>
    /// Creates a new outbox message from a serialized domain event.
    /// Called by BaseDbContext during SaveChangesAsync.
    /// </summary>
    public static OutboxMessage Create(string eventType, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAtUtc = DateTime.UtcNow,
            ProcessedAtUtc = null,
            Error = null,
            RetryCount = 0
        };
    }

    /// <summary>Marks the message as successfully published to the message broker.</summary>
    public void MarkAsProcessed()
    {
        ProcessedAtUtc = DateTime.UtcNow;
        Error = null;
    }

    /// <summary>Records a failed publish attempt — increments retry counter.</summary>
    public void MarkAsFailed(string error)
    {
        RetryCount++;
        Error = error;
    }
}
