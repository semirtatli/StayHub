namespace StayHub.Services.Payment.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// Immutable value object — all operations return new instances.
///
/// Same concept as Booking.Domain.ValueObjects.Money — duplicated per service
/// to maintain bounded context isolation (no shared domain model).
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Create(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
        => Create(Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero), Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} vs {other.Currency}.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    // EF Core requires parameterless constructor
    private Money() : this(0, "USD") { }
}
