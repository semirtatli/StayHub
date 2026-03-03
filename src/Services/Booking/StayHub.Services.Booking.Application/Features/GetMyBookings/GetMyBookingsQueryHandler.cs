using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.GetMyBookings;

/// <summary>
/// Returns all bookings for a guest, optionally filtered by status.
/// Uses the repository's GetByGuestUserIdAsync which returns ordered by CreatedAt desc.
/// </summary>
public sealed class GetMyBookingsQueryHandler
    : IQueryHandler<GetMyBookingsQuery, IReadOnlyList<BookingSummaryDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<BookingSummaryDto>>> Handle(
        GetMyBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByGuestUserIdAsync(
            request.GuestUserId, cancellationToken);

        // Optional status filter
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<BookingStatus>(request.Status, ignoreCase: true, out var statusFilter))
        {
            bookings = bookings.Where(b => b.Status == statusFilter).ToList();
        }

        var dtos = bookings.Select(b => b.ToSummaryDto()).ToList();

        return dtos.AsReadOnly();
    }
}
