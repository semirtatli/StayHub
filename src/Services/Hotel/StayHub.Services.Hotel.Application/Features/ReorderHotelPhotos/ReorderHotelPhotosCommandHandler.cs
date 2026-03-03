using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.ReorderHotelPhotos;

/// <summary>
/// Handles reordering a hotel's photo gallery.
/// Delegates to the aggregate's ReorderPhotos method which enforces
/// that the provided list contains exactly the same URLs.
/// </summary>
public sealed class ReorderHotelPhotosCommandHandler : ICommandHandler<ReorderHotelPhotosCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<ReorderHotelPhotosCommandHandler> _logger;

    public ReorderHotelPhotosCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<ReorderHotelPhotosCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<HotelDto>> Handle(
        ReorderHotelPhotosCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
        {
            return Result.Failure<HotelDto>(HotelErrors.Hotel.NotFound);
        }

        if (!hotel.OwnerId.Equals(request.OwnerId, StringComparison.Ordinal))
        {
            return Result.Failure<HotelDto>(HotelErrors.Hotel.NotOwner);
        }

        try
        {
            hotel.ReorderPhotos(request.PhotoUrls);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<HotelDto>(new Error(
                "Hotel.InvalidPhotoReorder",
                "Photo reorder list must contain exactly the same photos as the current gallery."));
        }

        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Hotel {HotelId} photos reordered ({Count} photos)",
            hotel.Id, request.PhotoUrls.Count);

        return HotelMappings.ToDto(hotel);
    }
}
