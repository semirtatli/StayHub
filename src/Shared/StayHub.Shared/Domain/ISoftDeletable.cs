namespace StayHub.Shared.Domain;

/// <summary>
/// Interface for entities that support soft deletion.
/// When deleted, records are flagged rather than physically removed.
/// EF Core global query filter automatically excludes soft-deleted records.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}
