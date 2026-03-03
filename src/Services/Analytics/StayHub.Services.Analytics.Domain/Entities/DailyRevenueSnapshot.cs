using StayHub.Shared.Domain;

namespace StayHub.Services.Analytics.Domain.Entities;

/// <summary>
/// Daily revenue projection per hotel. One row per (HotelId, Date).
/// Updated incrementally as booking/payment events arrive.
/// Supports time-series revenue dashboards and CSV export.
/// </summary>
public sealed class DailyRevenueSnapshot : Entity
{
    public Guid HotelId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public int BookingCount { get; private set; }
    public decimal AverageBookingValue { get; private set; }
    public int CancellationCount { get; private set; }
    public decimal RefundAmount { get; private set; }

    private DailyRevenueSnapshot() { }

    public static DailyRevenueSnapshot Create(Guid hotelId, DateOnly date)
    {
        return new DailyRevenueSnapshot
        {
            HotelId = hotelId,
            Date = date
        };
    }

    /// <summary>
    /// Record a confirmed booking's revenue contribution.
    /// </summary>
    public void AddBooking(decimal amount)
    {
        TotalRevenue += amount;
        BookingCount++;
        RecalculateAverage();
    }

    /// <summary>
    /// Record a booking cancellation (does not change revenue until refund).
    /// </summary>
    public void AddCancellation()
    {
        CancellationCount++;
    }

    /// <summary>
    /// Record a refund — reduces total revenue and recalculates average.
    /// </summary>
    public void AddRefund(decimal amount)
    {
        RefundAmount += amount;
        TotalRevenue -= amount;
        RecalculateAverage();
    }

    private void RecalculateAverage()
    {
        AverageBookingValue = BookingCount > 0
            ? TotalRevenue / BookingCount
            : 0;
    }
}
