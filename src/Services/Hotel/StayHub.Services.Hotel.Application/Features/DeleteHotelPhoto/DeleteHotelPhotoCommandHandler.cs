using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.DeleteHotelPhoto;

/// <summary>
/// Handles removing a photo URL from a hotel's gallery.
/// If the deleted photo was the cover image, the cover is cleared.
/// Physical file deletion is handled by the controller after this command succeeds.
/// </summary>
public sealed class DeleteHotelPhotoCommandHandler : ICommandHandler<DeleteHotelPhotoCommand>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<DeleteHotelPhotoCommandHandler> _logger;

    public DeleteHotelPhotoCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<DeleteHotelPhotoCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteHotelPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
        {
            return Result.Failure(HotelErrors.Hotel.NotFound);
        }

        if (!hotel.OwnerId.Equals(request.OwnerId, StringComparison.Ordinal))
        {
            return Result.Failure(HotelErrors.Hotel.NotOwner);
        }

        // Remove from gallery
        hotel.RemovePhotoUrl(request.PhotoUrl);

        // Clear cover if the deleted photo was the cover
        if (hotel.CoverImageUrl == request.PhotoUrl)
        {
            hotel.SetCoverImage(null);
        }

        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Photo removed from hotel {HotelId} gallery: {PhotoUrl}",
            hotel.Id, request.PhotoUrl);

        return Result.Success();
    }
}
