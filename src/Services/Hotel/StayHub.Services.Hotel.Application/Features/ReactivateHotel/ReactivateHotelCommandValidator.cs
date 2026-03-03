using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.ReactivateHotel;

public sealed class ReactivateHotelCommandValidator : AbstractValidator<ReactivateHotelCommand>
{
    public ReactivateHotelCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("AdminUserId is required.");
    }
}
