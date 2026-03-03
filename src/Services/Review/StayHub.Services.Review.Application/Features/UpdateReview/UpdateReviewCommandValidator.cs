using FluentValidation;

namespace StayHub.Services.Review.Application.Features.UpdateReview;

/// <summary>
/// Validates UpdateReviewCommand before it reaches the handler.
/// </summary>
public sealed class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Review title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Review body is required.")
            .MinimumLength(10).WithMessage("Body must be at least 10 characters.")
            .MaximumLength(5000).WithMessage("Body must not exceed 5000 characters.");

        RuleFor(x => x.Cleanliness)
            .InclusiveBetween(1, 5).WithMessage("Cleanliness rating must be between 1 and 5.");

        RuleFor(x => x.Service)
            .InclusiveBetween(1, 5).WithMessage("Service rating must be between 1 and 5.");

        RuleFor(x => x.Location)
            .InclusiveBetween(1, 5).WithMessage("Location rating must be between 1 and 5.");

        RuleFor(x => x.Comfort)
            .InclusiveBetween(1, 5).WithMessage("Comfort rating must be between 1 and 5.");

        RuleFor(x => x.ValueForMoney)
            .InclusiveBetween(1, 5).WithMessage("Value for money rating must be between 1 and 5.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
