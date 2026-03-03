using StayHub.Services.Hotel.Domain.Enums;

namespace StayHub.Services.Hotel.Domain.SearchCriteria;

/// <summary>
/// Encapsulates all search and filter parameters for hotel search.
/// Defined in the Domain layer because hotel search is a core domain concept
/// for an OTA marketplace.
///
/// Consumed by IHotelRepository.SearchAsync — the Infrastructure layer
/// translates these criteria into a Specification for EF Core.
/// </summary>
public sealed record HotelSearchCriteria
{
    /// <summary>
    /// Free-text search term — matched against hotel name and description.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filter by city (exact match, case-insensitive via SQL collation).
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Filter by country (exact match, case-insensitive via SQL collation).
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Minimum star rating (inclusive). Range: 1–5.
    /// </summary>
    public int? MinStarRating { get; init; }

    /// <summary>
    /// Maximum star rating (inclusive). Range: 1–5.
    /// </summary>
    public int? MaxStarRating { get; init; }

    /// <summary>
    /// Minimum room base price (inclusive). Hotels with at least one active room at this price or above.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Maximum room base price (inclusive). Hotels with at least one active room at this price or below.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Filter by room type. Hotels must have at least one active room of this type.
    /// </summary>
    public RoomType? RoomType { get; init; }

    /// <summary>
    /// Search center latitude (for geo-distance filtering).
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// Search center longitude (for geo-distance filtering).
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// Maximum distance in kilometers from the search center.
    /// Hotels outside this radius are excluded (bounding-box approximation in SQL,
    /// exact Haversine distance calculated after materialization).
    /// </summary>
    public double? RadiusKm { get; init; }

    /// <summary>
    /// Sort field: "name", "starRating", "price", "createdAt".
    /// Defaults to "createdAt" if null.
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction. True = descending, false = ascending.
    /// </summary>
    public bool SortDescending { get; init; }
}
