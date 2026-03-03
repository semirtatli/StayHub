using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.CheckAvailability;

/// <summary>
/// Query to check room availability for a hotel within a date range.
/// Returns availability status for each room type — used by the search/booking flow.
///
/// Public endpoint — accessible without authentication.
/// </summary>
public sealed record CheckAvailabilityQuery(
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut) : IQuery<HotelAvailabilityDto>;

/// <summary>
/// Availability result for a hotel across a date range.
/// </summary>
public sealed record HotelAvailabilityDto(
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    IReadOnlyList<RoomAvailabilityDto> Rooms);

/// <summary>
/// Per-room-type availability breakdown.
/// </summary>
public sealed record RoomAvailabilityDto(
    Guid RoomId,
    string RoomName,
    string RoomType,
    int MaxOccupancy,
    int MinAvailable,
    bool IsAvailable,
    decimal TotalPrice,
    string Currency,
    IReadOnlyList<DateAvailabilityDto> Dates);

/// <summary>
/// Per-date availability detail for a room.
/// </summary>
public sealed record DateAvailabilityDto(
    DateOnly Date,
    int TotalInventory,
    int BookedCount,
    int AvailableCount,
    decimal Price,
    bool IsBlocked);
