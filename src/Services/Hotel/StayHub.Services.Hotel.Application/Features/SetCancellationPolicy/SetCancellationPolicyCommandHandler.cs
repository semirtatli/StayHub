using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.SetCancellationPolicy;

/// <summary>
/// Handles setting or updating a hotel's cancellation policy.
///
/// Steps:
/// 1. Fetch hotel, verify existence
/// 2. Verify ownership
/// 3. Parse policy type and create CancellationPolicy (predefined or custom)
/// 4. Set via domain method (enforces status constraint)
/// 5. Return updated policy DTO
/// </summary>
public sealed class SetCancellationPolicyCommandHandler
    : ICommandHandler<SetCancellationPolicyCommand, CancellationPolicyDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<SetCancellationPolicyCommandHandler> _logger;

    public SetCancellationPolicyCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<SetCancellationPolicyCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<CancellationPolicyDto>> Handle(
        SetCancellationPolicyCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure<CancellationPolicyDto>(HotelErrors.Hotel.NotFound);

        if (hotel.OwnerId != request.OwnerId)
            return Result.Failure<CancellationPolicyDto>(HotelErrors.Hotel.NotOwner);

        // Parse the policy type
        if (!Enum.TryParse<CancellationPolicyType>(request.PolicyType, true, out var policyType))
            return Result.Failure<CancellationPolicyDto>(HotelErrors.Hotel.InvalidStatus);

        // Create the policy (predefined defaults or custom values)
        CancellationPolicy policy;

        if (request.UseCustom)
        {
            policy = CancellationPolicy.Create(
                policyType,
                request.FreeCancellationDays!.Value,
                request.PartialRefundPercentage!.Value,
                request.PartialRefundDays!.Value);
        }
        else
        {
            policy = CancellationPolicy.FromType(policyType);
        }

        try
        {
            hotel.SetCancellationPolicy(policy);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<CancellationPolicyDto>(HotelErrors.Hotel.InvalidStatus);
        }

        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Cancellation policy set for hotel {HotelId}: {PolicyType} (FreeDays={FreeDays}, Partial={PartialPct}%, PartialDays={PartialDays})",
            hotel.Id, policyType, policy.FreeCancellationDays,
            policy.PartialRefundPercentage, policy.PartialRefundDays);

        return Result.Success(new CancellationPolicyDto(
            policyType.ToString(),
            policy.FreeCancellationDays,
            policy.PartialRefundPercentage,
            policy.PartialRefundDays));
    }
}
