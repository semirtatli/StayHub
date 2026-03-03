namespace StayHub.Services.Booking.Application.Abstractions;

/// <summary>
/// Anti-corruption layer for calling the Hotel Service.
///
/// The Booking bounded context owns its own DTOs (defined below) to avoid
/// coupling to Hotel.Application types. The infrastructure implementation
/// maps the HTTP responses to these local types.
///
/// Used by CreateBookingCommandHandler to validate hotel/room existence
/// and check real-time availability before creating a reservation.
/// </summary>
public interface IHotelServiceClient
{
    /// <summary>
    /// Get hotel detail including rooms.
    /// Returns null when the hotel does not exist.
    /// </summary>
    Task<HotelDetailResponse?> GetHotelDetailAsync(
        Guid hotelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check room availability for a hotel within a date range.
    /// Returns null when the hotel does not exist.
    /// </summary>
    Task<HotelAvailabilityResponse?> CheckAvailabilityAsync(
        Guid hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default);
}

// ── Local response DTOs (anti-corruption layer) ──────────────────────────

/// <summary>
/// Hotel detail as seen by the Booking bounded context.
/// </summary>
public sealed record HotelDetailResponse(
    Guid Id,
    string Name,
    string Status,
    string OwnerId,
    IReadOnlyList<RoomResponse> Rooms);

/// <summary>
/// Room detail as seen by the Booking bounded context.
/// </summary>
public sealed record RoomResponse(
    Guid Id,
    string Name,
    string RoomType,
    int MaxOccupancy,
    decimal BasePrice,
    string Currency,
    bool IsActive);

/// <summary>
/// Hotel availability response as seen by the Booking bounded context.
/// </summary>
public sealed record HotelAvailabilityResponse(
    Guid HotelId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    IReadOnlyList<RoomAvailabilityResponse> Rooms);

/// <summary>
/// Per-room availability within the requested date range.
/// </summary>
public sealed record RoomAvailabilityResponse(
    Guid RoomId,
    string RoomName,
    int MaxOccupancy,
    bool IsAvailable,
    decimal TotalPrice,
    string Currency);
