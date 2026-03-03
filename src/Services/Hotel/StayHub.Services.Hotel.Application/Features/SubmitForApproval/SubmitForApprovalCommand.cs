using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.SubmitForApproval;

/// <summary>
/// Command to submit a hotel for admin approval.
/// Transitions from Draft or Rejected → PendingApproval.
/// Only the hotel owner can submit their hotel for review.
/// </summary>
public sealed record SubmitForApprovalCommand(
    Guid HotelId,
    string OwnerId) : ICommand;
