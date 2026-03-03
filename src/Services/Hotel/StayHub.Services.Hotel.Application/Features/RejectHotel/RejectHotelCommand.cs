using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.RejectHotel;

/// <summary>
/// Command to reject a hotel listing. Admin only.
/// Transitions from PendingApproval → Rejected. Requires a reason.
/// The owner can later fix issues and re-submit for approval.
/// </summary>
public sealed record RejectHotelCommand(
    Guid HotelId,
    string Reason,
    string AdminUserId) : ICommand;
