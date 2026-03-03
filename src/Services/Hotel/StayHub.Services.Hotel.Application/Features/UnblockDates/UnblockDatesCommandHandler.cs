using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.UnblockDates;

/// <summary>
/// Unblocks previously blocked dates for a room, restoring them to bookable status.
/// </summary>
public sealed class UnblockDatesCommandHandler : ICommandHandler<UnblockDatesCommand>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomAvailabilityRepository _availabilityRepository;

    public UnblockDatesCommandHandler(
        IHotelRepository hotelRepository,
        IRoomAvailabilityRepository availabilityRepository)
    {
        _hotelRepository = hotelRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result> Handle(
        UnblockDatesCommand request,
        CancellationToken cancellationToken)
    {
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Availability.HotelNotFound);

        if (hotel.OwnerId != request.OwnerId)
            return Result.Failure(HotelErrors.Availability.NotOwner);

        var room = hotel.Rooms.FirstOrDefault(r => r.Id == request.RoomId);
        if (room is null)
            return Result.Failure(HotelErrors.Availability.RoomNotFound);

        var existingRecords = await _availabilityRepository.GetByRoomAndDateRangeAsync(
            request.RoomId, request.FromDate, request.ToDate, cancellationToken);

        foreach (var record in existingRecords.Where(r => r.IsBlocked))
        {
            record.Unblock();
        }

        return Result.Success();
    }
}
