using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.RejectHotel;

/// <summary>
/// Handles rejecting a hotel listing. Admin only.
/// The domain entity enforces that only PendingApproval hotels can be rejected.
/// Sets StatusReason so the owner knows what to fix before re-submitting.
/// Raises HotelStatusChangedEvent for downstream consumers.
/// </summary>
public sealed class RejectHotelCommandHandler : ICommandHandler<RejectHotelCommand>
{
    private readonly IHotelRepository _hotelRepository;

    public RejectHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result> Handle(
        RejectHotelCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Hotel.NotFound);

        try
        {
            hotel.Reject(request.AdminUserId, request.Reason);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Hotel.InvalidStatusTransition);
        }

        _hotelRepository.Update(hotel);

        return Result.Success();
    }
}
