namespace StayHub.Services.Booking.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// Immutable value object — all operations return new instances.
///
/// Invariants:
/// - Amount must be non-negative.
/// - Currency must be a 3-letter ISO 4217 code.
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

    /// <summary>
    /// Create a Money value with validation.
    /// </summary>
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Create a zero-value Money instance.
    /// </summary>
    public static Money Zero(string currency) => Create(0, currency);

    /// <summary>
    /// Add two monetary amounts. Currencies must match.
    /// </summary>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtract a monetary amount. Currencies must match.
    /// </summary>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiply by a scalar (e.g., number of nights).
    /// </summary>
    public Money Multiply(int factor)
    {
        return Create(Amount * factor, Currency);
    }

    /// <summary>
    /// Multiply by a decimal factor (e.g., tax rate).
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return Create(Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero), Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} and {other.Currency}.");
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    // EF Core requires a parameterless constructor for owned entities
    private Money() : this(0, "USD") { }
}
