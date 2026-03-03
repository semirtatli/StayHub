using StayHub.Services.Hotel.Domain.Enums;

namespace StayHub.Services.Hotel.Application.DTOs;

/// <summary>
/// Hotel data transfer object for API responses.
/// Flattens the aggregate into a serializable shape.
/// </summary>
public sealed record HotelDto(
    Guid Id,
    string Name,
    string Description,
    int StarRating,
    AddressDto Address,
    GeoLocationDto? Location,
    ContactInfoDto ContactInfo,
    string OwnerId,
    string Status,
    string? StatusReason,
    string CheckInTime,
    string CheckOutTime,
    string? CoverImageUrl,
    IReadOnlyList<string> PhotoUrls,
    int RoomCount,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// Hotel summary for list/search results — excludes detailed fields for performance.
/// </summary>
 public sealed record HotelSummaryDto(
    Guid Id,
    string Name,
    int StarRating,
    string City,
    string Country,
    string Status,
    string? CoverImageUrl,
    int RoomCount,
    decimal? MinPrice,
    string? Currency,
    int PhotoCount,
    DateTime CreatedAt);

/// <summary>
/// Address DTO — flat representation of the Address value object.
/// </summary>
public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

/// <summary>
/// GeoLocation DTO.
/// </summary>
public sealed record GeoLocationDto(
    double Latitude,
    double Longitude);

/// <summary>
/// Contact info DTO.
/// </summary>
public sealed record ContactInfoDto(
    string Phone,
    string Email,
    string? Website);

/// <summary>
/// Room DTO for API responses.
/// </summary>
public sealed record RoomDto(
    Guid Id,
    Guid HotelId,
    string Name,
    string Description,
    string RoomType,
    int MaxOccupancy,
    decimal BasePrice,
    string Currency,
    int TotalInventory,
    decimal? SizeInSquareMeters,
    string? BedConfiguration,
    bool IsActive,
    IReadOnlyList<string> Amenities,
    IReadOnlyList<string> PhotoUrls);

/// <summary>
/// Search result DTO — extends summary with optional geo-distance.
/// Returned by the search engine for paginated public search results.
/// </summary>
public sealed record HotelSearchResultDto(
    Guid Id,
    string Name,
    string Description,
    int StarRating,
    string City,
    string Country,
    string Status,
    string? CoverImageUrl,
    int RoomCount,
    decimal? MinPrice,
    string? Currency,
    int PhotoCount,
    double? DistanceKm,
    DateTime CreatedAt);

/// <summary>
/// Detailed hotel DTO that includes rooms — used for single hotel detail view.
/// </summary>
public sealed record HotelDetailDto(
    Guid Id,
    string Name,
    string Description,
    int StarRating,
    AddressDto Address,
    GeoLocationDto? Location,
    ContactInfoDto ContactInfo,
    string OwnerId,
    string Status,
    string? StatusReason,
    string CheckInTime,
    string CheckOutTime,
    string? CoverImageUrl,
    IReadOnlyList<string> PhotoUrls,
    IReadOnlyList<RoomDto> Rooms,
    CancellationPolicyDto CancellationPolicy,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// Cancellation policy DTO — defines refund rules for a hotel.
/// </summary>
public sealed record CancellationPolicyDto(
    string PolicyType,
    int FreeCancellationDays,
    int PartialRefundPercentage,
    int PartialRefundDays);
