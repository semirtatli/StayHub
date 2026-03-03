using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.SearchCriteria;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Pagination;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.SearchHotels;

/// <summary>
/// Handles the public hotel search query.
///
/// Flow:
/// 1. Map query parameters to <see cref="HotelSearchCriteria"/>
/// 2. Call repository.SearchAsync (Specification-based, paginated SQL)
/// 3. Map results to DTOs, computing exact Haversine distance if geo-search
/// </summary>
public sealed class SearchHotelsQueryHandler
    : IQueryHandler<SearchHotelsQuery, PagedList<HotelSearchResultDto>>
{
    private readonly IHotelRepository _hotelRepository;

    public SearchHotelsQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result<PagedList<HotelSearchResultDto>>> Handle(
        SearchHotelsQuery request,
        CancellationToken cancellationToken)
    {
        // ── Build search criteria ───────────────────────────────────────
        var criteria = new HotelSearchCriteria
        {
            SearchTerm = request.SearchTerm,
            City = request.City,
            Country = request.Country,
            MinStarRating = request.MinStarRating,
            MaxStarRating = request.MaxStarRating,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            RoomType = request.RoomType is not null
                ? Enum.Parse<RoomType>(request.RoomType, ignoreCase: true)
                : null,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusKm = request.RadiusKm,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        // ── Execute paginated search ────────────────────────────────────
        var pagedHotels = await _hotelRepository.SearchAsync(
            criteria,
            request.Page,
            request.PageSize,
            cancellationToken);

        // ── Map to DTOs with optional distance ──────────────────────────
        GeoLocation? searchLocation = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            searchLocation = GeoLocation.Create(
                request.Latitude.Value, request.Longitude.Value);
        }

        var result = pagedHotels.Map(
            hotel => HotelMappings.ToSearchResultDto(hotel, searchLocation));

        return result;
    }
}
