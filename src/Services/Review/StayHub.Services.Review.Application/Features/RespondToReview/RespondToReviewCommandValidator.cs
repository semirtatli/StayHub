using FluentValidation;

namespace StayHub.Services.Review.Application.Features.RespondToReview;

/// <summary>
/// Validates RespondToReviewCommand.
/// </summary>
public sealed class RespondToReviewCommandValidator : AbstractValidator<RespondToReviewCommand>
{
    public RespondToReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required.");

        RuleFor(x => x.Response)
            .NotEmpty().WithMessage("Response text is required.")
            .MaximumLength(2000).WithMessage("Response must not exceed 2000 characters.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
