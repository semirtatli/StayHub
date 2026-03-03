using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Application.Abstractions;

namespace StayHub.Services.Booking.Infrastructure.ExternalServices;

/// <summary>
/// HTTP-based implementation of the Hotel Service client.
///
/// Calls the Hotel Service REST API to fetch hotel details and check availability.
/// Uses IHttpClientFactory-managed named HttpClient ("HotelService") configured
/// in DI with the base address from appsettings.
///
/// JSON deserialization uses camelCase naming and enum-as-string conversion
/// to match the Hotel API's default ASP.NET Core JSON output.
/// </summary>
public sealed class HotelServiceHttpClient : IHotelServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HotelServiceHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public HotelServiceHttpClient(
        HttpClient httpClient,
        ILogger<HotelServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<HotelDetailResponse?> GetHotelDetailAsync(
        Guid hotelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"api/hotels/{hotelId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Hotel {HotelId} not found in Hotel Service", hotelId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<HotelServiceDetailDto>(
                JsonOptions, cancellationToken);

            if (dto is null)
                return null;

            var rooms = dto.Rooms
                .Select(r => new RoomResponse(
                    r.Id, r.Name, r.RoomType,
                    r.MaxOccupancy, r.BasePrice, r.Currency, r.IsActive))
                .ToList();

            return new HotelDetailResponse(
                dto.Id, dto.Name, dto.Status, dto.OwnerId, rooms);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error fetching hotel detail for {HotelId}", hotelId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<HotelAvailabilityResponse?> CheckAvailabilityAsync(
        Guid hotelId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var checkInStr = checkIn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var checkOutStr = checkOut.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var response = await _httpClient.GetAsync(
                $"api/hotels/{hotelId}/availability?checkIn={checkInStr}&checkOut={checkOutStr}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug(
                    "Hotel {HotelId} not found when checking availability", hotelId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<HotelServiceAvailabilityDto>(
                JsonOptions, cancellationToken);

            if (dto is null)
                return null;

            var rooms = dto.Rooms
                .Select(r => new RoomAvailabilityResponse(
                    r.RoomId, r.RoomName, r.MaxOccupancy,
                    r.IsAvailable, r.TotalPrice, r.Currency))
                .ToList();

            return new HotelAvailabilityResponse(
                dto.HotelId, dto.CheckIn, dto.CheckOut, dto.Nights, rooms);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error checking availability for hotel {HotelId}", hotelId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<CancellationPolicyResponse?> GetCancellationPolicyAsync(
        Guid hotelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"api/hotels/{hotelId}/cancellation-policy", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug(
                    "Cancellation policy not found for hotel {HotelId}", hotelId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<HotelServiceCancellationPolicyDto>(
                JsonOptions, cancellationToken);

            if (dto is null)
                return null;

            return new CancellationPolicyResponse(
                dto.PolicyType,
                dto.FreeCancellationDays,
                dto.PartialRefundPercentage,
                dto.PartialRefundDays);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP error fetching cancellation policy for hotel {HotelId}", hotelId);
            return null;
        }
    }

    // ── Internal DTOs for JSON deserialization (match Hotel API response shape) ──

    private sealed record HotelServiceDetailDto(
        Guid Id,
        string Name,
        string Description,
        int StarRating,
        string OwnerId,
        string Status,
        List<HotelServiceRoomDto> Rooms);

    private sealed record HotelServiceRoomDto(
        Guid Id,
        Guid HotelId,
        string Name,
        string Description,
        string RoomType,
        int MaxOccupancy,
        decimal BasePrice,
        string Currency,
        int TotalInventory,
        bool IsActive);

    private sealed record HotelServiceAvailabilityDto(
        Guid HotelId,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Nights,
        List<HotelServiceRoomAvailabilityDto> Rooms);

    private sealed record HotelServiceRoomAvailabilityDto(
        Guid RoomId,
        string RoomName,
        string RoomType,
        int MaxOccupancy,
        int MinAvailable,
        bool IsAvailable,
        decimal TotalPrice,
        string Currency);

    private sealed record HotelServiceCancellationPolicyDto(
        string PolicyType,
        int FreeCancellationDays,
        int PartialRefundPercentage,
        int PartialRefundDays);
}
