using FluentAssertions;
using StayHub.Services.Booking.Domain.Entities;
using StayHub.Services.Booking.Domain.Enums;
using StayHub.Services.Booking.Domain.ValueObjects;

namespace StayHub.Services.Booking.UnitTests.Domain;

public class BookingEntityTests
{
    private static GuestInfo CreateTestGuest() =>
        GuestInfo.Create("John", "Doe", "john@example.com", "+1-555-1234");

    private static PriceBreakdown CreateTestPriceBreakdown(int nights = 2) =>
        PriceBreakdown.Calculate(Money.Create(100m, "USD"), nights);

    // ── Factory ─────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidParams_ShouldCreatePendingBooking()
    {
        var booking = BookingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "guest-1",
            "Grand Hotel",
            "Deluxe Room",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            2,
            CreateTestGuest(),
            CreateTestPriceBreakdown());

        booking.Status.Should().Be(BookingStatus.Pending);
        booking.PaymentStatus.Should().Be(PaymentStatus.Pending);
        booking.HotelName.Should().Be("Grand Hotel");
        booking.RoomName.Should().Be("Deluxe Room");
        booking.NumberOfGuests.Should().Be(2);
        booking.ConfirmationNumber.Should().StartWith("STH-");
        booking.CanCancel.Should().BeTrue();
        booking.IsActive.Should().BeTrue();
        booking.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithZeroGuests_ShouldThrow()
    {
        var act = () => BookingEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "guest-1", "Hotel", "Room",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            0, CreateTestGuest(), CreateTestPriceBreakdown());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyGuestUserId_ShouldThrow()
    {
        var act = () => BookingEntity.Create(
            Guid.NewGuid(), Guid.NewGuid(), "", "Hotel", "Room",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            1, CreateTestGuest(), CreateTestPriceBreakdown());

        act.Should().Throw<ArgumentException>();
    }

    // ── Confirm ─────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_FromPending_ShouldTransitionToConfirmed()
    {
        var booking = CreatePendingBooking();

        booking.Confirm();

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void Confirm_FromConfirmed_ShouldThrow()
    {
        var booking = CreatePendingBooking();
        booking.Confirm();

        var act = () => booking.Confirm();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── CheckIn ─────────────────────────────────────────────────────────

    [Fact]
    public void CheckIn_FromConfirmed_ShouldTransitionToCheckedIn()
    {
        var booking = CreateConfirmedBooking();

        booking.CheckIn();

        booking.Status.Should().Be(BookingStatus.CheckedIn);
        booking.CheckedInAt.Should().NotBeNull();
    }

    [Fact]
    public void CheckIn_FromPending_ShouldThrow()
    {
        var booking = CreatePendingBooking();

        var act = () => booking.CheckIn();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Complete ─────────────────────────────────────────────────────────

    [Fact]
    public void Complete_FromCheckedIn_ShouldTransitionToCompleted()
    {
        var booking = CreateConfirmedBooking();
        booking.CheckIn();

        booking.Complete();

        booking.Status.Should().Be(BookingStatus.Completed);
        booking.CompletedAt.Should().NotBeNull();
        booking.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Complete_FromConfirmed_ShouldThrow()
    {
        var booking = CreateConfirmedBooking();

        var act = () => booking.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Cancel ──────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromPending_ShouldTransitionToCancelled()
    {
        var booking = CreatePendingBooking();

        booking.Cancel();

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancelledAt.Should().NotBeNull();
        booking.CanCancel.Should().BeFalse();
    }

    [Fact]
    public void Cancel_FromConfirmed_WithReason_ShouldTransitionToCancelled()
    {
        var booking = CreateConfirmedBooking();

        booking.Cancel("Changed plans", 50);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancellationReason.Should().Be("Changed plans");
        booking.RefundPercentage.Should().Be(50);
        booking.RefundAmount.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_FromConfirmed_WithoutReason_ShouldThrow()
    {
        var booking = CreateConfirmedBooking();

        var act = () => booking.Cancel();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cancel_FromCheckedIn_ShouldThrow()
    {
        var booking = CreateConfirmedBooking();
        booking.CheckIn();

        var act = () => booking.Cancel("reason");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_RefundAmountCalculation_ShouldBeCorrect()
    {
        var booking = CreateConfirmedBooking();

        booking.Cancel("Changed plans", 100);

        // Total = nightlyRate(100) * 2nights * 1.15 (tax + fee) = 230
        booking.RefundAmount!.Amount.Should().Be(booking.PriceBreakdown.Total.Amount);
        booking.RefundPercentage.Should().Be(100);
    }

    // ── NoShow ──────────────────────────────────────────────────────────

    [Fact]
    public void MarkNoShow_FromConfirmed_ShouldTransitionToNoShow()
    {
        var booking = CreateConfirmedBooking();

        booking.MarkNoShow();

        booking.Status.Should().Be(BookingStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_FromPending_ShouldThrow()
    {
        var booking = CreatePendingBooking();

        var act = () => booking.MarkNoShow();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Refunded ────────────────────────────────────────────────────────

    [Fact]
    public void MarkRefunded_FromCancelled_ShouldTransitionToRefunded()
    {
        var booking = CreatePendingBooking();
        booking.Cancel();

        booking.MarkRefunded();

        booking.Status.Should().Be(BookingStatus.Refunded);
        booking.PaymentStatus.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void MarkRefunded_FromConfirmed_ShouldThrow()
    {
        var booking = CreateConfirmedBooking();

        var act = () => booking.MarkRefunded();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Payment ─────────────────────────────────────────────────────────

    [Fact]
    public void MarkPaymentProcessing_ShouldSetPaymentIntentId()
    {
        var booking = CreatePendingBooking();

        booking.MarkPaymentProcessing("pi_1234");

        booking.PaymentStatus.Should().Be(PaymentStatus.Processing);
        booking.PaymentIntentId.Should().Be("pi_1234");
    }

    [Fact]
    public void MarkPaymentFailed_ShouldSetPaymentFailed()
    {
        var booking = CreatePendingBooking();

        booking.MarkPaymentFailed();

        booking.PaymentStatus.Should().Be(PaymentStatus.Failed);
        booking.Status.Should().Be(BookingStatus.Pending); // Keeps pending for retry
    }

    // ── Full Lifecycle ──────────────────────────────────────────────────

    [Fact]
    public void FullLifecycle_PendingToCompleted_ShouldWork()
    {
        var booking = CreatePendingBooking();

        booking.MarkPaymentProcessing("pi_test");
        booking.Confirm();
        booking.CheckIn();
        booking.Complete();

        booking.Status.Should().Be(BookingStatus.Completed);
        booking.PaymentStatus.Should().Be(PaymentStatus.Paid);
        booking.CompletedAt.Should().NotBeNull();
        booking.CheckedInAt.Should().NotBeNull();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static BookingEntity CreatePendingBooking() =>
        BookingEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "guest-1",
            "Grand Hotel",
            "Deluxe Room",
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            2,
            CreateTestGuest(),
            CreateTestPriceBreakdown());

    private static BookingEntity CreateConfirmedBooking()
    {
        var booking = CreatePendingBooking();
        booking.Confirm();
        return booking;
    }
}
