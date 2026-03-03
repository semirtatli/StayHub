using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.UploadHotelPhoto;

/// <summary>
/// Handles adding a photo URL to a hotel's gallery.
/// Optionally sets it as the cover image.
///
/// The actual file upload is handled by the controller via IFileStorageService
/// before invoking this command — keeping the command handler storage-agnostic.
/// </summary>
public sealed class UploadHotelPhotoCommandHandler : ICommandHandler<UploadHotelPhotoCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<UploadHotelPhotoCommandHandler> _logger;

    public UploadHotelPhotoCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<UploadHotelPhotoCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<HotelDto>> Handle(
        UploadHotelPhotoCommand request,
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

        // Add to gallery
        hotel.AddPhotoUrl(request.PhotoUrl);

        // Optionally set as cover image
        if (request.SetAsCover)
        {
            hotel.SetCoverImage(request.PhotoUrl);
        }

        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Photo added to hotel {HotelId} gallery. SetAsCover={SetAsCover}",
            hotel.Id, request.SetAsCover);

        return HotelMappings.ToDto(hotel);
    }
}
