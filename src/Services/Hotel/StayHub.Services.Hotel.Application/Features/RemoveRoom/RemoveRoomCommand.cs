using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.RemoveRoom;

/// <summary>
/// Command to remove a room from a hotel.
/// The room is removed through the Hotel aggregate root (raises RoomRemovedEvent).
///
/// OwnerId is set by the controller from JWT claims — the handler verifies ownership.
/// </summary>
public sealed record RemoveRoomCommand(
    Guid HotelId,
    Guid RoomId,
    string OwnerId) : ICommand;
