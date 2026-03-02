namespace StayHub.Shared.Domain;

/// <summary>
/// Base class for aggregate roots — the entry point to a consistency boundary.
/// Only aggregate roots can be saved via repositories.
/// Collects domain events that are dispatched after SaveChanges.
/// </summary>
public abstract class AggregateRoot : Entity, ISoftDeletable
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this aggregate, dispatched after persistence.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Soft delete columns
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Optimistic concurrency token — mapped to SQL Server rowversion
    public byte[] RowVersion { get; set; } = [];

    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() { }

    /// <summary>
    /// Raise a domain event to be dispatched after the aggregate is persisted.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clear all domain events after they have been dispatched.
    /// Called by the infrastructure layer after SaveChanges.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
