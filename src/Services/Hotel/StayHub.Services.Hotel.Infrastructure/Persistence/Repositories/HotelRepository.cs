using Microsoft.EntityFrameworkCore;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IHotelRepository.
/// Inherits generic CRUD from Repository&lt;T&gt; and adds Hotel-specific queries.
/// </summary>
public sealed class HotelRepository : Repository<HotelEntity>, IHotelRepository
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
}
