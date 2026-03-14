using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Application.Features.CreateHotel;
using StayHub.Services.Hotel.Application.Features.GetHotelById;
using StayHub.Services.Hotel.Application.Features.GetHotelsByOwner;
using StayHub.Services.Hotel.Application.Features.SearchHotels;
using StayHub.Services.Hotel.Application.Features.UpdateHotel;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.Pagination;

namespace StayHub.Services.Hotel.Api.Controllers;

/// <summary>
/// Hotel management controller — CRUD operations for hotel listings.
///
/// Authorization:
/// - POST /api/hotels → HotelOwnerOrAdmin (create hotel listing)
/// - PUT /api/hotels/{id} → HotelOwnerOrAdmin (update own hotel; ownership verified in handler)
/// - GET /api/hotels/{id} → AllowAnonymous (public detail view)
/// - GET /api/hotels/my → HotelOwnerOrAdmin (list own hotels)
///
/// OwnerId is always extracted from JWT claims, never from request body.
/// The handler performs additional ownership verification for write operations.
/// </summary>
[Route("api/hotels")]
public sealed class HotelsController : ApiController
{
    /// <summary>
    /// Create a new hotel listing. Starts in Draft status.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreateHotelCommand(
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.Phone,
            request.Email,
            request.Website,
            request.CheckInTime,
            request.CheckOutTime,
            request.Latitude,
            request.Longitude,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleCreatedResult(result, nameof(GetById), new { id = result.IsSuccess ? result.Value.Id : (Guid?)null });
    }

    /// <summary>
    /// Update an existing hotel's information.
    /// Only the owner can update their hotel. Only Draft or Active hotels can be updated.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHotelRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateHotelCommand(
            id,
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.Phone,
            request.Email,
            request.Website,
            request.CheckInTime,
            request.CheckOutTime,
            request.Latitude,
            request.Longitude,
            ownerId);

        var result = await Mediator.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Search active hotels with dynamic filtering, geo-distance, and pagination.
    /// Public endpoint — no authentication required.
    ///
    /// Supports:
    /// - Free-text search (?q=term) on name and description
    /// - Filters: city, country, starRating range, price range, roomType
    /// - Geo-distance: latitude + longitude + radiusKm (bounding-box SQL + Haversine)
    /// - Sorting: name, starRating, price, createdAt (asc/desc)
    /// - Pagination: page (default 1), pageSize (default 20, max 100)
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedList<HotelSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] SearchHotelsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchHotelsQuery(
            request.Q,
            request.City,
            request.Country,
            request.MinStarRating,
            request.MaxStarRating,
            request.MinPrice,
            request.MaxPrice,
            request.RoomType,
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            request.SortBy,
            request.SortDescending ?? false,
            request.Page ?? 1,
            request.PageSize ?? 20);

        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get hotel details by ID. Public endpoint — no authentication required.
    /// Returns full hotel detail including rooms.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HotelDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetHotelByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Get all hotels (admin). Returns summary DTOs for admin dashboard.
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IReadOnlyList<HotelSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromServices] IHotelRepository hotelRepository,
        CancellationToken cancellationToken)
    {
        var hotels = await hotelRepository.GetAllAsync(cancellationToken);
        var summaries = hotels.Select(HotelMappings.ToSummaryDto).ToList();
        return Ok(summaries);
    }

    /// <summary>
    /// Get all hotels owned by the authenticated user.
    /// Returns summary DTOs for the owner's dashboard.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Policy = "HotelOwnerOrAdmin")]
    [ProducesResponseType(typeof(IReadOnlyList<HotelSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyHotels(
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = new GetHotelsByOwnerQuery(ownerId);
        var result = await Mediator.Send(query, cancellationToken);

        return HandleResult(result);
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────
// Separate from command records so OwnerId is never in the request body.

/// <summary>
/// Request body for creating a hotel.
/// OwnerId excluded — set from JWT claims by the controller.
/// </summary>
public sealed record CreateHotelRequest(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    string Phone,
    string Email,
    string? Website,
    string? CheckInTime,
    string? CheckOutTime,
    double? Latitude,
    double? Longitude);

/// <summary>
/// Request body for updating a hotel.
/// OwnerId and HotelId excluded — set from JWT claims and route parameter by the controller.
/// </summary>
public sealed record UpdateHotelRequest(
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode,
    string Phone,
    string Email,
    string? Website,
    string CheckInTime,
    string CheckOutTime,
    double? Latitude,
    double? Longitude);

/// <summary>
/// Query-string parameters for hotel search.
/// All fields are optional — omitting a filter means "no restriction".
/// </summary>
public sealed record SearchHotelsRequest
{
    /// <summary>Free-text search term (matched against name and description).</summary>
    [FromQuery(Name = "q")]
    public string? Q { get; init; }

    /// <summary>Filter by city (exact match).</summary>
    public string? City { get; init; }

    /// <summary>Filter by country (exact match).</summary>
    public string? Country { get; init; }

    /// <summary>Minimum star rating (1–5).</summary>
    public int? MinStarRating { get; init; }

    /// <summary>Maximum star rating (1–5).</summary>
    public int? MaxStarRating { get; init; }

    /// <summary>Minimum room base price.</summary>
    public decimal? MinPrice { get; init; }

    /// <summary>Maximum room base price.</summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>Filter by room type (e.g., Single, Double, Suite).</summary>
    public string? RoomType { get; init; }

    /// <summary>Search center latitude (requires Longitude and RadiusKm).</summary>
    public double? Latitude { get; init; }

    /// <summary>Search center longitude (requires Latitude and RadiusKm).</summary>
    public double? Longitude { get; init; }

    /// <summary>Maximum distance in km from search center (max 500).</summary>
    public double? RadiusKm { get; init; }

    /// <summary>Sort field: name, starRating, price, createdAt. Default: createdAt.</summary>
    public string? SortBy { get; init; }

    /// <summary>Sort direction. True = descending. Default: true.</summary>
    public bool? SortDescending { get; init; }

    /// <summary>Page number (1-based). Default: 1.</summary>
    public int? Page { get; init; }

    /// <summary>Items per page (1–100). Default: 20.</summary>
    public int? PageSize { get; init; }
}
