using MediatR;

namespace StayHub.Services.Notification.Application.IntegrationEvents;

/// <summary>
/// Integration event contracts consumed by the Notification Service.
///
/// These mirror domain events from other services but are decoupled —
/// published via the outbox/message bus. The Notification Service subscribes
/// to these without depending on other services' domain assemblies.
///
/// All implement INotification so MediatR can dispatch them to consumer handlers.
///
/// Naming convention: past tense, suffixed with "IntegrationEvent".
/// </summary>

// ── Booking Events ──────────────────────────────────────────────────────

/// <summary>Booking was confirmed (payment received).</summary>
public sealed record BookingConfirmedIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    string GuestUserId,
    DateTime OccurredAt) : INotification;

/// <summary>Booking was cancelled.</summary>
public sealed record BookingCancelledIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    string? CancellationReason,
    DateTime OccurredAt) : INotification;

/// <summary>Guest has checked in.</summary>
public sealed record GuestCheckedInIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    string GuestUserId,
    DateTime OccurredAt) : INotification;

/// <summary>Booking completed (checkout) — trigger review reminder.</summary>
public sealed record BookingCompletedIntegrationEvent(
    Guid BookingId,
    Guid HotelId,
    string GuestUserId,
    DateTime OccurredAt) : INotification;

// ── Payment Events ──────────────────────────────────────────────────────

/// <summary>Payment succeeded — send receipt.</summary>
public sealed record PaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    DateTime OccurredAt) : INotification;

/// <summary>Payment failed — notify guest.</summary>
public sealed record PaymentFailedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    string FailureReason,
    DateTime OccurredAt) : INotification;

/// <summary>Refund has been processed.</summary>
public sealed record RefundProcessedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal RefundAmount,
    string Currency,
    bool IsFullRefund,
    DateTime OccurredAt) : INotification;
