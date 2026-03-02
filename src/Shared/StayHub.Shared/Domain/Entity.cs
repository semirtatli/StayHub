namespace StayHub.Shared.Domain;

/// <summary>
/// Base class for all domain entities. Provides identity (Id) and audit columns.
/// Every entity in every service inherits from this.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected init; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() : this(Guid.NewGuid()) { }

    public override bool Equals(object? obj) =>
        obj is Entity other && Equals(other);

    public bool Equals(Entity? other) =>
        other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity? left, Entity? right) =>
        !(left == right);
}
