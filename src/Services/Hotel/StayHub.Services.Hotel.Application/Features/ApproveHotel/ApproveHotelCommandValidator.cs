using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.ApproveHotel;

public sealed class ApproveHotelCommandValidator : AbstractValidator<ApproveHotelCommand>
{
    public ApproveHotelCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("AdminUserId is required.");
    }
}
