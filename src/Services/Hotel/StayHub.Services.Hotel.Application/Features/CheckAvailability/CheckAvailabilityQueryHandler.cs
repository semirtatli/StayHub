using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.CheckAvailability;

/// <summary>
/// Checks room availability for a hotel across a date range.
///
/// For each active room in the hotel:
/// 1. Load availability records for [CheckIn, CheckOut)
/// 2. For dates without records → treat as unavailable (not yet initialized)
/// 3. Calculate per-date pricing (PriceOverride ?? Room.BasePrice)
/// 4. Determine MinAvailable (bottleneck night), IsAvailable, and TotalPrice
/// </summary>
public sealed class CheckAvailabilityQueryHandler
    : IQueryHandler<CheckAvailabilityQuery, HotelAvailabilityDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomAvailabilityRepository _availabilityRepository;

    public CheckAvailabilityQueryHandler(
        IHotelRepository hotelRepository,
        IRoomAvailabilityRepository availabilityRepository)
    {
        _hotelRepository = hotelRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<HotelAvailabilityDto>> Handle(
        CheckAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        // ── Load hotel with rooms ───────────────────────────────────────
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure<HotelAvailabilityDto>(HotelErrors.Availability.HotelNotFound);

        var activeRooms = hotel.Rooms.Where(r => r.IsActive).ToList();
        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;

        // ── Load all availability records for all rooms in date range ───
        var roomIds = activeRooms.Select(r => r.Id).ToList();
        var allAvailability = await _availabilityRepository.GetByRoomsAndDateRangeAsync(
            roomIds, request.CheckIn, request.CheckOut, cancellationToken);

        // Group by room
        var availabilityByRoom = allAvailability
            .GroupBy(a => a.RoomId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.Date));

        // ── Build per-room availability ─────────────────────────────────
        var roomResults = new List<RoomAvailabilityDto>();

        foreach (var room in activeRooms)
        {
            availabilityByRoom.TryGetValue(room.Id, out var roomAvailability);
            roomAvailability ??= [];

            var dateResults = new List<DateAvailabilityDto>();
            var minAvailable = int.MaxValue;
            var totalPrice = 0m;
            var allDatesAvailable = true;

            var currentDate = request.CheckIn;
            while (currentDate < request.CheckOut)
            {
                if (roomAvailability.TryGetValue(currentDate, out var avail))
                {
                    var available = avail.AvailableCount;
                    var price = avail.PriceOverride ?? room.BasePrice.Amount;

                    dateResults.Add(new DateAvailabilityDto(
                        currentDate,
                        avail.TotalInventory,
                        avail.BookedCount,
                        available,
                        price,
                        avail.IsBlocked));

                    if (avail.IsBlocked || available <= 0)
                        allDatesAvailable = false;

                    if (!avail.IsBlocked)
                        minAvailable = Math.Min(minAvailable, available);
                    else
                        minAvailable = 0;

                    totalPrice += price;
                }
                else
                {
                    // No availability record → not initialized → unavailable
                    dateResults.Add(new DateAvailabilityDto(
                        currentDate,
                        TotalInventory: 0,
                        BookedCount: 0,
                        AvailableCount: 0,
                        Price: room.BasePrice.Amount,
                        IsBlocked: false));

                    allDatesAvailable = false;
                    minAvailable = 0;
                    totalPrice += room.BasePrice.Amount;
                }

                currentDate = currentDate.AddDays(1);
            }

            if (minAvailable == int.MaxValue)
                minAvailable = 0;

            roomResults.Add(new RoomAvailabilityDto(
                room.Id,
                room.Name,
                room.RoomType.ToString(),
                room.MaxOccupancy,
                minAvailable,
                allDatesAvailable,
                Math.Round(totalPrice, 2),
                room.BasePrice.Currency,
                dateResults));
        }

        var result = new HotelAvailabilityDto(
            hotel.Id,
            request.CheckIn,
            request.CheckOut,
            nights,
            roomResults);

        return result;
    }
}
