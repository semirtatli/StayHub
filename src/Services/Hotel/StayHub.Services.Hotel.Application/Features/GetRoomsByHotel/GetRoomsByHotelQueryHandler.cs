using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.GetRoomsByHotel;

/// <summary>
/// Handles fetching all rooms for a hotel.
/// Uses GetByIdWithRoomsAsync to eagerly load rooms.
/// </summary>
public sealed class GetRoomsByHotelQueryHandler : IQueryHandler<GetRoomsByHotelQuery, IReadOnlyList<RoomDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<GetRoomsByHotelQueryHandler> _logger;

    public GetRoomsByHotelQueryHandler(
        IHotelRepository hotelRepository,
        ILogger<GetRoomsByHotelQueryHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<RoomDto>>> Handle(
        GetRoomsByHotelQuery request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
        {
            _logger.LogWarning("Hotel {HotelId} not found", request.HotelId);
            return Result.Failure<IReadOnlyList<RoomDto>>(HotelErrors.Hotel.NotFound);
        }

        var dtos = hotel.Rooms.Select(HotelMappings.ToRoomDto).ToList();

        _logger.LogDebug(
            "Found {Count} rooms for hotel {HotelId}",
            dtos.Count, request.HotelId);

        return dtos;
    }
}
