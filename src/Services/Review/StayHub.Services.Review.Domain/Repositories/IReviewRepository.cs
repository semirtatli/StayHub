using StayHub.Services.Review.Domain.Entities;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Review.Domain.Repositories;

/// <summary>
/// Repository interface for the Review aggregate.
/// </summary>
public interface IReviewRepository : IRepository<ReviewEntity>
{
    /// <summary>Get all reviews for a specific hotel.</summary>
    Task<IReadOnlyList<ReviewEntity>> GetByHotelIdAsync(
        Guid hotelId, CancellationToken cancellationToken = default);

    /// <summary>Get all reviews by a specific user.</summary>
    Task<IReadOnlyList<ReviewEntity>> GetByUserIdAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>Check if a user already reviewed a specific booking.</summary>
    Task<bool> HasUserReviewedBookingAsync(
        string userId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Get the review for a specific booking.</summary>
    Task<ReviewEntity?> GetByBookingIdAsync(
        Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Get rating summary for a hotel (for recalculation).</summary>
    Task<HotelRatingSummary?> GetRatingSummaryByHotelIdAsync(
        Guid hotelId, CancellationToken cancellationToken = default);

    /// <summary>Add a new rating summary.</summary>
    void AddRatingSummary(HotelRatingSummary summary);

    /// <summary>Update a rating summary.</summary>
    void UpdateRatingSummary(HotelRatingSummary summary);
}
