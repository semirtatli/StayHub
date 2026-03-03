using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.ReactivateHotel;

/// <summary>
/// Handles reactivating a suspended hotel. Admin only.
/// The domain entity enforces that only Suspended hotels can be reactivated.
/// Raises HotelStatusChangedEvent for downstream consumers.
/// </summary>
public sealed class ReactivateHotelCommandHandler : ICommandHandler<ReactivateHotelCommand>
{
    private readonly IHotelRepository _hotelRepository;

    public ReactivateHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result> Handle(
        ReactivateHotelCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Hotel.NotFound);

        try
        {
            hotel.Reactivate(request.AdminUserId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Hotel.InvalidStatusTransition);
        }

        _hotelRepository.Update(hotel);

        return Result.Success();
    }
}
