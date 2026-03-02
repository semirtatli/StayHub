using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.AddRoom;

/// <summary>
/// Command to add a room to an existing hotel.
/// Rooms are managed through the Hotel aggregate root to enforce uniqueness invariants.
///
/// OwnerId is set by the controller from JWT claims — the handler verifies ownership.
/// </summary>
public sealed record AddRoomCommand(
    Guid HotelId,
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
