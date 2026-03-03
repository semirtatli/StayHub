using StayHub.Services.Hotel.Domain.Entities;

namespace StayHub.Services.Hotel.Domain.Repositories;

/// <summary>
/// Repository for RoomAvailability queries and bulk operations.
///
/// RoomAvailability is not an aggregate root, but we provide a dedicated
/// repository because availability queries are performance-critical and
/// span date ranges — generic CRUD via IRepository is insufficient.
/// </summary>
public interface IRoomAvailabilityRepository
{
    /// <summary>
    /// Get availability records for a room within a date range.
    /// Returns one record per date (may have gaps if not initialized).
    /// </summary>
    Task<IReadOnlyList<RoomAvailability>> GetByRoomAndDateRangeAsync(
        Guid roomId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get availability records for multiple rooms within a date range.
    /// Used for checking hotel-level availability across all room types.
    /// </summary>
    Task<IReadOnlyList<RoomAvailability>> GetByRoomsAndDateRangeAsync(
        IEnumerable<Guid> roomIds,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a room has availability for every date in a range.
    /// Returns true only if all dates exist, are not blocked, and have AvailableCount > 0.
    /// </summary>
    Task<bool> IsAvailableAsync(
        Guid roomId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a single availability record.
    /// </summary>
    void Add(RoomAvailability availability);

    /// <summary>
    /// Add multiple availability records in bulk (batch insert for date-range initialization).
    /// </summary>
    void AddRange(IEnumerable<RoomAvailability> availabilities);

    /// <summary>
    /// Get a specific availability record by room and date.
    /// </summary>
    Task<RoomAvailability?> GetByRoomAndDateAsync(
        Guid roomId,
        DateOnly availabilityDate,
        CancellationToken cancellationToken = default);
}
