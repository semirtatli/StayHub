using StayHub.Services.Review.Domain.Events;
using StayHub.Services.Review.Domain.ValueObjects;
using StayHub.Shared.Domain;

namespace StayHub.Services.Review.Domain.Entities;

/// <summary>
/// Review aggregate root — represents a guest's review of a hotel after a completed stay.
///
/// Invariants:
/// - A guest can only review a hotel once per booking (enforced at application layer).
/// - Rating categories are each 1–5; Overall is calculated.
/// - Title is required, 3–200 chars.
/// - Body is required, 10–5000 chars.
/// - Review can only be created for a completed booking (enforced at application layer).
///
/// Lifecycle:
///   Created → (optionally Updated) → (optionally Soft-Deleted)
///
/// The review stores a snapshot of the booking reference for traceability.
/// </summary>
public sealed class ReviewEntity : AggregateRoot
{
    /// <summary>Reference to the hotel being reviewed.</summary>
    public Guid HotelId { get; private init; }

    /// <summary>Reference to the booking that justified this review.</summary>
    public Guid BookingId { get; private init; }

    /// <summary>The guest user who wrote the review.</summary>
    public string UserId { get; private init; } = null!;

    /// <summary>Guest display name snapshot (for denormalized read views).</summary>
    public string GuestName { get; private init; } = null!;

    /// <summary>Review title / headline.</summary>
    public string Title { get; private set; } = null!;

    /// <summary>Review body / detailed text.</summary>
    public string Body { get; private set; } = null!;

    /// <summary>Detailed category ratings.</summary>
    public Rating Rating { get; private set; } = null!;

    /// <summary>Stay dates snapshot for context (e.g., "January 2026").</summary>
    public DateOnly StayedFrom { get; private init; }
    public DateOnly StayedTo { get; private init; }

    /// <summary>Optional management response to this review.</summary>
    public string? ManagementResponse { get; private set; }

    /// <summary>When the management response was posted.</summary>
    public DateTime? ManagementResponseAt { get; private set; }

    // EF Core constructor
    private ReviewEntity()
    {
        UserId = null!;
        GuestName = null!;
        Title = null!;
        Body = null!;
        Rating = null!;
    }

    /// <summary>
    /// Creates a new review for a completed hotel stay.
    /// </summary>
    public static ReviewEntity Create(
        Guid hotelId,
        Guid bookingId,
        string userId,
        string guestName,
        string title,
        string body,
        Rating rating,
        DateOnly stayedFrom,
        DateOnly stayedTo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(guestName);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentNullException.ThrowIfNull(rating);

        if (title.Length is < 3 or > 200)
            throw new ArgumentOutOfRangeException(nameof(title), "Title must be between 3 and 200 characters.");

        if (body.Length is < 10 or > 5000)
            throw new ArgumentOutOfRangeException(nameof(body), "Body must be between 10 and 5000 characters.");

        var review = new ReviewEntity
        {
            HotelId = hotelId,
            BookingId = bookingId,
            UserId = userId,
            GuestName = guestName,
            Title = title,
            Body = body,
            Rating = rating,
            StayedFrom = stayedFrom,
            StayedTo = stayedTo
        };

        review.RaiseDomainEvent(new ReviewSubmittedEvent(
            review.Id, hotelId, userId, rating.Overall));

        return review;
    }

    /// <summary>
    /// Updates the review title, body, and/or rating.
    /// Only the review author can update (enforced at application layer).
    /// </summary>
    public void Update(string title, string body, Rating rating)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentNullException.ThrowIfNull(rating);

        if (title.Length is < 3 or > 200)
            throw new ArgumentOutOfRangeException(nameof(title), "Title must be between 3 and 200 characters.");

        if (body.Length is < 10 or > 5000)
            throw new ArgumentOutOfRangeException(nameof(body), "Body must be between 10 and 5000 characters.");

        var oldOverall = Rating.Overall;

        Title = title;
        Body = body;
        Rating = rating;

        RaiseDomainEvent(new ReviewUpdatedEvent(
            Id, HotelId, oldOverall, rating.Overall));
    }

    /// <summary>
    /// Adds or updates a management response to this review.
    /// Only the hotel owner or admin can respond (enforced at application layer).
    /// </summary>
    public void AddManagementResponse(string response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response);

        if (response.Length > 2000)
            throw new ArgumentOutOfRangeException(nameof(response), "Management response must not exceed 2000 characters.");

        ManagementResponse = response;
        ManagementResponseAt = DateTime.UtcNow;
    }
}
