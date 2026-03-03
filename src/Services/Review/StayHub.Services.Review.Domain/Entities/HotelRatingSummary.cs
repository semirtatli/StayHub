using StayHub.Services.Review.Domain.Events;
using StayHub.Shared.Domain;

namespace StayHub.Services.Review.Domain.Entities;

/// <summary>
/// Read model / materialized view that caches aggregated rating data for a hotel.
///
/// Updated reactively when reviews are submitted, updated, or deleted
/// (via domain event handlers). This avoids expensive COUNT/AVG queries
/// on every hotel detail request.
///
/// Not an aggregate root — it's a denormalized projection maintained in the Review service.
/// Uses Entity base for Id + audit columns.
/// </summary>
public sealed class HotelRatingSummary : AggregateRoot
{
    /// <summary>The hotel this summary belongs to.</summary>
    public Guid HotelId { get; private init; }

    /// <summary>Total number of active (non-deleted) reviews.</summary>
    public int TotalReviews { get; private set; }

    /// <summary>Average overall rating across all reviews.</summary>
    public decimal AverageOverall { get; private set; }

    /// <summary>Average cleanliness score.</summary>
    public decimal AverageCleanliness { get; private set; }

    /// <summary>Average service score.</summary>
    public decimal AverageService { get; private set; }

    /// <summary>Average location score.</summary>
    public decimal AverageLocation { get; private set; }

    /// <summary>Average comfort score.</summary>
    public decimal AverageComfort { get; private set; }

    /// <summary>Average value-for-money score.</summary>
    public decimal AverageValueForMoney { get; private set; }

    // EF Core constructor
    private HotelRatingSummary() { }

    /// <summary>
    /// Creates a new rating summary for a hotel (initially empty).
    /// </summary>
    public static HotelRatingSummary Create(Guid hotelId)
    {
        return new HotelRatingSummary
        {
            HotelId = hotelId,
            TotalReviews = 0,
            AverageOverall = 0,
            AverageCleanliness = 0,
            AverageService = 0,
            AverageLocation = 0,
            AverageComfort = 0,
            AverageValueForMoney = 0
        };
    }

    /// <summary>
    /// Recalculates all averages from the provided totals.
    /// Called by the rating recalculation handler after review changes.
    /// </summary>
    public void Recalculate(
        int totalReviews,
        decimal avgOverall,
        decimal avgCleanliness,
        decimal avgService,
        decimal avgLocation,
        decimal avgComfort,
        decimal avgValueForMoney)
    {
        TotalReviews = totalReviews;
        AverageOverall = Math.Round(avgOverall, 1);
        AverageCleanliness = Math.Round(avgCleanliness, 1);
        AverageService = Math.Round(avgService, 1);
        AverageLocation = Math.Round(avgLocation, 1);
        AverageComfort = Math.Round(avgComfort, 1);
        AverageValueForMoney = Math.Round(avgValueForMoney, 1);

        RaiseDomainEvent(new HotelRatingRecalculatedEvent(
            HotelId, AverageOverall, TotalReviews));
    }
}
