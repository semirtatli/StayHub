using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Hotel.Domain.Repositories;

/// <summary>
/// Repository for the Hotel aggregate root.
/// Following the DDD repository pattern — one repository per aggregate root.
/// </summary>
public interface IHotelRepository : IRepository<HotelEntity>
{
    /// <summary>
    /// Get a hotel by ID including its rooms (for command operations that modify rooms).
    /// </summary>
    Task<HotelEntity?> GetByIdWithRoomsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all hotels owned by a specific user.
    /// </summary>
    Task<IReadOnlyList<HotelEntity>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a hotel with the given name already exists for this owner.
    /// Prevents duplicate hotel names per owner.
    /// </summary>
    Task<bool> ExistsByNameAndOwnerAsync(string name, string ownerId, CancellationToken cancellationToken = default);
}
