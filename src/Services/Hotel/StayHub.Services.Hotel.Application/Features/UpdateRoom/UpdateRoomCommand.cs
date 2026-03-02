using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.UpdateRoom;

/// <summary>
/// Command to update an existing room's details.
/// The room is accessed through the Hotel aggregate root.
///
/// OwnerId is set by the controller from JWT claims — the handler verifies ownership.
/// </summary>
public sealed record UpdateRoomCommand(
    Guid HotelId,
    Guid RoomId,
    string Name,
    string Description,
    string RoomType,
    int MaxOccupancy,
    decimal BasePrice,
    string Currency,
    int TotalInventory,
    decimal? SizeInSquareMeters,
    string? BedConfiguration,
    IReadOnlyList<string>? Amenities,
    IReadOnlyList<string>? PhotoUrls,
    string OwnerId) : ICommand<RoomDto>;
