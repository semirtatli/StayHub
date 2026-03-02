namespace StayHub.Shared.Interfaces;

/// <summary>
/// Unit of Work pattern — coordinates saving all changes in a single transaction.
/// Each service's DbContext implements this.
/// Command handlers call SaveChangesAsync at the end of their work.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
