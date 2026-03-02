using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.AddRoom;

/// <summary>
/// Handles adding a room to a hotel.
///
/// Steps:
/// 1. Load hotel with rooms (need existing rooms to check name uniqueness)
/// 2. Verify ownership
/// 3. Parse RoomType enum, create Money value object
/// 4. Add room through aggregate root (enforces unique name invariant)
/// 5. Set optional properties (size, bed config, amenities, photos)
/// 6. Return mapped RoomDto (TransactionBehavior commits)
/// </summary>
public sealed class AddRoomCommandHandler : ICommandHandler<AddRoomCommand, RoomDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<AddRoomCommandHandler> _logger;

    public AddRoomCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<AddRoomCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<RoomDto>> Handle(
        AddRoomCommand request,
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
                "User {UserId} attempted to add room to hotel {HotelId} owned by {OwnerId}",
                request.OwnerId, request.HotelId, hotel.OwnerId);

            return Result.Failure<RoomDto>(HotelErrors.Hotel.NotOwner);
        }

        // 3. Parse room type and create money value object
        var roomType = Enum.Parse<RoomType>(request.RoomType, ignoreCase: true);
        var basePrice = Money.Create(request.BasePrice, request.Currency);

        // 4. Add room through aggregate root (raises RoomAddedEvent, checks name uniqueness)
        Domain.Entities.Room room;
        try
        {
            room = hotel.AddRoom(
                request.Name,
                request.Description,
                roomType,
                request.MaxOccupancy,
                basePrice,
                request.TotalInventory);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<RoomDto>(HotelErrors.Room.DuplicateName);
        }

        // 5. Set optional properties
        if (request.SizeInSquareMeters.HasValue)
        {
            room.SetSize(request.SizeInSquareMeters.Value);
        }

        if (request.BedConfiguration is not null)
        {
            room.SetBedConfiguration(request.BedConfiguration);
        }

        if (request.Amenities is { Count: > 0 })
        {
            room.SetAmenities(request.Amenities);
        }

        if (request.PhotoUrls is { Count: > 0 })
        {
            foreach (var photoUrl in request.PhotoUrls)
            {
                room.AddPhotoUrl(photoUrl);
            }
        }

        // 6. Mark aggregate as updated
        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Room '{RoomName}' ({RoomType}) added to hotel {HotelId}",
            room.Name, room.RoomType, hotel.Id);

        // 7. Return mapped DTO (TransactionBehavior commits)
        return HotelMappings.ToRoomDto(room);
    }
}
