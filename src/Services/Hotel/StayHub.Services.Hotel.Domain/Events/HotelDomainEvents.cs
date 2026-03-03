using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.Events;

/// <summary>
/// Raised when a new hotel listing is created.
/// Handlers: notify admin for approval, track analytics.
/// </summary>
public sealed record HotelCreatedEvent(
    Guid HotelId,
    string Name,
    string OwnerId) : DomainEvent;

/// <summary>
/// Raised when hotel details are updated (name, description, contact, etc.).
/// Handlers: invalidate search cache, update denormalized data.
/// </summary>
public sealed record HotelUpdatedEvent(
    Guid HotelId,
    string Name) : DomainEvent;

/// <summary>
/// Raised when a hotel's status changes (approval workflow transitions).
/// Handlers: notify owner, update search index.
/// </summary>
public sealed record HotelStatusChangedEvent(
    Guid HotelId,
    HotelStatus OldStatus,
    HotelStatus NewStatus,
    string ChangedByUserId,
    string? Reason) : DomainEvent;

/// <summary>
/// Raised when a room is added to a hotel.
/// Handlers: update room count, recalculate price range.
/// </summary>
public sealed record RoomAddedEvent(
    Guid HotelId,
    Guid RoomId,
    string RoomName,
    RoomType RoomType) : DomainEvent;

/// <summary>
/// Raised when a room is updated.
/// Handlers: update search index pricing.
/// </summary>
public sealed record RoomUpdatedEvent(
    Guid HotelId,
    Guid RoomId,
    string RoomName) : DomainEvent;

/// <summary>
/// Raised when a room is removed from a hotel.
/// Handlers: cancel pending reservations for this room, update availability.
/// </summary>
public sealed record RoomRemovedEvent(
    Guid HotelId,
    Guid RoomId) : DomainEvent;

/// <summary>
/// Raised when room availability is initialized/updated for a date range.
/// Handlers: notify booking service, update search availability indicators.
/// </summary>
public sealed record RoomAvailabilityUpdatedEvent(
    Guid HotelId,
    Guid RoomId,
    DateOnly FromDate,
    DateOnly ToDate) : DomainEvent;
