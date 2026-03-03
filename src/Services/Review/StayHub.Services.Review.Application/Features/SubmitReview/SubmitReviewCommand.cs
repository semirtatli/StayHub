using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.SubmitReview;

/// <summary>
/// Command to submit a review for a completed hotel stay.
///
/// A guest can only review a booking once. The booking must be in Completed status.
/// UserId is set by the controller from JWT claims.
/// </summary>
public sealed record SubmitReviewCommand(
    Guid HotelId,
    Guid BookingId,
    string GuestName,
    string Title,
    string Body,
    int Cleanliness,
    int Service,
    int Location,
    int Comfort,
    int ValueForMoney,
    DateOnly StayedFrom,
    DateOnly StayedTo,
    string UserId) : ICommand<ReviewDto>;
