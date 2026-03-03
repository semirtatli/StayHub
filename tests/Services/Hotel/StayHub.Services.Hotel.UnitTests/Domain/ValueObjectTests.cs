using FluentAssertions;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.ValueObjects;

namespace StayHub.Services.Hotel.UnitTests.Domain;

public class ValueObjectTests
{
    // ── Address ─────────────────────────────────────────────────────────

    [Fact]
    public void Address_Create_WithValidParams_ShouldCreateAddress()
    {
        var address = Address.Create("123 Main St", "Istanbul", "Istanbul", "Turkey", "34000");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Istanbul");
        address.Country.Should().Be("Turkey");
        address.ZipCode.Should().Be("34000");
    }

    [Fact]
    public void Address_Create_WithEmptyStreet_ShouldThrow()
    {
        var act = () => Address.Create("", "Istanbul", "Istanbul", "Turkey", "34000");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Address_Equality_SameValues_ShouldBeEqual()
    {
        var a1 = Address.Create("123 Main", "Istanbul", "Istanbul", "Turkey", "34000");
        var a2 = Address.Create("123 Main", "Istanbul", "Istanbul", "Turkey", "34000");

        a1.Should().Be(a2);
    }

    [Fact]
    public void Address_Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a1 = Address.Create("123 Main", "Istanbul", "Istanbul", "Turkey", "34000");
        var a2 = Address.Create("456 Other", "Istanbul", "Istanbul", "Turkey", "34000");

        a1.Should().NotBe(a2);
    }

    // ── Money ───────────────────────────────────────────────────────────

    [Fact]
    public void Money_Create_WithValidParams_ShouldCreateMoney()
    {
        var money = Money.Create(100.50m, "USD");

        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Money_Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Money.Create(-1m, "USD");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_Create_WithInvalidCurrency_ShouldThrow()
    {
        var act = () => Money.Create(100m, "INVALID");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Money_Add_SameCurrency_ShouldReturnSum()
    {
        var a = Money.Create(100m, "USD");
        var b = Money.Create(50.25m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(150.25m);
        result.Currency.Should().Be("USD");
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
    public void Money_Multiply_ByInt_ShouldReturnProduct()
    {
        var money = Money.Create(50m, "USD");

        var result = money.Multiply(3);

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Money_Multiply_ByDecimal_ShouldReturnRoundedProduct()
    {
        var money = Money.Create(100m, "USD");

        var result = money.Multiply(0.10m);

        result.Amount.Should().Be(10m);
    }

    [Fact]
    public void Money_Zero_ShouldCreateZeroAmount()
    {
        var money = Money.Zero("EUR");

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    // ── GeoLocation ─────────────────────────────────────────────────────

    [Fact]
    public void GeoLocation_Create_WithValidCoords_ShouldCreate()
    {
        var geo = GeoLocation.Create(41.0082, 28.9784);

        geo.Latitude.Should().Be(41.0082);
        geo.Longitude.Should().Be(28.9784);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    public void GeoLocation_Create_WithInvalidLatitude_ShouldThrow(double lat, double lon)
    {
        var act = () => GeoLocation.Create(lat, lon);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void GeoLocation_Create_WithInvalidLongitude_ShouldThrow(double lat, double lon)
    {
        var act = () => GeoLocation.Create(lat, lon);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── ContactInfo ─────────────────────────────────────────────────────

    [Fact]
    public void ContactInfo_Create_WithValidParams_ShouldCreate()
    {
        var contact = ContactInfo.Create("+90-555-1234567", "hotel@example.com", "https://hotel.com");

        contact.Phone.Should().Be("+90-555-1234567");
        contact.Email.Should().Be("hotel@example.com");
        contact.Website.Should().Be("https://hotel.com");
    }

    [Fact]
    public void ContactInfo_Create_WithInvalidEmail_ShouldThrow()
    {
        var act = () => ContactInfo.Create("+90-555-1234567", "invalid-email");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ContactInfo_Create_WithEmptyPhone_ShouldThrow()
    {
        var act = () => ContactInfo.Create("", "hotel@example.com");

        act.Should().Throw<ArgumentException>();
    }

    // ── CancellationPolicy ──────────────────────────────────────────────

    [Fact]
    public void CancellationPolicy_FromType_Flexible_ShouldHaveDefaultValues()
    {
        var policy = CancellationPolicy.FromType(CancellationPolicyType.Flexible);

        policy.PolicyType.Should().Be(CancellationPolicyType.Flexible);
        policy.FreeCancellationDays.Should().Be(1);
        policy.PartialRefundPercentage.Should().Be(50);
    }

    [Fact]
    public void CancellationPolicy_FromType_NonRefundable_ShouldHaveZeroValues()
    {
        var policy = CancellationPolicy.FromType(CancellationPolicyType.NonRefundable);

        policy.FreeCancellationDays.Should().Be(0);
        policy.PartialRefundPercentage.Should().Be(0);
        policy.PartialRefundDays.Should().Be(0);
    }

    [Theory]
    [InlineData(10, 100)]   // Well before free period → full refund
    [InlineData(5, 100)]    // At free period → full refund
    [InlineData(3, 50)]     // Partial period → partial refund
    [InlineData(1, 0)]      // Below partial period → no refund
    [InlineData(0, 0)]      // Day of → no refund
    public void CancellationPolicy_Moderate_CalculateRefundPercentage(int daysBefore, int expectedRefund)
    {
        var policy = CancellationPolicy.FromType(CancellationPolicyType.Moderate);
        // Moderate: FreeCancellationDays=5, PartialRefundDays=2, PartialRefundPercentage=50

        var refund = policy.CalculateRefundPercentage(daysBefore);

        refund.Should().Be(expectedRefund);
    }

    [Fact]
    public void CancellationPolicy_NonRefundable_AlwaysZero()
    {
        var policy = CancellationPolicy.FromType(CancellationPolicyType.NonRefundable);

        policy.CalculateRefundPercentage(100).Should().Be(0);
        policy.CalculateRefundPercentage(0).Should().Be(0);
    }
}
