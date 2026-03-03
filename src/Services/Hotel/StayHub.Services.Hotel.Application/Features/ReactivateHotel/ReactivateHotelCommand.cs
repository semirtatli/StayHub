using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.ReactivateHotel;

/// <summary>
/// Command to reactivate a suspended hotel. Admin only.
/// Transitions from Suspended → Active.
/// </summary>
public sealed record ReactivateHotelCommand(
    Guid HotelId,
    string AdminUserId) : ICommand;
