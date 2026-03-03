using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.GetBookingById;

/// <summary>
/// Returns full booking details for a single booking.
/// The user must be the guest who made the booking — ownership check is
/// performed here rather than in a policy so we can return a clear error.
/// </summary>
public sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<GetBookingByIdQueryHandler> _logger;

    public GetBookingByIdQueryHandler(
        IBookingRepository bookingRepository,
        ILogger<GetBookingByIdQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(
        GetBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.NotFound);
        }

        // Only the guest can view their own booking details via this query
        if (booking.GuestUserId != request.UserId)
            return Result.Failure<BookingDto>(BookingErrors.Booking.NotGuest);

        return booking.ToDto();
    }
}
