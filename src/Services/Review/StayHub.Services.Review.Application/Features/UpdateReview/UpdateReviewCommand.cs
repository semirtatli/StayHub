using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.UpdateReview;

/// <summary>
/// Command to update an existing review.
/// Only the review author can update. UserId set by controller from JWT.
/// </summary>
public sealed record UpdateReviewCommand(
    Guid ReviewId,
    string Title,
    string Body,
    int Cleanliness,
    int Service,
    int Location,
    int Comfort,
    int ValueForMoney,
    string UserId) : ICommand<ReviewDto>;
