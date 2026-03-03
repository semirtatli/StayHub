using StayHub.Services.Booking.Application.DTOs;
using StayHub.Services.Booking.Domain.Entities;

namespace StayHub.Services.Booking.Application;

/// <summary>
/// Manual mapping extensions from domain entities to DTOs.
/// Using manual mappings for full control and no reflection overhead.
/// </summary>
public static class BookingMappings
{
    public static BookingDto ToDto(this BookingEntity booking)
    {
        return new BookingDto(
            booking.Id,
            booking.HotelId,
            booking.RoomId,
            booking.GuestUserId,
            booking.HotelName,
            booking.RoomName,
            booking.ConfirmationNumber,
            booking.StayPeriod.CheckIn,
            booking.StayPeriod.CheckOut,
            booking.StayPeriod.Nights,
            booking.NumberOfGuests,
            new GuestInfoDto(
                booking.GuestInfo.FirstName,
                booking.GuestInfo.LastName,
                booking.GuestInfo.Email,
                booking.GuestInfo.Phone),
            new PriceBreakdownDto(
                booking.PriceBreakdown.NightlyRate.Amount,
                booking.PriceBreakdown.Nights,
                booking.PriceBreakdown.Subtotal.Amount,
                booking.PriceBreakdown.TaxAmount.Amount,
                booking.PriceBreakdown.ServiceFee.Amount,
                booking.PriceBreakdown.Total.Amount,
                booking.PriceBreakdown.Total.Currency),
            booking.Status.ToString(),
            booking.PaymentStatus.ToString(),
            booking.SpecialRequests,
            booking.CancellationReason,
            booking.CancelledAt,
            booking.CheckedInAt,
            booking.CompletedAt,
            booking.CreatedAt,
            booking.LastModifiedAt);
    }

    public static BookingSummaryDto ToSummaryDto(this BookingEntity booking)
    {
        return new BookingSummaryDto(
            booking.Id,
            booking.ConfirmationNumber,
            booking.HotelName,
            booking.RoomName,
            booking.StayPeriod.CheckIn,
            booking.StayPeriod.CheckOut,
            booking.StayPeriod.Nights,
            booking.Status.ToString(),
            booking.PaymentStatus.ToString(),
            booking.PriceBreakdown.Total.Amount,
            booking.PriceBreakdown.Total.Currency,
            booking.CreatedAt);
    }
}
