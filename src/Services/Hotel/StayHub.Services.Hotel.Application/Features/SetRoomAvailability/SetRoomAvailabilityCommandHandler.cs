using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.SetRoomAvailability;

/// <summary>
/// Handles initializing/updating room availability for a date range.
///
/// For each date in [FromDate, ToDate):
/// - If no record exists → create one with TotalInventory and optional PriceOverride
/// - If a record exists → update TotalInventory (must not go below BookedCount)
///   and optionally set PriceOverride
///
/// TransactionBehavior auto-commits on success.
/// </summary>
public sealed class SetRoomAvailabilityCommandHandler : ICommandHandler<SetRoomAvailabilityCommand>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomAvailabilityRepository _availabilityRepository;

    public SetRoomAvailabilityCommandHandler(
        IHotelRepository hotelRepository,
        IRoomAvailabilityRepository availabilityRepository)
    {
        _hotelRepository = hotelRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result> Handle(
        SetRoomAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        // ── Load hotel with rooms ───────────────────────────────────────
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(
            request.HotelId, cancellationToken);

        if (hotel is null)
            return Result.Failure(HotelErrors.Availability.HotelNotFound);

        if (hotel.OwnerId != request.OwnerId)
            return Result.Failure(HotelErrors.Availability.NotOwner);

        // ── Verify room exists ──────────────────────────────────────────
        var room = hotel.Rooms.FirstOrDefault(r => r.Id == request.RoomId);
        if (room is null)
            return Result.Failure(HotelErrors.Availability.RoomNotFound);

        // ── Load existing availability records for the range ────────────
        var existingRecords = await _availabilityRepository.GetByRoomAndDateRangeAsync(
            request.RoomId, request.FromDate, request.ToDate, cancellationToken);

        var existingByDate = existingRecords.ToDictionary(a => a.Date);

        // ── Create or update each date ──────────────────────────────────
        var current = request.FromDate;
        while (current < request.ToDate)
        {
            if (existingByDate.TryGetValue(current, out var existing))
            {
                // Update existing record
                existing.UpdateInventory(request.TotalInventory);
                existing.SetPriceOverride(request.PriceOverride);
            }
            else
            {
                // Create new record
                var availability = RoomAvailability.Create(
                    request.RoomId, current, request.TotalInventory);
                availability.SetPriceOverride(request.PriceOverride);
                _availabilityRepository.Add(availability);
            }

            current = current.AddDays(1);
        }

        return Result.Success();
    }
}
