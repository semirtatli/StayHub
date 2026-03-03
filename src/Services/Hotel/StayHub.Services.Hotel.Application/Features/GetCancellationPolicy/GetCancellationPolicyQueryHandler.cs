using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.GetCancellationPolicy;

/// <summary>
/// Handles fetching a hotel's cancellation policy.
/// Returns the policy configuration for display to guests.
/// </summary>
public sealed class GetCancellationPolicyQueryHandler
    : IQueryHandler<GetCancellationPolicyQuery, CancellationPolicyDto>
{
    private readonly IHotelRepository _hotelRepository;

    public GetCancellationPolicyQueryHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result<CancellationPolicyDto>> Handle(
        GetCancellationPolicyQuery request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure<CancellationPolicyDto>(HotelErrors.Hotel.NotFound);

        var policy = hotel.CancellationPolicy;

        return Result.Success(new CancellationPolicyDto(
            policy.PolicyType.ToString(),
            policy.FreeCancellationDays,
            policy.PartialRefundPercentage,
            policy.PartialRefundDays));
    }
}
