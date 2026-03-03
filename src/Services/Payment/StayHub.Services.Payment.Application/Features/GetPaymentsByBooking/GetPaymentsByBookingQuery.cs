using StayHub.Services.Payment.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Payment.Application.Features.GetPaymentsByBooking;

/// <summary>
/// Query to get all payments for a specific booking.
/// </summary>
public sealed record GetPaymentsByBookingQuery(
    Guid BookingId,
    string UserId) : IQuery<IReadOnlyList<PaymentSummaryDto>>;
