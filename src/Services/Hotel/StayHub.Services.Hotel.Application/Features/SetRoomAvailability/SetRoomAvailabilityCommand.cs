using StayHub.Shared.CQRS;

namespace StayHub.Services.Hotel.Application.Features.SetRoomAvailability;

/// <summary>
/// Command to initialize or update room availability for a date range.
///
/// Creates RoomAvailability records for each date in [FromDate, ToDate).
/// If records already exist for those dates, updates TotalInventory and PriceOverride.
///
/// Used by hotel owners to:
/// - Open rooms for booking (initial inventory setup)
/// - Adjust capacity for specific periods (seasonal changes, maintenance)
/// - Set date-specific pricing (holidays, events)
/// </summary>
public sealed record SetRoomAvailabilityCommand(
    Guid HotelId,
    Guid RoomId,
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalInventory,
    decimal? PriceOverride,
    string OwnerId) : ICommand;
