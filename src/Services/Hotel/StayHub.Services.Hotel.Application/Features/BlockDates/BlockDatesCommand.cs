using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.BlockDates;

/// <summary>
/// Command to block dates for a room (maintenance, renovation, seasonal closure).
/// Blocked dates cannot be booked regardless of available inventory.
/// Only allowed if no bookings exist on those dates.
/// </summary>
public sealed record BlockDatesCommand(
    Guid HotelId,
    Guid RoomId,
    DateOnly FromDate,
    DateOnly ToDate,
    string? Reason,
    string OwnerId) : ICommand;
