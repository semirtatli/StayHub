using StayHub.Shared.Domain;

namespace StayHub.Services.Review.Domain.ValueObjects;

/// <summary>
/// Value object representing a guest's detailed rating across multiple categories.
///
/// Each category is rated 1–5 (whole numbers). The Overall score is the average.
/// Categories:
/// - Cleanliness: Room and common area cleanliness
/// - Service: Staff friendliness, responsiveness
/// - Location: Proximity to attractions, transport
/// - Comfort: Bed quality, noise levels, room size
/// - ValueForMoney: Price vs quality perception
///
/// Invariants:
/// - All individual scores must be between 1 and 5.
/// - Overall is calculated, not supplied.
/// </summary>
public sealed class Rating : ValueObject
{
    public int Cleanliness { get; private init; }
    public int Service { get; private init; }
    public int Location { get; private init; }
    public int Comfort { get; private init; }
    public int ValueForMoney { get; private init; }

    /// <summary>
    /// Calculated average of all category scores, rounded to 1 decimal place.
    /// </summary>
    public decimal Overall { get; private init; }

    // EF Core constructor
    private Rating() { }

    public static Rating Create(
        int cleanliness,
        int service,
        int location,
        int comfort,
        int valueForMoney)
    {
        ValidateScore(cleanliness, nameof(cleanliness));
        ValidateScore(service, nameof(service));
        ValidateScore(location, nameof(location));
        ValidateScore(comfort, nameof(comfort));
        ValidateScore(valueForMoney, nameof(valueForMoney));

        var overall = Math.Round(
            (cleanliness + service + location + comfort + valueForMoney) / 5.0m, 1);

        return new Rating
        {
            Cleanliness = cleanliness,
            Service = service,
            Location = location,
            Comfort = comfort,
            ValueForMoney = valueForMoney,
            Overall = overall
        };
    }

    private static void ValidateScore(int score, string categoryName)
    {
        if (score is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                categoryName,
                $"{categoryName} rating must be between 1 and 5.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Cleanliness;
        yield return Service;
        yield return Location;
        yield return Comfort;
        yield return ValueForMoney;
    }
}
