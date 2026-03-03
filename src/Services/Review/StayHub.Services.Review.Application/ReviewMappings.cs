using StayHub.Services.Review.Application.DTOs;
using StayHub.Services.Review.Domain.Entities;

namespace StayHub.Services.Review.Application;

/// <summary>
/// Manual mapping extensions from domain entities to DTOs.
/// </summary>
public static class ReviewMappings
{
    public static ReviewDto ToDto(this ReviewEntity review)
    {
        return new ReviewDto(
            review.Id,
            review.HotelId,
            review.BookingId,
            review.UserId,
            review.GuestName,
            review.Title,
            review.Body,
            new RatingDto(
                review.Rating.Cleanliness,
                review.Rating.Service,
                review.Rating.Location,
                review.Rating.Comfort,
                review.Rating.ValueForMoney,
                review.Rating.Overall),
            review.StayedFrom,
            review.StayedTo,
            review.ManagementResponse,
            review.ManagementResponseAt,
            review.CreatedAt,
            review.LastModifiedAt);
    }

    public static ReviewSummaryDto ToSummaryDto(this ReviewEntity review)
    {
        return new ReviewSummaryDto(
            review.Id,
            review.GuestName,
            review.Title,
            review.Rating.Overall,
            review.StayedFrom,
            review.StayedTo,
            review.ManagementResponse,
            review.CreatedAt);
    }

    public static HotelRatingSummaryDto ToDto(this HotelRatingSummary summary)
    {
        return new HotelRatingSummaryDto(
            summary.HotelId,
            summary.TotalReviews,
            summary.AverageOverall,
            summary.AverageCleanliness,
            summary.AverageService,
            summary.AverageLocation,
            summary.AverageComfort,
            summary.AverageValueForMoney);
    }
}
