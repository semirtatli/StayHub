using Microsoft.EntityFrameworkCore;
using StayHub.Services.Review.Domain.Entities;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Shared.Infrastructure.Persistence;

namespace StayHub.Services.Review.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IReviewRepository.
/// </summary>
public sealed class ReviewRepository : Repository<ReviewEntity>, IReviewRepository
{
    private readonly ReviewDbContext _reviewDbContext;

    public ReviewRepository(ReviewDbContext dbContext) : base(dbContext)
    {
        _reviewDbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReviewEntity>> GetByHotelIdAsync(
        Guid hotelId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.HotelId == hotelId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReviewEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasUserReviewedBookingAsync(
        string userId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(r => r.UserId == userId && r.BookingId == bookingId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ReviewEntity?> GetByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.BookingId == bookingId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HotelRatingSummary?> GetRatingSummaryByHotelIdAsync(
        Guid hotelId, CancellationToken cancellationToken = default)
    {
        return await _reviewDbContext.RatingSummaries
            .FirstOrDefaultAsync(s => s.HotelId == hotelId, cancellationToken);
    }

    /// <inheritdoc />
    public void AddRatingSummary(HotelRatingSummary summary)
    {
        _reviewDbContext.RatingSummaries.Add(summary);
    }

    /// <inheritdoc />
    public void UpdateRatingSummary(HotelRatingSummary summary)
    {
        _reviewDbContext.RatingSummaries.Update(summary);
    }
}
