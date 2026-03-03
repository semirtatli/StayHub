using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.CheckAvailability;

public sealed class CheckAvailabilityQueryValidator : AbstractValidator<CheckAvailabilityQuery>
{
    public CheckAvailabilityQueryValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.CheckIn)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn)
            .WithMessage("Check-out date must be after check-in date.");

        RuleFor(x => x)
            .Must(x => (x.CheckOut.DayNumber - x.CheckIn.DayNumber) <= 30)
            .WithMessage("Cannot check availability for more than 30 nights.");
    }
}
