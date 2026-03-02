using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.CreateHotel;

/// <summary>
/// Handles hotel creation.
///
/// Pipeline: ValidationBehavior → LoggingBehavior → TransactionBehavior → this handler.
/// By the time we reach here, the command has already passed FluentValidation.
/// TransactionBehavior commits the unit of work after a successful result.
///
/// Steps:
/// 1. Check for duplicate hotel name per owner
/// 2. Build value objects (Address, ContactInfo, optional GeoLocation)
/// 3. Create HotelEntity aggregate via factory method
/// 4. Persist via repository (TransactionBehavior commits)
/// 5. Return mapped DTO
/// </summary>
public sealed class CreateHotelCommandHandler : ICommandHandler<CreateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<CreateHotelCommandHandler> _logger;

    public CreateHotelCommandHandler(
        IHotelRepository hotelRepository,
        ILogger<CreateHotelCommandHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<HotelDto>> Handle(
        CreateHotelCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check for duplicate hotel name per owner
        var isDuplicate = await _hotelRepository.ExistsByNameAndOwnerAsync(
            request.Name, request.OwnerId, cancellationToken);

        if (isDuplicate)
        {
            _logger.LogWarning(
                "Duplicate hotel name '{HotelName}' for owner {OwnerId}",
                request.Name, request.OwnerId);

            return Result.Failure<HotelDto>(HotelErrors.Hotel.DuplicateName);
        }

        // 2. Build value objects
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

        // 3. Parse optional times
        TimeOnly? checkInTime = request.CheckInTime is not null
            ? TimeOnly.ParseExact(request.CheckInTime, "HH:mm")
            : null;

        TimeOnly? checkOutTime = request.CheckOutTime is not null
            ? TimeOnly.ParseExact(request.CheckOutTime, "HH:mm")
            : null;

        // 4. Create aggregate via factory method
        var hotel = HotelEntity.Create(
            request.Name,
            request.Description,
            request.StarRating,
            address,
            contactInfo,
            request.OwnerId,
            checkInTime,
            checkOutTime);

        // 5. Set optional geo location
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var location = GeoLocation.Create(request.Latitude.Value, request.Longitude.Value);
            hotel.SetLocation(location);
        }

        // 6. Persist (TransactionBehavior will call SaveChangesAsync)
        _hotelRepository.Add(hotel);

        _logger.LogInformation(
            "Hotel '{HotelName}' created with ID {HotelId} by owner {OwnerId}",
            hotel.Name, hotel.Id, hotel.OwnerId);

        // 7. Map to DTO and return
        return HotelMappings.ToDto(hotel);
    }
}
