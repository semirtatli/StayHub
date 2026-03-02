using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Physical address of a hotel.
/// Value object — equality based on all address components.
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; private init; }
    public string City { get; private init; }
    public string State { get; private init; }
    public string Country { get; private init; }
    public string ZipCode { get; private init; }

    private Address(string street, string city, string state, string country, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string city, string state, string country, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        return new Address(street.Trim(), city.Trim(), state.Trim(), country.Trim(), zipCode.Trim());
    }

    /// <summary>
    /// Human-readable formatted address.
    /// </summary>
    public string FullAddress =>
        string.IsNullOrWhiteSpace(State)
            ? $"{Street}, {City}, {ZipCode}, {Country}"
            : $"{Street}, {City}, {State} {ZipCode}, {Country}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }

    // EF Core parameterless constructor
#pragma warning disable CS8618
    private Address() { }
#pragma warning restore CS8618
}
