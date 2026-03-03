using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.BlockDates;

/// <summary>
/// Blocks dates for a room. For each date:
/// - If an availability record exists → call Block()
/// - If no record exists → create one with 0 inventory and block it
///
/// Block() throws if bookings exist on that date (safety guard).
/// </summary>
public sealed class BlockDatesCommandHandler : ICommandHandler<BlockDatesCommand>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomAvailabilityRepository _availabilityRepository;

    public BlockDatesCommandHandler(
        IHotelRepository hotelRepository,
        IRoomAvailabilityRepository availabilityRepository)
    {
        _hotelRepository = hotelRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result> Handle(
        BlockDatesCommand request,
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

        // Load existing records
        var existingRecords = await _availabilityRepository.GetByRoomAndDateRangeAsync(
            request.RoomId, request.FromDate, request.ToDate, cancellationToken);

        var existingByDate = existingRecords.ToDictionary(a => a.Date);

        var current = request.FromDate;
        while (current < request.ToDate)
        {
            if (existingByDate.TryGetValue(current, out var existing))
            {
                if (!existing.IsBlocked)
                {
                    existing.Block(request.Reason);
                }
            }
            else
            {
                // Create a blocked record with 0 inventory
                var availability = RoomAvailability.Create(request.RoomId, current, 0);
                availability.Block(request.Reason);
                _availabilityRepository.Add(availability);
            }

            current = current.AddDays(1);
        }

        return Result.Success();
    }
}
