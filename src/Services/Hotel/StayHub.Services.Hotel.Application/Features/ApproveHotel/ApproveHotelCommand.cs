using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.ApproveHotel;

/// <summary>
/// Command to approve a hotel listing. Admin only.
/// Transitions from PendingApproval → Active.
/// </summary>
public sealed record ApproveHotelCommand(
    Guid HotelId,
    string AdminUserId) : ICommand;
