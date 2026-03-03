using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.SearchCriteria;
using StayHub.Shared.Interfaces;
using StayHub.Shared.Pagination;

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

    /// <summary>
    /// Full-text search with dynamic filtering, geo-distance bounding box,
    /// and pagination. Only returns Active hotels (public search).
    ///
    /// Infrastructure builds a Specification from the criteria and evaluates
    /// it against EF Core. Geo-distance uses a bounding-box approximation in SQL;
    /// the caller can compute exact Haversine distance after materialization.
    /// </summary>
    Task<PagedList<HotelEntity>> SearchAsync(
        HotelSearchCriteria criteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
