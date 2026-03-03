using FluentValidation;

namespace StayHub.Services.Booking.Application.Features.CancelBooking;

/// <summary>
/// Validates CancelBookingCommand before it reaches the handler.
/// Runs automatically in the ValidationBehavior pipeline.
///
/// Rules:
/// - BookingId: required
/// - CancellationReason: max 2000 characters if provided
/// - UserId: required (set by controller from JWT)
/// </summary>
public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required.");

        RuleFor(x => x.CancellationReason)
            .MaximumLength(2000).WithMessage("Cancellation reason must not exceed 2000 characters.")
            .When(x => x.CancellationReason is not null);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
