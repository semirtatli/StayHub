namespace StayHub.Services.Booking.Domain.Enums;

/// <summary>
/// Booking lifecycle state machine:
///
///   Pending → Confirmed → CheckedIn → Completed
///             ↓                      ↓
///           Cancelled             Cancelled
///
/// Pending:    Reservation created, awaiting payment confirmation.
/// Confirmed:  Payment received, room reserved for the guest.
/// CheckedIn:  Guest has arrived and checked in.
/// Completed:  Stay finished, guest checked out.
/// Cancelled:  Booking cancelled (from Pending or Confirmed states).
/// NoShow:     Guest did not arrive by the check-in deadline.
/// Refunded:   Cancelled booking with refund processed.
/// </summary>
public enum BookingStatus
{
    /// <summary>Reservation created, awaiting payment confirmation.</summary>
    Pending = 0,

    /// <summary>Payment confirmed, room reserved.</summary>
    Confirmed = 1,

    /// <summary>Guest has arrived and checked in.</summary>
    CheckedIn = 2,

    /// <summary>Stay completed, guest checked out.</summary>
    Completed = 3,

    /// <summary>Booking cancelled before check-in.</summary>
    Cancelled = 4,

    /// <summary>Guest did not arrive by the check-in deadline.</summary>
    NoShow = 5,

    /// <summary>Cancellation with refund processed.</summary>
    Refunded = 6
}
