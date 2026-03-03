using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.ReorderHotelPhotos;

/// <summary>
/// Command to reorder a hotel's photo gallery.
/// The provided list must contain exactly the same URLs — no additions or removals.
/// </summary>
public sealed record ReorderHotelPhotosCommand(
    Guid HotelId,
    IReadOnlyList<string> PhotoUrls,
    string OwnerId) : ICommand<HotelDto>;
