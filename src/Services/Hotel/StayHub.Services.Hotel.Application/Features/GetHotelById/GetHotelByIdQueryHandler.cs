using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.GetHotelById;

/// <summary>
/// Handles fetching a single hotel with its rooms.
/// Uses GetByIdWithRoomsAsync to eagerly load the room collection.
/// </summary>
public sealed class GetHotelByIdQueryHandler : IQueryHandler<GetHotelByIdQuery, HotelDetailDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<GetHotelByIdQueryHandler> _logger;

    public GetHotelByIdQueryHandler(
        IHotelRepository hotelRepository,
        ILogger<GetHotelByIdQueryHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<HotelDetailDto>> Handle(
        GetHotelByIdQuery request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
        {
            _logger.LogWarning("Hotel {HotelId} not found", request.HotelId);
            return Result.Failure<HotelDetailDto>(HotelErrors.Hotel.NotFound);
        }

        return HotelMappings.ToDetailDto(hotel);
    }
}
