using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.UpdateHotel;

/// <summary>
/// Handles hotel update.
///
/// Steps:
/// 1. Load hotel by ID (with rooms for accurate mapping)
/// 2. Verify ownership — OwnerId from JWT must match hotel's OwnerId
/// 3. Check for duplicate name if name is being changed
/// 4. Build value objects and call aggregate Update method
/// 5. Optionally update geo location
/// 6. Return updated DTO (TransactionBehavior commits)
/// </summary>
public sealed class UpdateHotelCommandHandler : ICommandHandler<UpdateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<UpdateHotelCommandHandler> _logger;

    public UpdateHotelCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<UpdateHotelCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<HotelDto>> Handle(
        UpdateHotelCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load hotel with rooms
        var hotel = await _hotelRepository.GetByIdWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
        {
            return Result.Failure<HotelDto>(HotelErrors.Hotel.NotFound);
        }

        // 2. Verify ownership
        if (!hotel.OwnerId.Equals(request.OwnerId, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "User {UserId} attempted to update hotel {HotelId} owned by {OwnerId}",
                request.OwnerId, request.HotelId, hotel.OwnerId);

            return Result.Failure<HotelDto>(HotelErrors.Hotel.NotOwner);
        }

        // 3. Check for duplicate name if changed
        if (!hotel.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isDuplicate = await _hotelRepository.ExistsByNameAndOwnerAsync(
                request.Name, request.OwnerId, cancellationToken);

            if (isDuplicate)
            {
                return Result.Failure<HotelDto>(HotelErrors.Hotel.DuplicateName);
            }
        }

        // 4. Build value objects
        var address = Address.Create(
            request.Street,
            request.City,
            request.State,
            request.Country,
            request.ZipCode);

        var contactInfo = ContactInfo.Create(
            request.Phone,
            request.Email,
            request.Website);

        var checkInTime = TimeOnly.ParseExact(request.CheckInTime, "HH:mm");
        var checkOutTime = TimeOnly.ParseExact(request.CheckOutTime, "HH:mm");

        // 5. Update aggregate — domain validates status allows update
        try
        {
            hotel.Update(
                request.Name,
                request.Description,
                request.StarRating,
                address,
                contactInfo,
                checkInTime,
                checkOutTime);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<HotelDto>(HotelErrors.Hotel.InvalidStatus);
        }

        // 6. Update geo location
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var location = GeoLocation.Create(request.Latitude.Value, request.Longitude.Value);
            hotel.SetLocation(location);
        }

        // 7. Mark as updated in EF change tracker
        _hotelRepository.Update(hotel);

        _logger.LogInformation(
            "Hotel {HotelId} updated by owner {OwnerId}",
            hotel.Id, request.OwnerId);

        // 8. Return updated DTO (TransactionBehavior commits)
        return HotelMappings.ToDto(hotel);
    }
}
