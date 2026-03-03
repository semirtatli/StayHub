using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.GetPendingApprovals;

/// <summary>
/// Query to get all hotels pending admin approval.
/// Admin only — used in the admin dashboard to review submitted hotels.
/// </summary>
public sealed record GetPendingApprovalsQuery : IQuery<IReadOnlyList<HotelSummaryDto>>;
