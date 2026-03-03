using StayHub.Services.Analytics.Domain.Enums;
using StayHub.Shared.Domain;

namespace StayHub.Services.Analytics.Domain.Entities;

/// <summary>
/// Raw analytics event — an append-only log of all business events
/// consumed from other services. Serves as the event store for
/// rebuilding projections and ad-hoc analysis.
/// </summary>
public sealed class AnalyticsEvent : Entity
{
    public Guid HotelId { get; private set; }
    public Guid? BookingId { get; private set; }
    public Guid? UserId { get; private set; }
    public AnalyticsEventType EventType { get; private set; }
    public decimal Amount { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private AnalyticsEvent() { }

    public static AnalyticsEvent Create(
        Guid hotelId,
        Guid? bookingId,
        Guid? userId,
        AnalyticsEventType eventType,
        decimal amount,
        string? metadata,
        DateTime occurredAt)
    {
        return new AnalyticsEvent
        {
            HotelId = hotelId,
            BookingId = bookingId,
            UserId = userId,
            EventType = eventType,
            Amount = amount,
            Metadata = metadata,
            OccurredAt = occurredAt
        };
    }
}
