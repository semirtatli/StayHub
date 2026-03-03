using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.UnblockDates;

/// <summary>
/// Command to unblock previously blocked dates for a room.
/// Restores dates to bookable status.
/// </summary>
public sealed record UnblockDatesCommand(
    Guid HotelId,
    Guid RoomId,
    DateOnly FromDate,
    DateOnly ToDate,
    string OwnerId) : ICommand;
