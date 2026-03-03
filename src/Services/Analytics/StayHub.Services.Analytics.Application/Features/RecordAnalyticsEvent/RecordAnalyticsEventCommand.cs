using StayHub.Services.Analytics.Domain.Enums;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;

/// <summary>
/// Records a raw analytics event and updates the relevant read-model projections.
/// Dispatched by integration event consumers when business events arrive.
/// </summary>
public sealed record RecordAnalyticsEventCommand(
    AnalyticsEventType EventType,
    Guid HotelId,
    Guid? BookingId = null,
    Guid? UserId = null,
    string? HotelName = null,
    decimal Amount = 0,
    DateOnly? CheckInDate = null,
    DateOnly? CheckOutDate = null,
    int RoomCount = 0,
    int TotalRooms = 0,
    decimal Rating = 0) : ICommand;
