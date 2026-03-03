using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.GetHotelReviews;

/// <summary>
/// Query to get all reviews for a specific hotel.
/// </summary>
public sealed record GetHotelReviewsQuery(Guid HotelId) : IQuery<IReadOnlyList<ReviewSummaryDto>>;
