using StayHub.Services.Booking.Domain.Enums;
using StayHub.Services.Booking.Domain.Events;
using StayHub.Services.Booking.Domain.ValueObjects;
using StayHub.Shared.Domain;

namespace StayHub.Services.Booking.Domain.Entities;

/// <summary>
/// Booking aggregate root — represents a hotel room reservation.
///
/// Lifecycle state machine:
///   Pending → Confirmed → CheckedIn → Completed
///   Pending → Cancelled
///   Confirmed → Cancelled → Refunded
///   Confirmed → NoShow
///
/// Invariants:
/// - A booking references a single room in a single hotel.
/// - Guest info is a snapshot captured at booking time.
/// - Price breakdown is locked at booking time — immune to future price changes.
/// - State transitions are enforced by the domain entity.
/// - Cancellation reason is required when cancelling a Confirmed booking.
///
/// Uses "BookingEntity" instead of "Booking" to avoid namespace conflict with
/// StayHub.Services.Booking.Domain namespace.
/// </summary>
public sealed class BookingEntity : AggregateRoot
{
    // ── Properties ───────────────────────────────────────────────────────

    /// <summary>Hotel ID (from Hotel Service). Cross-service reference.</summary>
    public Guid HotelId { get; private init; }

    /// <summary>Room ID (from Hotel Service). Cross-service reference.</summary>
    public Guid RoomId { get; private init; }

    /// <summary>Identity Service user ID of the guest who made the booking.</summary>
    public string GuestUserId { get; private init; }

    /// <summary>Hotel name snapshot at booking time for display purposes.</summary>
    public string HotelName { get; private init; }

    /// <summary>Room name snapshot at booking time.</summary>
    public string RoomName { get; private init; }

    /// <summary>Check-in/check-out date range.</summary>
    public StayPeriod StayPeriod { get; private init; }

    /// <summary>Number of guests for this booking.</summary>
    public int NumberOfGuests { get; private set; }

    /// <summary>Guest information snapshot.</summary>
    public GuestInfo GuestInfo { get; private set; }

    /// <summary>Price breakdown locked at booking time.</summary>
    public PriceBreakdown PriceBreakdown { get; private set; }

    /// <summary>Current booking status.</summary>
    public BookingStatus Status { get; private set; }

    /// <summary>Current payment status.</summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>Special requests from the guest (e.g., late check-in, extra bed).</summary>
    public string? SpecialRequests { get; private set; }

    /// <summary>Reason for cancellation (required for Confirmed → Cancelled).</summary>
    public string? CancellationReason { get; private set; }

    /// <summary>When the booking was cancelled.</summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>When the guest checked in.</summary>
    public DateTime? CheckedInAt { get; private set; }

    /// <summary>When the guest checked out / booking completed.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Unique confirmation number for the guest.
    /// Format: STH-{YYYYMMDD}-{SHORT-GUID}
    /// </summary>
    public string ConfirmationNumber { get; private init; }

    /// <summary>
    /// External payment reference (e.g., Stripe PaymentIntent ID).
    /// Set when payment is initiated.
    /// </summary>
    public string? PaymentIntentId { get; private set; }

    // ── Constructors ─────────────────────────────────────────────────────

    private BookingEntity()
    {
        GuestUserId = null!;
        HotelName = null!;
        RoomName = null!;
        StayPeriod = null!;
        GuestInfo = null!;
        PriceBreakdown = null!;
        ConfirmationNumber = null!;
    }

    // ── Factory Method ───────────────────────────────────────────────────

    /// <summary>
    /// Create a new booking. Starts in Pending status with Pending payment.
    /// </summary>
    public static BookingEntity Create(
        Guid hotelId,
        Guid roomId,
        string guestUserId,
        string hotelName,
        string roomName,
        DateOnly checkIn,
        DateOnly checkOut,
        int numberOfGuests,
        GuestInfo guestInfo,
        PriceBreakdown priceBreakdown,
        string? specialRequests = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guestUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(hotelName);
        ArgumentException.ThrowIfNullOrWhiteSpace(roomName);
        ArgumentNullException.ThrowIfNull(guestInfo);
        ArgumentNullException.ThrowIfNull(priceBreakdown);

        if (numberOfGuests <= 0)
            throw new ArgumentException("Number of guests must be at least 1.", nameof(numberOfGuests));

        var stayPeriod = StayPeriod.Create(checkIn, checkOut);
        var confirmationNumber = GenerateConfirmationNumber(checkIn);

        var booking = new BookingEntity
        {
            HotelId = hotelId,
            RoomId = roomId,
            GuestUserId = guestUserId,
            HotelName = hotelName,
            RoomName = roomName,
            StayPeriod = stayPeriod,
            NumberOfGuests = numberOfGuests,
            GuestInfo = guestInfo,
            PriceBreakdown = priceBreakdown,
            SpecialRequests = specialRequests,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            ConfirmationNumber = confirmationNumber
        };

        booking.RaiseDomainEvent(new BookingCreatedEvent(
            booking.Id,
            hotelId,
            roomId,
            guestUserId,
            checkIn,
            checkOut,
            numberOfGuests));

        return booking;
    }

    // ── Status Workflow Methods ──────────────────────────────────────────

    /// <summary>
    /// Mark payment as processing (e.g., 3D Secure initiated).
    /// </summary>
    public void MarkPaymentProcessing(string paymentIntentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentIntentId);

        if (PaymentStatus != PaymentStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot start payment processing when payment status is {PaymentStatus}.");

        PaymentIntentId = paymentIntentId;
        PaymentStatus = PaymentStatus.Processing;
    }

    /// <summary>
    /// Confirm the booking after successful payment.
    /// Transition: Pending → Confirmed.
    /// </summary>
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot confirm booking in {Status} status.");

        var oldStatus = Status;
        Status = BookingStatus.Confirmed;
        PaymentStatus = PaymentStatus.Paid;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, null));
        RaiseDomainEvent(new BookingConfirmedEvent(Id, HotelId, GuestUserId));
    }

    /// <summary>
    /// Mark payment as failed. Keeps booking in Pending for retry.
    /// </summary>
    public void MarkPaymentFailed()
    {
        if (PaymentStatus is not (PaymentStatus.Pending or PaymentStatus.Processing))
            throw new InvalidOperationException(
                $"Cannot mark payment as failed when payment status is {PaymentStatus}.");

        PaymentStatus = PaymentStatus.Failed;
    }

    /// <summary>
    /// Check in the guest.
    /// Transition: Confirmed → CheckedIn.
    /// </summary>
    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot check in when booking status is {Status}.");

        var oldStatus = Status;
        Status = BookingStatus.CheckedIn;
        CheckedInAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, null));
        RaiseDomainEvent(new GuestCheckedInEvent(Id, HotelId, GuestUserId));
    }

    /// <summary>
    /// Complete the booking (guest checks out).
    /// Transition: CheckedIn → Completed.
    /// </summary>
    public void Complete()
    {
        if (Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException(
                $"Cannot complete booking in {Status} status.");

        var oldStatus = Status;
        Status = BookingStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, null));
        RaiseDomainEvent(new BookingCompletedEvent(Id, HotelId, RoomId, GuestUserId));
    }

    /// <summary>
    /// Cancel the booking.
    /// Transition: Pending → Cancelled, Confirmed → Cancelled (requires reason).
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            throw new InvalidOperationException(
                $"Cannot cancel booking in {Status} status.");

        if (Status == BookingStatus.Confirmed && string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "Cancellation reason is required for confirmed bookings.");

        var oldStatus = Status;
        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, reason));
        RaiseDomainEvent(new BookingCancelledEvent(
            Id, HotelId, RoomId,
            StayPeriod.CheckIn, StayPeriod.CheckOut,
            reason));
    }

    /// <summary>
    /// Mark as no-show. Admin/hotel staff action.
    /// Transition: Confirmed → NoShow.
    /// </summary>
    public void MarkNoShow()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot mark as no-show when booking status is {Status}.");

        var oldStatus = Status;
        Status = BookingStatus.NoShow;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, "Guest did not arrive."));
    }

    /// <summary>
    /// Mark the booking as refunded after cancellation.
    /// Transition: Cancelled → Refunded.
    /// </summary>
    public void MarkRefunded()
    {
        if (Status != BookingStatus.Cancelled)
            throw new InvalidOperationException(
                $"Cannot mark as refunded when booking status is {Status}.");

        var oldStatus = Status;
        Status = BookingStatus.Refunded;
        PaymentStatus = PaymentStatus.Refunded;

        RaiseDomainEvent(new BookingStatusChangedEvent(Id, oldStatus, Status, null));
    }

    // ── Query Methods ────────────────────────────────────────────────────

    /// <summary>
    /// Whether this booking can be cancelled.
    /// </summary>
    public bool CanCancel => Status is BookingStatus.Pending or BookingStatus.Confirmed;

    /// <summary>
    /// Whether this booking is in an active state (not terminal).
    /// </summary>
    public bool IsActive => Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn;

    // ── Helper Methods ───────────────────────────────────────────────────

    /// <summary>
    /// Generate a human-readable confirmation number.
    /// Format: STH-YYYYMMDD-XXXX (4 hex chars from GUID for uniqueness).
    /// </summary>
    private static string GenerateConfirmationNumber(DateOnly checkIn)
    {
        var datePart = checkIn.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var guidPart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"STH-{datePart}-{guidPart}";
    }
}
