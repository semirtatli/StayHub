using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.DeleteReview;

/// <summary>
/// Command to soft-delete a review.
/// Only the review author or admin can delete.
/// </summary>
public sealed record DeleteReviewCommand(
    Guid ReviewId,
    string UserId) : ICommand;
