using FluentAssertions;
using StayHub.Services.Payment.Domain.Entities;
using StayHub.Services.Payment.Domain.Enums;

namespace StayHub.Services.Payment.UnitTests.Domain;

public class PaymentEntityTests
{
    // ── Factory ─────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidParams_ShouldCreatePendingPayment()
    {
        var payment = CreatePendingPayment();

        payment.BookingId.Should().NotBeEmpty();
        payment.UserId.Should().Be("user-1");
        payment.Amount.Amount.Should().Be(250m);
        payment.Amount.Currency.Should().Be("USD");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Method.Should().Be(PaymentMethod.CreditCard);
        payment.RefundedAmount.Amount.Should().Be(0);
        payment.ProviderTransactionId.Should().BeNull();
        payment.ClientSecret.Should().BeNull();
        payment.PaidAt.Should().BeNull();
        payment.FailedAt.Should().BeNull();
        payment.CancelledAt.Should().BeNull();
        payment.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => PaymentEntity.Create(
            Guid.NewGuid(), "user-1", 0m, "USD", PaymentMethod.CreditCard);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => PaymentEntity.Create(
            Guid.NewGuid(), "user-1", -10m, "USD", PaymentMethod.CreditCard);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrow()
    {
        var act = () => PaymentEntity.Create(
            Guid.NewGuid(), "", 100m, "USD", PaymentMethod.CreditCard);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrow()
    {
        var act = () => PaymentEntity.Create(
            Guid.NewGuid(), "user-1", 100m, "", PaymentMethod.CreditCard);

        act.Should().Throw<ArgumentException>();
    }

    // ── MarkAsProcessing ────────────────────────────────────────────────

    [Fact]
    public void MarkAsProcessing_FromPending_ShouldTransitionToProcessing()
    {
        var payment = CreatePendingPayment();

        payment.MarkAsProcessing("pi_123", "cs_secret");

        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.ProviderTransactionId.Should().Be("pi_123");
        payment.ClientSecret.Should().Be("cs_secret");
    }

    [Fact]
    public void MarkAsProcessing_FromProcessing_ShouldThrow()
    {
        var payment = CreateProcessingPayment();

        var act = () => payment.MarkAsProcessing("pi_456");

        act.Should().Throw<InvalidOperationException>();
    }

    // ── MarkAsSucceeded (Complete) ──────────────────────────────────────

    [Fact]
    public void MarkAsSucceeded_FromProcessing_ShouldTransitionToSucceeded()
    {
        var payment = CreateProcessingPayment();

        payment.MarkAsSucceeded("pi_123");

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSucceeded_FromPending_ShouldTransitionToSucceeded()
    {
        var payment = CreatePendingPayment();

        payment.MarkAsSucceeded("pi_123");

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSucceeded_FromSucceeded_ShouldThrow()
    {
        var payment = CreateSucceededPayment();

        var act = () => payment.MarkAsSucceeded("pi_456");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsSucceeded_FromFailed_ShouldThrow()
    {
        var payment = CreateProcessingPayment();
        payment.MarkAsFailed("Card declined");

        var act = () => payment.MarkAsSucceeded("pi_456");

        act.Should().Throw<InvalidOperationException>();
    }

    // ── MarkAsFailed ────────────────────────────────────────────────────

    [Fact]
    public void MarkAsFailed_FromProcessing_ShouldTransitionToFailed()
    {
        var payment = CreateProcessingPayment();

        payment.MarkAsFailed("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Insufficient funds");
        payment.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_FromPending_ShouldTransitionToFailed()
    {
        var payment = CreatePendingPayment();

        payment.MarkAsFailed("Card declined");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Card declined");
    }

    [Fact]
    public void MarkAsFailed_FromSucceeded_ShouldThrow()
    {
        var payment = CreateSucceededPayment();

        var act = () => payment.MarkAsFailed("Some reason");

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Cancel ──────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromPending_ShouldTransitionToCancelled()
    {
        var payment = CreatePendingPayment();

        payment.Cancel();

        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_FromProcessing_ShouldThrow()
    {
        var payment = CreateProcessingPayment();

        var act = () => payment.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromSucceeded_ShouldThrow()
    {
        var payment = CreateSucceededPayment();

        var act = () => payment.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Refund ──────────────────────────────────────────────────────────

    [Fact]
    public void ProcessRefund_FullRefund_FromSucceeded_ShouldTransitionToFullyRefunded()
    {
        var payment = CreateSucceededPayment();

        payment.ProcessRefund(250m);

        payment.Status.Should().Be(PaymentStatus.FullyRefunded);
        payment.RefundedAmount.Amount.Should().Be(250m);
        payment.CanRefund.Should().BeFalse();
        payment.RefundableAmount.Should().Be(0);
    }

    [Fact]
    public void ProcessRefund_PartialRefund_FromSucceeded_ShouldTransitionToPartiallyRefunded()
    {
        var payment = CreateSucceededPayment();

        payment.ProcessRefund(100m);

        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        payment.RefundedAmount.Amount.Should().Be(100m);
        payment.CanRefund.Should().BeTrue();
        payment.RefundableAmount.Should().Be(150m);
    }

    [Fact]
    public void ProcessRefund_MultiplePartialRefunds_ShouldAccumulate()
    {
        var payment = CreateSucceededPayment();

        payment.ProcessRefund(100m);
        payment.ProcessRefund(150m);

        payment.Status.Should().Be(PaymentStatus.FullyRefunded);
        payment.RefundedAmount.Amount.Should().Be(250m);
        payment.CanRefund.Should().BeFalse();
    }

    [Fact]
    public void ProcessRefund_ExceedingAmount_ShouldThrow()
    {
        var payment = CreateSucceededPayment();

        var act = () => payment.ProcessRefund(300m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProcessRefund_FromPending_ShouldThrow()
    {
        var payment = CreatePendingPayment();

        var act = () => payment.ProcessRefund(50m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProcessRefund_FromFailed_ShouldThrow()
    {
        var payment = CreateProcessingPayment();
        payment.MarkAsFailed("Declined");

        var act = () => payment.ProcessRefund(50m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProcessRefund_FromCancelled_ShouldThrow()
    {
        var payment = CreatePendingPayment();
        payment.Cancel();

        var act = () => payment.ProcessRefund(50m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ProcessRefund_ZeroAmount_ShouldThrow()
    {
        var payment = CreateSucceededPayment();

        var act = () => payment.ProcessRefund(0m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── CanRefund / RefundableAmount ────────────────────────────────────

    [Fact]
    public void CanRefund_WhenSucceeded_ShouldBeTrue()
    {
        var payment = CreateSucceededPayment();

        payment.CanRefund.Should().BeTrue();
    }

    [Fact]
    public void CanRefund_WhenPending_ShouldBeFalse()
    {
        var payment = CreatePendingPayment();

        payment.CanRefund.Should().BeFalse();
    }

    [Fact]
    public void RefundableAmount_WhenNoRefunds_ShouldEqualFullAmount()
    {
        var payment = CreateSucceededPayment();

        payment.RefundableAmount.Should().Be(250m);
    }

    // ── Full Lifecycle ──────────────────────────────────────────────────

    [Fact]
    public void FullLifecycle_PendingToFullyRefunded_ShouldWork()
    {
        var payment = CreatePendingPayment();

        payment.MarkAsProcessing("pi_test", "cs_test");
        payment.MarkAsSucceeded("pi_test");
        payment.ProcessRefund(250m);

        payment.Status.Should().Be(PaymentStatus.FullyRefunded);
        payment.PaidAt.Should().NotBeNull();
        payment.RefundedAmount.Amount.Should().Be(250m);
        payment.CanRefund.Should().BeFalse();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static PaymentEntity CreatePendingPayment() =>
        PaymentEntity.Create(
            Guid.NewGuid(),
            "user-1",
            250m,
            "USD",
            PaymentMethod.CreditCard);

    private static PaymentEntity CreateProcessingPayment()
    {
        var payment = CreatePendingPayment();
        payment.MarkAsProcessing("pi_123");
        return payment;
    }

    private static PaymentEntity CreateSucceededPayment()
    {
        var payment = CreateProcessingPayment();
        payment.MarkAsSucceeded("pi_123");
        return payment;
    }
}
