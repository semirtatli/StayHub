using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.GetHotelsByOwner;

/// <summary>
/// Query to get all hotels owned by a specific user.
/// Returns summary DTOs for the owner's hotel management dashboard.
///
/// OwnerId is set by the controller from JWT claims — a user can only
/// list their own hotels (admin can list any owner's hotels).
/// </summary>
public sealed record GetHotelsByOwnerQuery(string OwnerId) : IQuery<IReadOnlyList<HotelSummaryDto>>;
