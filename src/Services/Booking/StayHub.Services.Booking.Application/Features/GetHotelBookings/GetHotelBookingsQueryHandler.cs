using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.GetHotelBookings;

/// <summary>
/// Returns all bookings for a hotel, optionally filtered by status.
/// Used by hotel owners to view their hotel's bookings dashboard.
///
/// If a valid status string is provided, uses the repository's optimized
/// GetByHotelIdAndStatusAsync query. Otherwise returns all hotel bookings.
/// </summary>
public sealed class GetHotelBookingsQueryHandler
    : IQueryHandler<GetHotelBookingsQuery, IReadOnlyList<BookingSummaryDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetHotelBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<BookingSummaryDto>>> Handle(
        GetHotelBookingsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.BookingEntity> bookings;

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<BookingStatus>(request.Status, ignoreCase: true, out var statusFilter))
        {
            bookings = await _bookingRepository.GetByHotelIdAndStatusAsync(
                request.HotelId, statusFilter, cancellationToken);
        }
        else
        {
            bookings = await _bookingRepository.GetByHotelIdAsync(
                request.HotelId, cancellationToken);
        }

        var dtos = bookings.Select(b => b.ToSummaryDto()).ToList();

        return dtos.AsReadOnly();
    }
}
