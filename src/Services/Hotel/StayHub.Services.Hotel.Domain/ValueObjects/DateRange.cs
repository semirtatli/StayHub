using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Date range value object — represents a check-in / check-out date pair.
/// Used by availability and booking calculations.
///
/// Invariants:
/// - CheckIn must be before CheckOut (at least 1 night).
/// - Both dates are date-only (no time component — DateOnly in .NET).
/// - CheckIn cannot be in the past (not enforced here — domain services validate
///   against "current date" to avoid coupling to wall clock).
/// </summary>
public sealed class DateRange : ValueObject
{
    /// <summary>
    /// Start of the stay (check-in day).
    /// </summary>
    public DateOnly CheckIn { get; private init; }

    /// <summary>
    /// End of the stay (check-out day — guest departs, room freed).
    /// </summary>
    public DateOnly CheckOut { get; private init; }

    private DateRange(DateOnly checkIn, DateOnly checkOut)
    {
        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    /// <summary>
    /// Creates a validated date range. CheckIn must be strictly before CheckOut.
    /// </summary>
    public static DateRange Create(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkIn >= checkOut)
            throw new ArgumentException(
                "Check-in date must be before check-out date.", nameof(checkIn));

        return new DateRange(checkIn, checkOut);
    }

    /// <summary>
    /// Number of nights in this date range.
    /// </summary>
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    /// <summary>
    /// Enumerates every date in the range (check-in inclusive, check-out exclusive).
    /// Each date represents a "night" — the guest occupies the room on this date.
    /// </summary>
    public IEnumerable<DateOnly> GetDates()
    {
        var current = CheckIn;
        while (current < CheckOut)
        {
            yield return current;
            current = current.AddDays(1);
        }
    }

    /// <summary>
    /// Whether this date range overlaps with another.
    /// Two ranges [A..B) and [C..D) overlap when A &lt; D AND C &lt; B.
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return CheckIn < other.CheckOut && other.CheckIn < CheckOut;
    }

    /// <summary>
    /// Whether this date range fully contains another.
    /// </summary>
    public bool Contains(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return CheckIn <= other.CheckIn && CheckOut >= other.CheckOut;
    }

    /// <summary>
    /// Whether a specific date falls within this range (check-in inclusive, check-out exclusive).
    /// </summary>
    public bool Contains(DateOnly date) =>
        date >= CheckIn && date < CheckOut;

    public override string ToString() => $"{CheckIn:yyyy-MM-dd} → {CheckOut:yyyy-MM-dd} ({Nights}N)";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CheckIn;
        yield return CheckOut;
    }

    // EF Core parameterless constructor
    private DateRange() { }
}
