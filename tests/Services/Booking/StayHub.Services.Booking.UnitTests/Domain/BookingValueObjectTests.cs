using FluentAssertions;
using StayHub.Services.Booking.Domain.ValueObjects;

namespace StayHub.Services.Booking.UnitTests.Domain;

public class BookingValueObjectTests
{
    // ── Money ───────────────────────────────────────────────────────────

    [Fact]
    public void Money_Create_ValidParams_ShouldWork()
    {
        var money = Money.Create(99.99m, "USD");

        money.Amount.Should().Be(99.99m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Create_NegativeAmount_ShouldThrow()
    {
        var act = () => Money.Create(-1m, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_Create_InvalidCurrency_ShouldThrow()
    {
        var act = () => Money.Create(100m, "US");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_Create_NormalizesToUpperCase()
    {
        var money = Money.Create(100m, "usd");
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Add_SameCurrency_ShouldAdd()
    {
        var a = Money.Create(100m, "USD");
        var b = Money.Create(50.75m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(150.75m);
    }

    [Fact]
    public void Money_Add_DifferentCurrency_ShouldThrow()
    {
        var a = Money.Create(100m, "USD");
        var b = Money.Create(50m, "EUR");

        var act = () => a.Add(b);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Money_Subtract_ShouldWork()
    {
        var a = Money.Create(100m, "USD");
        var b = Money.Create(30m, "USD");

        var result = a.Subtract(b);

        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Money_Multiply_ByInt_ShouldWork()
    {
        var money = Money.Create(25m, "USD");
        var result = money.Multiply(4);
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Money_Multiply_ByDecimal_ShouldRound()
    {
        var money = Money.Create(100m, "USD");
        var result = money.Multiply(0.333m);
        result.Amount.Should().Be(33.30m); // rounded to 2 decimal places
    }

    [Fact]
    public void Money_ToString_ShouldFormat()
    {
        var money = Money.Create(99.99m, "USD");
        money.ToString().Should().Be("99.99 USD");
    }

    // ── StayPeriod ──────────────────────────────────────────────────────

    [Fact]
    public void StayPeriod_Create_ValidDates_ShouldWork()
    {
        var checkIn = new DateOnly(2026, 6, 1);
        var checkOut = new DateOnly(2026, 6, 5);

        var period = StayPeriod.Create(checkIn, checkOut);

        period.CheckIn.Should().Be(checkIn);
        period.CheckOut.Should().Be(checkOut);
        period.Nights.Should().Be(4);
    }

    [Fact]
    public void StayPeriod_Create_CheckInAfterCheckOut_ShouldThrow()
    {
        var checkIn = new DateOnly(2026, 6, 5);
        var checkOut = new DateOnly(2026, 6, 1);

        var act = () => StayPeriod.Create(checkIn, checkOut);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StayPeriod_Create_SameDates_ShouldThrow()
    {
        var date = new DateOnly(2026, 6, 1);

        var act = () => StayPeriod.Create(date, date);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void StayPeriod_OneNightStay_ShouldBeOneNight()
    {
        var period = StayPeriod.Create(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 2));

        period.Nights.Should().Be(1);
    }

    // ── GuestInfo ───────────────────────────────────────────────────────

    [Fact]
    public void GuestInfo_Create_ValidParams_ShouldWork()
    {
        var guest = GuestInfo.Create("John", "Doe", "john@example.com", "+1-555-1234");

        guest.FirstName.Should().Be("John");
        guest.LastName.Should().Be("Doe");
        guest.Email.Should().Be("john@example.com");
        guest.Phone.Should().Be("+1-555-1234");
        guest.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void GuestInfo_Create_WithEmptyFirstName_ShouldThrow()
    {
        var act = () => GuestInfo.Create("", "Doe", "john@example.com");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GuestInfo_Create_WithEmptyEmail_ShouldThrow()
    {
        var act = () => GuestInfo.Create("John", "Doe", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GuestInfo_Create_NormalizesEmail()
    {
        var guest = GuestInfo.Create("John", "Doe", "John@Example.COM");

        guest.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void GuestInfo_Create_TrimsWhitespace()
    {
        var guest = GuestInfo.Create("  John  ", "  Doe  ", "john@example.com");

        guest.FirstName.Should().Be("John");
        guest.LastName.Should().Be("Doe");
    }

    // ── PriceBreakdown ──────────────────────────────────────────────────

    [Fact]
    public void PriceBreakdown_Calculate_ShouldComputeCorrectly()
    {
        var nightlyRate = Money.Create(100m, "USD");

        var breakdown = PriceBreakdown.Calculate(nightlyRate, 3);

        breakdown.NightlyRate.Amount.Should().Be(100m);
        breakdown.Nights.Should().Be(3);
        breakdown.Subtotal.Amount.Should().Be(300m);          // 100 * 3
        breakdown.TaxAmount.Amount.Should().Be(30m);           // 300 * 0.10
        breakdown.ServiceFee.Amount.Should().Be(15m);          // 300 * 0.05
        breakdown.Total.Amount.Should().Be(345m);              // 300 + 30 + 15
    }

    [Fact]
    public void PriceBreakdown_Calculate_CustomRates_ShouldWork()
    {
        var nightlyRate = Money.Create(200m, "EUR");

        var breakdown = PriceBreakdown.Calculate(nightlyRate, 2, taxRate: 0.20m, serviceFeeRate: 0.10m);

        breakdown.Subtotal.Amount.Should().Be(400m);          // 200 * 2
        breakdown.TaxAmount.Amount.Should().Be(80m);           // 400 * 0.20
        breakdown.ServiceFee.Amount.Should().Be(40m);          // 400 * 0.10
        breakdown.Total.Amount.Should().Be(520m);              // 400 + 80 + 40
    }

    [Fact]
    public void PriceBreakdown_Calculate_ZeroNights_ShouldThrow()
    {
        var nightlyRate = Money.Create(100m, "USD");

        var act = () => PriceBreakdown.Calculate(nightlyRate, 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PriceBreakdown_Calculate_SingleNight_ShouldWork()
    {
        var nightlyRate = Money.Create(50m, "USD");

        var breakdown = PriceBreakdown.Calculate(nightlyRate, 1);

        breakdown.Nights.Should().Be(1);
        breakdown.Subtotal.Amount.Should().Be(50m);
        breakdown.Total.Amount.Should().Be(57.5m); // 50 + 5 + 2.5
    }
}
