namespace StayHub.Services.Review.Application.DTOs;

/// <summary>
/// Full review detail DTO for API responses.
/// </summary>
public sealed record ReviewDto(
    Guid Id,
    Guid HotelId,
    Guid BookingId,
    string UserId,
    string GuestName,
    string Title,
    string Body,
    RatingDto Rating,
    DateOnly StayedFrom,
    DateOnly StayedTo,
    string? ManagementResponse,
    DateTime? ManagementResponseAt,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// Review summary for list views.
/// </summary>
public sealed record ReviewSummaryDto(
    Guid Id,
    string GuestName,
    string Title,
    decimal OverallRating,
    DateOnly StayedFrom,
    DateOnly StayedTo,
    string? ManagementResponse,
    DateTime CreatedAt);

/// <summary>
/// Detailed rating breakdown DTO.
/// </summary>
public sealed record RatingDto(
    int Cleanliness,
    int Service,
    int Location,
    int Comfort,
    int ValueForMoney,
    decimal Overall);

/// <summary>
/// Hotel rating summary DTO — aggregated scores.
/// </summary>
public sealed record HotelRatingSummaryDto(
    Guid HotelId,
    int TotalReviews,
    decimal AverageOverall,
    decimal AverageCleanliness,
    decimal AverageService,
    decimal AverageLocation,
    decimal AverageComfort,
    decimal AverageValueForMoney);
