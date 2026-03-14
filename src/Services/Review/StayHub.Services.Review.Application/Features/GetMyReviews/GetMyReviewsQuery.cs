using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.GetMyReviews;

public sealed record GetMyReviewsQuery(string UserId) : IQuery<IReadOnlyList<ReviewDto>>;
