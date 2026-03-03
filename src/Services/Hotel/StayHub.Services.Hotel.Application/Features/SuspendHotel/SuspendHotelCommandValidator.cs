using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.SuspendHotel;

public sealed class SuspendHotelCommandValidator : AbstractValidator<SuspendHotelCommand>
{
    public SuspendHotelCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => x.Reason is not null)
            .WithMessage("Suspension reason cannot exceed 1000 characters.");
    }
}
