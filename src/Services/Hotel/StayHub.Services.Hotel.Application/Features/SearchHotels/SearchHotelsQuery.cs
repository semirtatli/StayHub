using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Pagination;

namespace StayHub.Services.Hotel.Application.Features.SearchHotels;

/// <summary>
/// Public hotel search query — accessible without authentication.
///
/// Supports:
/// - Free-text search on name and description
/// - Filters: city, country, star-rating range, price range, room type
/// - Geo-distance: bounding-box pre-filter in SQL + Haversine post-calculation
/// - Sorting: name, starRating, price, createdAt (asc/desc)
/// - Pagination: page/pageSize
///
/// Only Active hotels are returned (enforced in the Specification).
/// </summary>
public sealed record SearchHotelsQuery(
    string? SearchTerm,
    string? City,
    string? Country,
    int? MinStarRating,
    int? MaxStarRating,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? RoomType,
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    string? SortBy,
    bool SortDescending,
    int Page,
    int PageSize) : IQuery<PagedList<HotelSearchResultDto>>;
