using Microsoft.EntityFrameworkCore;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;

namespace StayHub.Services.Hotel.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IRoomAvailabilityRepository.
/// Optimized for date-range queries with no-tracking for reads.
/// </summary>
public sealed class RoomAvailabilityRepository : IRoomAvailabilityRepository
{
    private readonly HotelDbContext _dbContext;

    public RoomAvailabilityRepository(HotelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoomAvailability>> GetByRoomAndDateRangeAsync(
        Guid roomId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomAvailability
            .Where(a => a.RoomId == roomId && a.Date >= fromDate && a.Date < toDate)
            .OrderBy(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoomAvailability>> GetByRoomsAndDateRangeAsync(
        IEnumerable<Guid> roomIds,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var roomIdList = roomIds.ToList();

        return await _dbContext.RoomAvailability
            .Where(a => roomIdList.Contains(a.RoomId) && a.Date >= fromDate && a.Date < toDate)
            .OrderBy(a => a.RoomId)
            .ThenBy(a => a.Date)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(
        Guid roomId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var requiredNights = toDate.DayNumber - fromDate.DayNumber;

        // Count dates that are available (not blocked, have inventory)
        var availableDates = await _dbContext.RoomAvailability
            .AsNoTracking()
            .CountAsync(
                a => a.RoomId == roomId
                    && a.Date >= fromDate
                    && a.Date < toDate
                    && !a.IsBlocked
                    && (a.TotalInventory - a.BookedCount) > 0,
                cancellationToken);

        // All dates in range must be available
        return availableDates == requiredNights;
    }

    /// <inheritdoc />
    public void Add(RoomAvailability availability)
    {
        _dbContext.RoomAvailability.Add(availability);
    }

    /// <inheritdoc />
    public void AddRange(IEnumerable<RoomAvailability> availabilities)
    {
        _dbContext.RoomAvailability.AddRange(availabilities);
    }

    /// <inheritdoc />
    public async Task<RoomAvailability?> GetByRoomAndDateAsync(
        Guid roomId,
        DateOnly availabilityDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoomAvailability
            .FirstOrDefaultAsync(
                a => a.RoomId == roomId && a.Date == availabilityDate,
                cancellationToken);
    }
}
