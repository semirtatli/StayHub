using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.GetBookingByConfirmation;

/// <summary>
/// Looks up a booking by confirmation number and verifies guest ownership.
/// This endpoint is useful for guests to look up their booking via the
/// confirmation number received in their email.
/// </summary>
public sealed class GetBookingByConfirmationQueryHandler
    : IQueryHandler<GetBookingByConfirmationQuery, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<GetBookingByConfirmationQueryHandler> _logger;

    public GetBookingByConfirmationQueryHandler(
        IBookingRepository bookingRepository,
        ILogger<GetBookingByConfirmationQueryHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(
        GetBookingByConfirmationQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByConfirmationNumberAsync(
            request.ConfirmationNumber, cancellationToken);

        if (booking is null)
        {
            _logger.LogWarning(
                "Booking not found for confirmation number {ConfirmationNumber}",
                request.ConfirmationNumber);
            return Result.Failure<BookingDto>(BookingErrors.Booking.NotFound);
        }

        if (booking.GuestUserId != request.UserId)
            return Result.Failure<BookingDto>(BookingErrors.Booking.NotGuest);

        return booking.ToDto();
    }
}
