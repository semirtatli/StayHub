using FluentValidation;

namespace StayHub.Services.Hotel.Application.Features.SubmitForApproval;

public sealed class SubmitForApprovalCommandValidator : AbstractValidator<SubmitForApprovalCommand>
{
    public SubmitForApprovalCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("HotelId is required.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
    }
}
