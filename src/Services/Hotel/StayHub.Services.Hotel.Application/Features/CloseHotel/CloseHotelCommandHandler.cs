using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.CloseHotel;

/// <summary>
/// Handles permanently closing a hotel. Owner or admin.
/// The domain entity enforces that already-closed hotels cannot be closed again.
/// Raises HotelStatusChangedEvent for downstream consumers.
/// </summary>
public sealed class CloseHotelCommandHandler : ICommandHandler<CloseHotelCommand>
{
    private readonly IHotelRepository _hotelRepository;

    public CloseHotelCommandHandler(IHotelRepository hotelRepository)
    {
        _hotelRepository = hotelRepository;
    }

    public async Task<Result> Handle(
        CloseHotelCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Hotel.NotFound);

        try
        {
            hotel.Close(request.UserId, request.Reason);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Hotel.InvalidStatusTransition);
        }

        _hotelRepository.Update(hotel);

        return Result.Success();
    }
}
