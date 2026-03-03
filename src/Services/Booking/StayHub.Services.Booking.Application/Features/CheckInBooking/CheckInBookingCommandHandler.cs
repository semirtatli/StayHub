using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.CheckInBooking;

/// <summary>
/// Handles guest check-in — Confirmed → CheckedIn.
///
/// The domain entity enforces that only Confirmed bookings can be checked in.
/// Raises BookingStatusChangedEvent + GuestCheckedInEvent for downstream consumers.
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class CheckInBookingCommandHandler : ICommandHandler<CheckInBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<CheckInBookingCommandHandler> _logger;

    public CheckInBookingCommandHandler(
        IBookingRepository bookingRepository,
        ILogger<CheckInBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CheckInBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.Booking.NotFound);

        try
        {
            booking.CheckIn();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(BookingErrors.Booking.InvalidStatusTransition);
        }

        _bookingRepository.Update(booking);

        _logger.LogInformation(
            "Guest checked in for booking {BookingId} by user {UserId}",
            booking.Id, request.UserId);

        return Result.Success();
    }
}
