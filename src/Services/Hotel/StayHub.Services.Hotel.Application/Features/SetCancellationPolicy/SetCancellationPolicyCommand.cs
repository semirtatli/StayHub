using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.SetCancellationPolicy;

/// <summary>
/// Command to set or update a hotel's cancellation policy.
/// Only the hotel owner can change the policy.
///
/// Supports both predefined types (Flexible/Moderate/Strict/NonRefundable)
/// and custom configurations with explicit day/percentage values.
/// When UseCustom is false, the predefined defaults for PolicyType are used.
/// </summary>
public sealed record SetCancellationPolicyCommand(
    Guid HotelId,
    string PolicyType,
    bool UseCustom,
    int? FreeCancellationDays,
    int? PartialRefundPercentage,
    int? PartialRefundDays,
    string OwnerId) : ICommand<CancellationPolicyDto>;
