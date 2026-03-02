using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Geographic coordinates (latitude/longitude) for hotel location.
/// Used for map display and distance-based search.
/// </summary>
public sealed class GeoLocation : ValueObject
{
    /// <summary>
    /// Latitude in decimal degrees. Range: -90 to +90.
    /// </summary>
    public double Latitude { get; private init; }

    /// <summary>
    /// Longitude in decimal degrees. Range: -180 to +180.
    /// </summary>
    public double Longitude { get; private init; }

    private GeoLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        return new GeoLocation(latitude, longitude);
    }

    /// <summary>
    /// Calculates the Haversine distance (in kilometers) to another location.
    /// Good enough for search radius filtering; not geodetic-survey precise.
    /// </summary>
    public double DistanceToKm(GeoLocation other)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    // EF Core parameterless constructor
    private GeoLocation() { }
}
