using StayHub.Services.Booking.Domain.Entities;

namespace StayHub.Services.Booking.Application.Abstractions;

/// <summary>
/// Generates PDF confirmation documents for bookings.
///
/// Abstraction defined in Application layer, implemented in Infrastructure.
/// Allows swapping PDF generation library without touching business logic.
/// </summary>
public interface IBookingConfirmationGenerator
{
    /// <summary>
    /// Generates a PDF confirmation document for the given booking.
    /// Returns the raw PDF bytes for streaming to the client.
    /// </summary>
    Task<byte[]> GenerateConfirmationPdfAsync(
        BookingEntity booking,
        CancellationToken cancellationToken = default);
}
