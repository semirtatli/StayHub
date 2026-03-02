using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.GetHotelsByOwner;

/// <summary>
/// Handles fetching all hotels for a given owner.
/// Returns summary DTOs (without room details) for list/dashboard display.
/// </summary>
public sealed class GetHotelsByOwnerQueryHandler : IQueryHandler<GetHotelsByOwnerQuery, IReadOnlyList<HotelSummaryDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<GetHotelsByOwnerQueryHandler> _logger;

    public GetHotelsByOwnerQueryHandler(
        IHotelRepository hotelRepository,
        ILogger<GetHotelsByOwnerQueryHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<HotelSummaryDto>>> Handle(
        GetHotelsByOwnerQuery request,
        CancellationToken cancellationToken)
    {
        var hotels = await _hotelRepository.GetByOwnerIdAsync(
            request.OwnerId, cancellationToken);

        _logger.LogDebug(
            "Found {Count} hotels for owner {OwnerId}",
            hotels.Count, request.OwnerId);

        var dtos = hotels.Select(HotelMappings.ToSummaryDto).ToList();

        return dtos;
    }
}
