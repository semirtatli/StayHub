using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.UploadHotelPhoto;

/// <summary>
/// Command to upload a photo for a hotel.
/// The controller handles the multipart upload via IFileStorageService,
/// then passes the resulting URL to this command.
///
/// SetAsCover: if true, also sets the uploaded photo as the cover image.
/// </summary>
public sealed record UploadHotelPhotoCommand(
    Guid HotelId,
    string PhotoUrl,
    bool SetAsCover,
    string OwnerId) : ICommand<HotelDto>;
