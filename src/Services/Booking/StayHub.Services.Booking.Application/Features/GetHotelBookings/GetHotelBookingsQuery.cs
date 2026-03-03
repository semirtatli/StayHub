using StayHub.Services.Booking.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.GetHotelBookings;

/// <summary>
/// Query to fetch all bookings for a specific hotel.
///
/// Access: Hotel owner or admin only (enforced by the controller policy).
/// Optional status filter for the hotel dashboard (e.g., show only Confirmed bookings).
/// </summary>
public sealed record GetHotelBookingsQuery(
    Guid HotelId,
    string? Status = null) : IQuery<IReadOnlyList<BookingSummaryDto>>;
