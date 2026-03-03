using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.ConfirmBooking;

/// <summary>
/// Handles booking confirmation — Pending → Confirmed.
///
/// The domain entity enforces that only Pending bookings can be confirmed.
/// Raises BookingStatusChangedEvent + BookingConfirmedEvent for downstream consumers.
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class ConfirmBookingCommandHandler : ICommandHandler<ConfirmBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    public ConfirmBookingCommandHandler(
        IBookingRepository bookingRepository,
        ILogger<ConfirmBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.Booking.NotFound);

        try
        {
            booking.Confirm();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(BookingErrors.Booking.InvalidStatusTransition);
        }

        _bookingRepository.Update(booking);

        _logger.LogInformation(
            "Booking {BookingId} confirmed — Confirmation: {ConfirmationNumber}",
            booking.Id, booking.ConfirmationNumber);

        return Result.Success();
    }
}
