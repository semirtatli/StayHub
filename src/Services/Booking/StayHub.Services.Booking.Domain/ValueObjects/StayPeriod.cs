namespace StayHub.Services.Booking.Domain.ValueObjects;

/// <summary>
/// Represents the date range of a hotel stay.
/// Check-in and check-out are dates only (no time component).
///
/// Invariants:
/// - CheckIn must be before CheckOut.
/// - Minimum stay is 1 night.
/// </summary>
public sealed record StayPeriod
{
    public DateOnly CheckIn { get; }
    public DateOnly CheckOut { get; }

    /// <summary>
    /// Number of nights in the stay.
    /// </summary>
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    private StayPeriod(DateOnly checkIn, DateOnly checkOut)
    {
        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    /// <summary>
    /// Create a new StayPeriod with validation.
    /// </summary>
    public static StayPeriod Create(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkIn >= checkOut)
            throw new ArgumentException("Check-in date must be before check-out date.");

        return new StayPeriod(checkIn, checkOut);
    }

    /// <summary>
    /// Returns all dates in the stay (check-in to day before check-out).
    /// Used for per-night availability checks and pricing.
    /// </summary>
    public IEnumerable<DateOnly> GetNights()
    {
        var current = CheckIn;
        while (current < CheckOut)
        {
            yield return current;
            current = current.AddDays(1);
        }
    }

    /// <summary>
    /// Check if two stay periods overlap.
    /// </summary>
    public bool Overlaps(StayPeriod other)
    {
        return CheckIn < other.CheckOut && other.CheckIn < CheckOut;
    }

    /// <summary>
    /// Check if this stay period contains a specific date.
    /// </summary>
    public bool Contains(DateOnly date)
    {
        return date >= CheckIn && date < CheckOut;
    }

    // EF Core requires a parameterless constructor for owned entities
    private StayPeriod() { }
}
