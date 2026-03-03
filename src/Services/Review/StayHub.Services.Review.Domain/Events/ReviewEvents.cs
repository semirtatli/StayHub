using StayHub.Shared.Domain;

namespace StayHub.Services.Review.Domain.Events;

/// <summary>Raised when a new review is submitted.</summary>
public sealed record ReviewSubmittedEvent(
    Guid ReviewId,
    Guid HotelId,
    string UserId,
    decimal OverallRating) : DomainEvent;

/// <summary>Raised when a review is updated.</summary>
public sealed record ReviewUpdatedEvent(
    Guid ReviewId,
    Guid HotelId,
    decimal OldOverallRating,
    decimal NewOverallRating) : DomainEvent;

/// <summary>Raised when a review is soft-deleted.</summary>
public sealed record ReviewDeletedEvent(
    Guid ReviewId,
    Guid HotelId,
    decimal OverallRating) : DomainEvent;

/// <summary>Raised when a hotel's aggregate rating summary is recalculated.</summary>
public sealed record HotelRatingRecalculatedEvent(
    Guid HotelId,
    decimal AverageOverall,
    int TotalReviews) : DomainEvent;
