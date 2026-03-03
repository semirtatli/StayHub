using Microsoft.EntityFrameworkCore;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.SearchCriteria;
using StayHub.Services.Hotel.Infrastructure.Persistence.Specifications;
using StayHub.Shared.Infrastructure.Persistence;
using StayHub.Shared.Pagination;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IHotelRepository.
/// Inherits from SpecificationRepository for Specification-pattern support,
/// which in turn inherits generic CRUD from Repository&lt;T&gt;.
/// </summary>
public sealed class HotelRepository : SpecificationRepository<HotelEntity>, IHotelRepository
{
    public HotelRepository(HotelDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<HotelEntity?> GetByIdWithRoomsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HotelEntity>> GetByOwnerIdAsync(
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(h => h.OwnerId == ownerId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAndOwnerAsync(
        string name,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            h => h.Name == name && h.OwnerId == ownerId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedList<HotelEntity>> SearchAsync(
        HotelSearchCriteria criteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var specification = new HotelSearchSpecification(criteria);
        return await PagedListAsync(specification, page, pageSize, cancellationToken);
    }
}
