using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.DeleteHotelPhoto;

/// <summary>
/// Command to remove a photo from a hotel's gallery.
/// If the deleted photo is the cover image, the cover is also cleared.
///
/// The controller handles physical file deletion via IFileStorageService
/// after the command succeeds.
/// </summary>
public sealed record DeleteHotelPhotoCommand(
    Guid HotelId,
    string PhotoUrl,
    string OwnerId) : ICommand;
