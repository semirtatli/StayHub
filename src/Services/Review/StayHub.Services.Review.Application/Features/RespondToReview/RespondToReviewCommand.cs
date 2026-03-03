using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.RespondToReview;

/// <summary>
/// Command for hotel management to respond to a guest review.
/// Only the hotel owner or admin can respond.
/// </summary>
public sealed record RespondToReviewCommand(
    Guid ReviewId,
    string Response,
    string UserId) : ICommand;
