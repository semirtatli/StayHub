using StayHub.Shared.Result;

namespace StayHub.Services.Booking.Application;

/// <summary>
/// Static error definitions for the Booking bounded context.
/// Follows the pattern: "{Entity}.{ErrorType}" for consistent error codes.
/// </summary>
public static class BookingErrors
{
    public static class Booking
    {
        public static readonly Error NotFound = new(
            "Booking.NotFound",
            "Booking was not found.");

        public static readonly Error InvalidStatus = new(
            "Booking.InvalidStatus",
            "The operation is not allowed for the booking's current status.");

        public static readonly Error InvalidStatusTransition = new(
            "Booking.InvalidStatusTransition",
            "This status transition is not allowed for the booking's current status.");

        public static readonly Error NotGuest = new(
            "Booking.NotGuest",
            "You are not the guest of this booking.");

        public static readonly Error NotHotelOwner = new(
            "Booking.NotHotelOwner",
            "You are not the owner of the hotel for this booking.");

        public static readonly Error RoomUnavailable = new(
            "Booking.RoomUnavailable",
            "The requested room is not available for the selected dates.");

        public static readonly Error OverlappingBooking = new(
            "Booking.OverlappingBooking",
            "An active booking already exists for this room on the selected dates.");

        public static readonly Error HotelNotFound = new(
            "Booking.HotelNotFound",
            "The referenced hotel was not found or is not active.");

        public static readonly Error RoomNotFound = new(
            "Booking.RoomNotFound",
            "The referenced room was not found in the hotel.");

        public static readonly Error InvalidDateRange = new(
            "Booking.InvalidDateRange",
            "Check-in date must be before check-out date.");

        public static readonly Error DateInPast = new(
            "Booking.DateInPast",
            "Check-in date cannot be in the past.");

        public static readonly Error CancellationReasonRequired = new(
            "Booking.CancellationReasonRequired",
            "Cancellation reason is required for confirmed bookings.");

        public static readonly Error PaymentFailed = new(
            "Booking.PaymentFailed",
            "Payment processing failed.");
    }
}
