using Microsoft.EntityFrameworkCore;
using StayHub.Services.Payment.Domain.Entities;
using StayHub.Services.Payment.Domain.Enums;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Payment.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IPaymentRepository.
/// </summary>
public sealed class PaymentRepository : Repository<PaymentEntity>, IPaymentRepository
{
    public PaymentRepository(PaymentDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentEntity>> GetByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentEntity?> GetSuccessfulPaymentByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                p => p.BookingId == bookingId && p.Status == PaymentStatus.Succeeded,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentEntity?> GetByProviderTransactionIdAsync(
        string providerTransactionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                p => p.ProviderTransactionId == providerTransactionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentEntity>> GetByStatusAsync(
        PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
