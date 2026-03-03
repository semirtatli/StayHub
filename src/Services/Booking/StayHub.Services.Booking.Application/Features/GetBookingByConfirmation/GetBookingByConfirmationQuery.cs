using StayHub.Services.Booking.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Booking.Application.Features.GetBookingByConfirmation;

/// <summary>
/// Query to look up a booking by its confirmation number (e.g., STH-20260315-A1B2C3D4).
///
/// Used by guests who may not know their booking GUID but have the confirmation
/// number from their email. Validates guest ownership in the handler.
/// </summary>
public sealed record GetBookingByConfirmationQuery(
    string ConfirmationNumber,
    string UserId) : IQuery<BookingDto>;
