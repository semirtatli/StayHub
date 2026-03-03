using MediatR;

namespace StayHub.Services.Analytics.Application.IntegrationEvents;

/// <summary>
/// Integration event contracts consumed by the Analytics Service.
/// Each service defines its own view of shared event contracts.
/// When MassTransit is wired, these map to broker message types.
/// </summary>

// ── From Booking Service ──

public sealed record BookingConfirmedIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    Guid UserId,
    string HotelName,
    decimal TotalAmount,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int RoomCount,
    int TotalRooms) : INotification;

public sealed record BookingCancelledIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    decimal Amount,
    int RoomCount) : INotification;

// ── From Payment Service ──

public sealed record PaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    Guid HotelId,
    decimal Amount) : INotification;

public sealed record RefundProcessedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    Guid HotelId,
    decimal Amount) : INotification;

// ── From Review Service ──

public sealed record ReviewSubmittedIntegrationEvent(
    Guid ReviewId,
    Guid HotelId,
    Guid UserId,
    decimal OverallRating) : INotification;
