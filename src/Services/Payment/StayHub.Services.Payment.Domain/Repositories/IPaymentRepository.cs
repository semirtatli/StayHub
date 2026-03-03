using StayHub.Services.Payment.Domain.Entities;
using StayHub.Services.Payment.Domain.Enums;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Payment.Domain.Repositories;

/// <summary>
/// Repository interface for the Payment aggregate.
/// Extends the generic IRepository with payment-specific queries.
/// </summary>
public interface IPaymentRepository : IRepository<PaymentEntity>
{
    /// <summary>Find all payments for a specific booking.</summary>
    Task<IReadOnlyList<PaymentEntity>> GetByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Find the successful payment for a booking (for refund processing).</summary>
    Task<PaymentEntity?> GetSuccessfulPaymentByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Find payment by external provider transaction ID (for webhook processing).</summary>
    Task<PaymentEntity?> GetByProviderTransactionIdAsync(
        string providerTransactionId, CancellationToken cancellationToken = default);

    /// <summary>Get all payments for a user.</summary>
    Task<IReadOnlyList<PaymentEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>Get payments by status (admin dashboard).</summary>
    Task<IReadOnlyList<PaymentEntity>> GetByStatusAsync(
        PaymentStatus status, CancellationToken cancellationToken = default);
}
