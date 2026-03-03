using StayHub.Shared.Domain;

namespace StayHub.Services.Analytics.Domain.Entities;

/// <summary>
/// Daily occupancy projection per hotel. One row per (HotelId, Date).
/// Tracks room utilisation for occupancy rate dashboards.
/// </summary>
public sealed class OccupancySnapshot : Entity
{
    public Guid HotelId { get; private set; }
    public DateOnly Date { get; private set; }
    public int TotalRooms { get; private set; }
    public int BookedRooms { get; private set; }
    public decimal OccupancyRate { get; private set; }

    private OccupancySnapshot() { }

    public static OccupancySnapshot Create(Guid hotelId, DateOnly date, int totalRooms)
    {
        return new OccupancySnapshot
        {
            HotelId = hotelId,
            Date = date,
            TotalRooms = totalRooms
        };
    }

    /// <summary>
    /// Record rooms booked by a new confirmed booking.
    /// </summary>
    public void RecordBooking(int rooms)
    {
        BookedRooms += rooms;
        RecalculateRate();
    }

    /// <summary>
    /// Release rooms when a booking is cancelled.
    /// </summary>
    public void CancelBooking(int rooms)
    {
        BookedRooms = Math.Max(0, BookedRooms - rooms);
        RecalculateRate();
    }

    private void RecalculateRate()
    {
        OccupancyRate = TotalRooms > 0
            ? (decimal)BookedRooms / TotalRooms * 100m
            : 0;
    }
}
