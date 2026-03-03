using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.RejectHotel;

public sealed class RejectHotelCommandValidator : AbstractValidator<RejectHotelCommand>
{
    public RejectHotelCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.AdminUserId)
            .NotEmpty().WithMessage("AdminUserId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.");
    }
}
