namespace StayHub.Services.Booking.Application.DTOs;

/// <summary>
/// Full booking detail DTO for API responses.
/// </summary>
public sealed record BookingDto(
    Guid Id,
    Guid HotelId,
    Guid RoomId,
    string GuestUserId,
    string HotelName,
    string RoomName,
    string ConfirmationNumber,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    int NumberOfGuests,
    GuestInfoDto GuestInfo,
    PriceBreakdownDto PriceBreakdown,
    string Status,
    string PaymentStatus,
    string? SpecialRequests,
    string? CancellationReason,
    RefundInfoDto? RefundInfo,
    DateTime? CancelledAt,
    DateTime? CheckedInAt,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// Booking summary for list views — excludes detailed pricing and guest details.
/// </summary>
public sealed record BookingSummaryDto(
    Guid Id,
    string ConfirmationNumber,
    string HotelName,
    string RoomName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    string Status,
    string PaymentStatus,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt);

/// <summary>
/// Guest information DTO.
/// </summary>
public sealed record GuestInfoDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);

/// <summary>
/// Price breakdown DTO.
/// </summary>
public sealed record PriceBreakdownDto(
    decimal NightlyRate,
    int Nights,
    decimal Subtotal,
    decimal TaxAmount,
    decimal ServiceFee,
    decimal Total,
    string Currency);

/// <summary>
/// Refund information DTO — included only when a booking has been cancelled.
/// </summary>
public sealed record RefundInfoDto(
    int RefundPercentage,
    decimal RefundAmount,
    string Currency);
