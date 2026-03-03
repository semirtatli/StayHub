using StayHub.Shared.Domain;

namespace StayHub.Services.Analytics.Domain.Entities;

/// <summary>
/// Aggregated KPI snapshot per hotel. Updated incrementally by event projectors.
/// Powers the "Top Hotels" ranking and per-hotel dashboard cards.
/// </summary>
public sealed class HotelPerformanceSummary : Entity
{
    public Guid HotelId { get; private set; }
    public string HotelName { get; private set; } = null!;
    public int TotalBookings { get; private set; }
    public int TotalCancellations { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public decimal AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public decimal CancellationRate { get; private set; }
    public decimal AverageOccupancyRate { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private HotelPerformanceSummary() { }

    public static HotelPerformanceSummary Create(Guid hotelId, string hotelName)
    {
        return new HotelPerformanceSummary
        {
            HotelId = hotelId,
            HotelName = hotelName,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void RecordBooking(decimal amount)
    {
        TotalBookings++;
        TotalRevenue += amount;
        RecalculateRates();
    }

    public void RecordCancellation()
    {
        TotalCancellations++;
        RecalculateRates();
    }

    public void RecordRefund(decimal amount)
    {
        TotalRevenue -= amount;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void RecordReview(decimal rating)
    {
        var totalRatingSum = AverageRating * TotalReviews;
        TotalReviews++;
        AverageRating = (totalRatingSum + rating) / TotalReviews;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOccupancyRate(decimal rate)
    {
        AverageOccupancyRate = rate;
        LastUpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateRates()
    {
        CancellationRate = TotalBookings > 0
            ? (decimal)TotalCancellations / TotalBookings * 100m
            : 0;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
