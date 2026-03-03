using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.GetReviewById;

/// <summary>
/// Query to get a review by its ID.
/// </summary>
public sealed record GetReviewByIdQuery(Guid ReviewId) : IQuery<ReviewDto>;
