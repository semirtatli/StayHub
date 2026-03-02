using System.Globalization;
using StayHub.Shared.Domain;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with a specific currency.
/// Value object — prevents primitive obsession for monetary values.
///
/// Important: Money arithmetic only works between same-currency instances.
/// Cross-currency operations require explicit conversion via an exchange rate service.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a Money value. Amount must be non-negative.
    /// Currency should be an ISO 4217 code (e.g., USD, EUR, TRY).
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative.", nameof(amount));

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    /// <summary>
    /// Shorthand for zero amount in a given currency.
    /// </summary>
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

    public Money Multiply(decimal factor) =>
        Create(Amount * factor, Currency);

    public bool IsZero => Amount == 0;

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot perform operation on Money with different currencies: {Currency} vs {other.Currency}.");
    }

    public override string ToString() =>
        $"{Amount.ToString("F2", CultureInfo.InvariantCulture)} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    // EF Core parameterless constructor
#pragma warning disable CS8618
    private Money() { }
#pragma warning restore CS8618
}
