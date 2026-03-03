using StayHub.Services.Booking.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.GetBookingById;

/// <summary>
/// Query to fetch a single booking by ID with full details.
///
/// Access: The requesting user must be either the guest who made the booking
/// or a hotel owner/admin (verified in the handler).
/// </summary>
public sealed record GetBookingByIdQuery(Guid BookingId, string UserId) : IQuery<BookingDto>;
