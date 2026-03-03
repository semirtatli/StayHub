using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.BlockDates;

public sealed class BlockDatesCommandValidator : AbstractValidator<BlockDatesCommand>
{
    public BlockDatesCommandValidator()
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

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null)
            .WithMessage("Block reason cannot exceed 500 characters.");
    }
}
