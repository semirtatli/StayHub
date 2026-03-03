using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.CompleteBooking;

/// <summary>
/// Handles booking completion (guest checkout) — CheckedIn → Completed.
///
/// The domain entity enforces that only CheckedIn bookings can be completed.
/// Raises BookingStatusChangedEvent + BookingCompletedEvent for downstream consumers.
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class CompleteBookingCommandHandler : ICommandHandler<CompleteBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<CompleteBookingCommandHandler> _logger;

    public CompleteBookingCommandHandler(
        IBookingRepository bookingRepository,
        ILogger<CompleteBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CompleteBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.Booking.NotFound);

        try
        {
            booking.Complete();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(BookingErrors.Booking.InvalidStatusTransition);
        }

        _bookingRepository.Update(booking);

        _logger.LogInformation(
            "Booking {BookingId} completed (guest checkout) by user {UserId}",
            booking.Id, request.UserId);

        return Result.Success();
    }
}
