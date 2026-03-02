using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.RemoveRoom;

/// <summary>
/// Handles removing a room from a hotel.
///
/// Steps:
/// 1. Load hotel with rooms
/// 2. Verify ownership
/// 3. Remove room through aggregate root (raises RoomRemovedEvent)
/// 4. TransactionBehavior commits on success
/// </summary>
public sealed class RemoveRoomCommandHandler : ICommandHandler<RemoveRoomCommand>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<RemoveRoomCommandHandler> _logger;

    public RemoveRoomCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<RemoveRoomCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemoveRoomCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load hotel with rooms
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
        {
            return Result.Failure(HotelErrors.Hotel.NotFound);
        }

        // 2. Verify ownership
        if (!hotel.OwnerId.Equals(request.OwnerId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "User {UserId} attempted to remove room from hotel {HotelId} owned by {OwnerId}",
                request.OwnerId, request.HotelId, hotel.OwnerId);

            return Result.Failure(HotelErrors.Hotel.NotOwner);
        }

        // 3. Remove room through aggregate root (raises RoomRemovedEvent)
        try
        {
            hotel.RemoveRoom(request.RoomId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(HotelErrors.Room.NotFound);
        }

        // 4. Mark aggregate as updated
        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Room {RoomId} removed from hotel {HotelId}",
            request.RoomId, hotel.Id);

        // TransactionBehavior commits
        return Result.Success();
    }
}
