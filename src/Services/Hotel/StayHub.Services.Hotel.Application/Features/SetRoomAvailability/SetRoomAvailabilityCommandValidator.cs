using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.SetRoomAvailability;

public sealed class SetRoomAvailabilityCommandValidator : AbstractValidator<SetRoomAvailabilityCommand>
{
    public SetRoomAvailabilityCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");

        RuleFor(x => x.FromDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("FromDate cannot be in the past.");

        RuleFor(x => x.ToDate)
            .GreaterThan(x => x.FromDate)
            .WithMessage("ToDate must be after FromDate.");

        RuleFor(x => x)
            .Must(x => (x.ToDate.DayNumber - x.FromDate.DayNumber) <= 365)
            .WithMessage("Cannot set availability for more than 365 days at once.");

        RuleFor(x => x.TotalInventory)
            .GreaterThanOrEqualTo(0)
            .WithMessage("TotalInventory must be >= 0.");

        RuleFor(x => x.PriceOverride)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceOverride.HasValue)
            .WithMessage("PriceOverride must be >= 0.");
    }
}
