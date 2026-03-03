using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.SuspendHotel;

/// <summary>
/// Command to suspend an active hotel. Can be done by the owner or admin.
/// Transitions from Active → Suspended. Optional reason.
/// Suspended hotels are hidden from search results.
/// </summary>
public sealed record SuspendHotelCommand(
    Guid HotelId,
    string? Reason,
    string UserId) : ICommand;
