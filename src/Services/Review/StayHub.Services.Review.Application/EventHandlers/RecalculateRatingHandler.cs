using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Services.Review.Domain.Events;
using StayHub.Services.Review.Domain.Repositories;

namespace StayHub.Services.Review.Application.EventHandlers;

/// <summary>
/// Recalculates the HotelRatingSummary whenever a review is submitted, updated, or deleted.
/// Listens to ReviewSubmittedEvent, ReviewUpdatedEvent, and ReviewDeletedEvent.
///
/// Creates the summary row if it doesn't exist (first review for a hotel).
/// Queries all active reviews for the hotel and recalculates averages.
/// </summary>
public sealed class RecalculateRatingHandler :
    INotificationHandler<ReviewSubmittedEvent>,
    INotificationHandler<ReviewUpdatedEvent>,
    INotificationHandler<ReviewDeletedEvent>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<RecalculateRatingHandler> _logger;

    public RecalculateRatingHandler(
        IReviewRepository reviewRepository,
        ILogger<RecalculateRatingHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public Task Handle(ReviewSubmittedEvent notification, CancellationToken cancellationToken)
        => RecalculateAsync(notification.HotelId, cancellationToken);

    public Task Handle(ReviewUpdatedEvent notification, CancellationToken cancellationToken)
        => RecalculateAsync(notification.HotelId, cancellationToken);

    public Task Handle(ReviewDeletedEvent notification, CancellationToken cancellationToken)
        => RecalculateAsync(notification.HotelId, cancellationToken);

    private async Task RecalculateAsync(Guid hotelId, CancellationToken cancellationToken)
    {
        // Get all active reviews for the hotel
        var reviews = await _reviewRepository.GetByHotelIdAsync(hotelId, cancellationToken);

        // Get or create the summary
        var summary = await _reviewRepository.GetRatingSummaryByHotelIdAsync(
            hotelId, cancellationToken);

        if (summary is null)
        {
            summary = HotelRatingSummary.Create(hotelId);
            _reviewRepository.AddRatingSummary(summary);
        }

        if (reviews.Count == 0)
        {
            summary.Recalculate(0, 0, 0, 0, 0, 0, 0);
        }
        else
        {
            summary.Recalculate(
                totalReviews: reviews.Count,
                avgOverall: reviews.Average(r => r.Rating.Overall),
                avgCleanliness: reviews.Average(r => (decimal)r.Rating.Cleanliness),
                avgService: reviews.Average(r => (decimal)r.Rating.Service),
                avgLocation: reviews.Average(r => (decimal)r.Rating.Location),
                avgComfort: reviews.Average(r => (decimal)r.Rating.Comfort),
                avgValueForMoney: reviews.Average(r => (decimal)r.Rating.ValueForMoney));
        }

        _reviewRepository.UpdateRatingSummary(summary);

        _logger.LogInformation(
            "Hotel {HotelId} rating recalculated — Average: {Average}, Reviews: {Count}",
            hotelId, summary.AverageOverall, summary.TotalReviews);
    }
}
