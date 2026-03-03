using StayHub.Services.Booking.Domain.Enums;
using StayHub.Shared.Domain;

namespace StayHub.Services.Booking.Domain.Events;

/// <summary>
/// Raised when a new booking is created.
/// Consumers: reserve room availability, send confirmation email.
/// </summary>
public sealed record BookingCreatedEvent(
    Guid BookingId,
    Guid HotelId,
    Guid RoomId,
    string GuestUserId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests) : DomainEvent;

/// <summary>
/// Raised when a booking transitions to Confirmed (payment received).
/// Consumers: send confirmation email, notify hotel owner.
/// </summary>
public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid HotelId,
    string GuestUserId) : DomainEvent;

/// <summary>
/// Raised when booking status changes. Generic event for any transition.
/// Consumers: audit log, notifications.
/// </summary>
public sealed record BookingStatusChangedEvent(
    Guid BookingId,
    BookingStatus OldStatus,
    BookingStatus NewStatus,
    string? Reason) : DomainEvent;

/// <summary>
/// Raised when a booking is cancelled.
/// Consumers: release room availability, process refund, notify hotel owner.
/// </summary>
public sealed record BookingCancelledEvent(
    Guid BookingId,
    Guid HotelId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string? CancellationReason) : DomainEvent;

/// <summary>
/// Raised when a guest checks in.
/// Consumers: update hotel dashboard, send welcome message.
/// </summary>
public sealed record GuestCheckedInEvent(
    Guid BookingId,
    Guid HotelId,
    string GuestUserId) : DomainEvent;

/// <summary>
/// Raised when a guest checks out / booking completes.
/// Consumers: trigger review request, finalize payment.
/// </summary>
public sealed record BookingCompletedEvent(
    Guid BookingId,
    Guid HotelId,
    Guid RoomId,
    string GuestUserId) : DomainEvent;
