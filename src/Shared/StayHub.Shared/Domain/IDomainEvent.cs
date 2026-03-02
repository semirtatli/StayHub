using MediatR;

namespace StayHub.Shared.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened within a bounded context.
/// They are dispatched after the aggregate root is persisted (via SaveChanges interceptor).
/// Implements INotification so MediatR can dispatch them to multiple handlers.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for domain events. Using record for immutability and value equality.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
