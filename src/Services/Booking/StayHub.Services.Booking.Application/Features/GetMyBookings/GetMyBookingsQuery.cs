using StayHub.Services.Booking.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.GetMyBookings;

/// <summary>
/// Query to fetch all bookings for the authenticated guest.
///
/// Returns BookingSummaryDto list (lightweight) sorted by creation date descending.
/// Optional status filter allows the guest to view only active/past/cancelled bookings.
/// </summary>
public sealed record GetMyBookingsQuery(
    string GuestUserId,
    string? Status = null) : IQuery<IReadOnlyList<BookingSummaryDto>>;
