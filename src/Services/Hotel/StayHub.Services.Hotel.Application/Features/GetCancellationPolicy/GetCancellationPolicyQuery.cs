using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.GetCancellationPolicy;

/// <summary>
/// Query to get a hotel's cancellation policy.
/// Public endpoint — guests need to see the policy before booking.
/// </summary>
public sealed record GetCancellationPolicyQuery(
    Guid HotelId) : IQuery<CancellationPolicyDto>;
