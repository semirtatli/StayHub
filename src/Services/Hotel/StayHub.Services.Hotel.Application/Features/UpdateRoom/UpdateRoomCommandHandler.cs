using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.UpdateRoom;

/// <summary>
/// Handles updating a room in a hotel.
///
/// Steps:
/// 1. Load hotel with rooms
/// 2. Verify ownership
/// 3. Get room from aggregate (throws if not found)
/// 4. Update room properties via entity methods
/// 5. Return mapped RoomDto (TransactionBehavior commits)
/// </summary>
public sealed class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand, RoomDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<UpdateRoomCommandHandler> _logger;

    public UpdateRoomCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<UpdateRoomCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<RoomDto>> Handle(
        UpdateRoomCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load hotel with rooms
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
        {
            return Result.Failure<RoomDto>(HotelErrors.Hotel.NotFound);
        }

        // 2. Verify ownership
        if (!hotel.OwnerId.Equals(request.OwnerId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "User {UserId} attempted to update room in hotel {HotelId} owned by {OwnerId}",
                request.OwnerId, request.HotelId, hotel.OwnerId);

            return Result.Failure<RoomDto>(HotelErrors.Hotel.NotOwner);
        }

        // 3. Get room from aggregate
        Domain.Entities.Room room;
        try
        {
            room = hotel.GetRoom(request.RoomId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<RoomDto>(HotelErrors.Room.NotFound);
        }

        // 4. Check for duplicate name if name is being changed (exclude current room)
        if (!room.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var duplicateRoom = hotel.Rooms.FirstOrDefault(r =>
                r.Id != request.RoomId &&
                r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

            if (duplicateRoom is not null)
            {
                return Result.Failure<RoomDto>(HotelErrors.Room.DuplicateName);
            }
        }

        // 5. Parse room type and create money value object
        var roomType = Enum.Parse<RoomType>(request.RoomType, ignoreCase: true);
        var basePrice = Money.Create(request.BasePrice, request.Currency);

        // 6. Update room via entity method
        room.Update(
            request.Name,
            request.Description,
            roomType,
            request.MaxOccupancy,
            basePrice,
            request.TotalInventory);

        // 7. Set optional properties
        room.SetSize(request.SizeInSquareMeters);
        room.SetBedConfiguration(request.BedConfiguration);

        if (request.Amenities is not null)
        {
            room.SetAmenities(request.Amenities);
        }

        if (request.PhotoUrls is not null)
        {
            // Replace all photos — clear existing, add new
            foreach (var existingUrl in room.PhotoUrls.ToList())
            {
                room.RemovePhotoUrl(existingUrl);
            }

            foreach (var photoUrl in request.PhotoUrls)
            {
                room.AddPhotoUrl(photoUrl);
            }
        }

        // 8. Mark aggregate as updated
        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Room {RoomId} in hotel {HotelId} updated",
            room.Id, hotel.Id);

        // 9. Return mapped DTO (TransactionBehavior commits)
        return HotelMappings.ToRoomDto(room);
    }
}
