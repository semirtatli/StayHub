using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.CloseHotel;

public sealed class CloseHotelCommandValidator : AbstractValidator<CloseHotelCommand>
{
    public CloseHotelCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => x.Reason is not null)
            .WithMessage("Closure reason cannot exceed 1000 characters.");
    }
}
