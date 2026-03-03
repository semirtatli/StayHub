using StayHub.Shared.Result;

namespace StayHub.Services.Review.Application;

/// <summary>
/// Static error definitions for the Review bounded context.
/// </summary>
public static class ReviewErrors
{
    public static class Review
    {
        public static readonly Error NotFound = new(
            "Review.NotFound",
            "Review was not found.");

        public static readonly Error AlreadyReviewed = new(
            "Review.AlreadyReviewed",
            "You have already submitted a review for this booking.");

        public static readonly Error NotAuthor = new(
            "Review.NotAuthor",
            "You are not the author of this review.");

        public static readonly Error BookingNotCompleted = new(
            "Review.BookingNotCompleted",
            "You can only review a booking after the stay is completed.");

        public static readonly Error BookingNotFound = new(
            "Review.BookingNotFound",
            "The referenced booking was not found.");

        public static readonly Error HotelNotFound = new(
            "Review.HotelNotFound",
            "The referenced hotel was not found.");

        public static readonly Error NotHotelOwner = new(
            "Review.NotHotelOwner",
            "Only the hotel owner can respond to reviews.");
    }

    public static class RatingSummary
    {
        public static readonly Error NotFound = new(
            "RatingSummary.NotFound",
            "Rating summary was not found for this hotel.");
    }
}
