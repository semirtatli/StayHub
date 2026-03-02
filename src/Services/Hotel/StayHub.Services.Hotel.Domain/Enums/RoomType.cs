namespace StayHub.Services.Hotel.Domain.Enums;

/// <summary>
/// Accommodation room types.
/// Affects pricing, search filters, and capacity constraints.
/// </summary>
public enum RoomType
{
    /// <summary>Single bed room for one guest.</summary>
    Single = 0,

    /// <summary>Double bed room for two guests.</summary>
    Double = 1,

    /// <summary>Two separate beds for two guests.</summary>
    Twin = 2,

    /// <summary>Luxury suite with separate living area.</summary>
    Suite = 3,

    /// <summary>Premium suite — top floor, best amenities.</summary>
    Deluxe = 4,

    /// <summary>Family room — extra beds, child-friendly.</summary>
    Family = 5,

    /// <summary>Shared dormitory room (hostels).</summary>
    Dormitory = 6,

    /// <summary>Studio apartment-style room with kitchenette.</summary>
    Studio = 7,

    /// <summary>Entire penthouse floor.</summary>
    Penthouse = 8
}
