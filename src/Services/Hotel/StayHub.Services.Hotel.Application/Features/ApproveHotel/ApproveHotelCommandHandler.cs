using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.ApproveHotel;

/// <summary>
/// Handles approving a hotel listing. Admin only.
/// The domain entity enforces that only PendingApproval hotels can be approved.
/// Raises HotelStatusChangedEvent for downstream consumers.
/// </summary>
public sealed class ApproveHotelCommandHandler : ICommandHandler<ApproveHotelCommand>
{
    private readonly IHotelRepository _hotelRepository;

    public ApproveHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result> Handle(
        ApproveHotelCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Hotel.NotFound);

        try
        {
            hotel.Approve(request.AdminUserId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Hotel.InvalidStatusTransition);
        }

        _hotelRepository.Update(hotel);

        return Result.Success();
    }
}
