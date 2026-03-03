using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.MarkNoShow;

/// <summary>
/// Handles marking a booking as no-show — Confirmed → NoShow.
///
/// The domain entity enforces that only Confirmed bookings can be marked as no-show.
/// Raises BookingStatusChangedEvent for downstream consumers (fee processing, analytics).
/// TransactionBehavior commits the unit of work after a successful result.
/// </summary>
public sealed class MarkNoShowCommandHandler : ICommandHandler<MarkNoShowCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<MarkNoShowCommandHandler> _logger;

    public MarkNoShowCommandHandler(
        IBookingRepository bookingRepository,
        ILogger<MarkNoShowCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        MarkNoShowCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(
            request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure(BookingErrors.Booking.NotFound);

        try
        {
            booking.MarkNoShow();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(BookingErrors.Booking.InvalidStatusTransition);
        }

        _bookingRepository.Update(booking);

        _logger.LogInformation(
            "Booking {BookingId} marked as no-show by user {UserId}",
            booking.Id, request.UserId);

        return Result.Success();
    }
}
