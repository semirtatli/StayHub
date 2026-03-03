using StayHub.Services.Booking.Domain.Entities;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Booking.Domain.Repositories;

/// <summary>
/// Repository for the Booking aggregate root.
/// One repository per aggregate root following DDD.
/// </summary>
public interface IBookingRepository : IRepository<BookingEntity>
{
    /// <summary>
    /// Get a booking by confirmation number.
    /// </summary>
    Task<BookingEntity?> GetByConfirmationNumberAsync(
        string confirmationNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all bookings for a specific guest.
    /// </summary>
    Task<IReadOnlyList<BookingEntity>> GetByGuestUserIdAsync(
        string guestUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all bookings for a specific hotel.
    /// </summary>
    Task<IReadOnlyList<BookingEntity>> GetByHotelIdAsync(
        Guid hotelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get bookings for a hotel filtered by status.
    /// </summary>
    Task<IReadOnlyList<BookingEntity>> GetByHotelIdAndStatusAsync(
        Guid hotelId,
        BookingStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get bookings for a specific room within a date range.
    /// Used for availability conflict checks.
    /// </summary>
    Task<IReadOnlyList<BookingEntity>> GetByRoomAndDateRangeAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any active (non-cancelled, non-completed) booking exists
    /// for the given room overlapping the date range.
    /// </summary>
    Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default);
}
