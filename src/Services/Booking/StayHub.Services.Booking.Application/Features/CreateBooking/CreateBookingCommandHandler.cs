using Microsoft.Extensions.Logging;
using StayHub.Services.Booking.Application.Abstractions;
using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Entities;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Services.Booking.Domain.ValueObjects;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application.Features.CreateBooking;

/// <summary>
/// Handles reservation creation.
///
/// Pipeline: ValidationBehavior → LoggingBehavior → TransactionBehavior → this handler.
/// By the time we reach here, the command has already passed FluentValidation.
/// TransactionBehavior commits the unit of work after a successful result.
///
/// Steps:
/// 1. Fetch hotel detail from Hotel Service — verify the hotel exists and is Active
/// 2. Locate the requested room — verify it exists and is active
/// 3. Check availability from Hotel Service — verify the room is available for the dates
/// 4. Check for overlapping bookings in the Booking database (double-safety)
/// 5. Validate guest count against room max occupancy
/// 6. Calculate price breakdown from the room's nightly rate
/// 7. Create BookingEntity aggregate via factory method
/// 8. Persist via repository (TransactionBehavior commits)
/// 9. Return mapped DTO
/// </summary>
public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, BookingDto>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IHotelServiceClient _hotelServiceClient;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IHotelServiceClient hotelServiceClient,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _bookingRepository = bookingRepository;
        _hotelServiceClient = hotelServiceClient;
        _logger = logger;
    }

    public async Task<Result<BookingDto>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch hotel detail — verify existence and Active status
        var hotelDetail = await _hotelServiceClient.GetHotelDetailAsync(
            request.HotelId, cancellationToken);

        if (hotelDetail is null)
        {
            _logger.LogWarning("Hotel {HotelId} not found when creating booking", request.HotelId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.HotelNotFound);
        }

        if (!string.Equals(hotelDetail.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Hotel {HotelId} is not active (Status={Status}), cannot create booking",
                request.HotelId, hotelDetail.Status);
            return Result.Failure<BookingDto>(BookingErrors.Booking.HotelNotFound);
        }

        // 2. Find the requested room in the hotel
        var room = hotelDetail.Rooms.FirstOrDefault(r => r.Id == request.RoomId);

        if (room is null)
        {
            _logger.LogWarning(
                "Room {RoomId} not found in hotel {HotelId}",
                request.RoomId, request.HotelId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.RoomNotFound);
        }

        if (!room.IsActive)
        {
            _logger.LogWarning("Room {RoomId} is not active", request.RoomId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.RoomNotFound);
        }

        // 3. Validate guest count against room max occupancy
        if (request.NumberOfGuests > room.MaxOccupancy)
        {
            _logger.LogWarning(
                "Guest count {GuestCount} exceeds room max occupancy {MaxOccupancy} for room {RoomId}",
                request.NumberOfGuests, room.MaxOccupancy, request.RoomId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.RoomUnavailable);
        }

        // 4. Check availability via Hotel Service
        var availability = await _hotelServiceClient.CheckAvailabilityAsync(
            request.HotelId, request.CheckIn, request.CheckOut, cancellationToken);

        if (availability is null)
        {
            _logger.LogWarning(
                "Availability check failed for hotel {HotelId}", request.HotelId);
            return Result.Failure<BookingDto>(BookingErrors.Booking.HotelNotFound);
        }

        var roomAvailability = availability.Rooms.FirstOrDefault(r => r.RoomId == request.RoomId);

        if (roomAvailability is null || !roomAvailability.IsAvailable)
        {
            _logger.LogWarning(
                "Room {RoomId} is not available for {CheckIn} to {CheckOut}",
                request.RoomId, request.CheckIn, request.CheckOut);
            return Result.Failure<BookingDto>(BookingErrors.Booking.RoomUnavailable);
        }

        // 5. Double-check for overlapping bookings in our own database
        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(
            request.RoomId, request.CheckIn, request.CheckOut, cancellationToken);

        if (hasOverlap)
        {
            _logger.LogWarning(
                "Overlapping booking exists for room {RoomId} from {CheckIn} to {CheckOut}",
                request.RoomId, request.CheckIn, request.CheckOut);
            return Result.Failure<BookingDto>(BookingErrors.Booking.OverlappingBooking);
        }

        // 6. Calculate price breakdown from room nightly rate
        var nightlyRate = Money.Create(room.BasePrice, room.Currency);
        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var priceBreakdown = PriceBreakdown.Calculate(nightlyRate, nights);

        // 7. Build guest info snapshot
        var guestInfo = GuestInfo.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        // 8. Create the booking aggregate
        var booking = BookingEntity.Create(
            request.HotelId,
            request.RoomId,
            request.GuestUserId,
            hotelDetail.Name,
            room.Name,
            request.CheckIn,
            request.CheckOut,
            request.NumberOfGuests,
            guestInfo,
            priceBreakdown,
            request.SpecialRequests);

        // 9. Persist (TransactionBehavior will call SaveChangesAsync)
        _bookingRepository.Add(booking);

        _logger.LogInformation(
            "Booking {BookingId} created — Confirmation: {ConfirmationNumber}, Hotel: {HotelId}, Room: {RoomId}, Guest: {GuestUserId}, {CheckIn} to {CheckOut}",
            booking.Id, booking.ConfirmationNumber, request.HotelId,
            request.RoomId, request.GuestUserId, request.CheckIn, request.CheckOut);

        // 10. Map to DTO and return
        return Result.Success(booking.ToDto());
    }
}
