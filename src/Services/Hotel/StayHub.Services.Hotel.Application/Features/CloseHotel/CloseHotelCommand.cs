using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.CloseHotel;

/// <summary>
/// Command to permanently close a hotel. Owner or admin.
/// Can transition from any status except Closed.
/// This is a terminal state — the hotel cannot be reactivated after closing.
/// </summary>
public sealed record CloseHotelCommand(
    Guid HotelId,
    string? Reason,
    string UserId) : ICommand;
