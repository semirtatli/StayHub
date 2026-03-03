using FluentValidation;

namespace StayHub.Services.Booking.Application.Features.CreateBooking;

/// <summary>
/// Validates CreateBookingCommand before it reaches the handler.
/// Runs automatically in the ValidationBehavior pipeline.
///
/// Rules:
/// - HotelId / RoomId: must be non-empty GUIDs
/// - CheckIn: required, must not be in the past
/// - CheckOut: required, must be after CheckIn
/// - NumberOfGuests: at least 1, max 20
/// - Guest info: FirstName, LastName, Email required
/// - Email: must be valid format
/// - GuestUserId: required (set by controller from JWT)
/// </summary>
public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required.");

        RuleFor(x => x.CheckIn)
            .NotEmpty().WithMessage("Check-in date is required.")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOut)
            .NotEmpty().WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out date must be after the check-in date.");

        RuleFor(x => x.NumberOfGuests)
            .InclusiveBetween(1, 20)
            .WithMessage("Number of guests must be between 1 and 20.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Guest first name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Guest last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Guest email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(2000).WithMessage("Special requests must not exceed 2000 characters.")
            .When(x => x.SpecialRequests is not null);

        RuleFor(x => x.GuestUserId)
            .NotEmpty().WithMessage("Guest user ID is required.");
    }
}
