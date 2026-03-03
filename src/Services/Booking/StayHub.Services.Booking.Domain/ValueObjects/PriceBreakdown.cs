namespace StayHub.Services.Booking.Domain.ValueObjects;

/// <summary>
/// Price breakdown for a booking.
/// Captures per-night rate, taxes, fees, and total.
/// Stored as a snapshot at booking time — immune to future price changes.
/// </summary>
public sealed record PriceBreakdown
{
    /// <summary>Base rate per night.</summary>
    public Money NightlyRate { get; }

    /// <summary>Number of nights.</summary>
    public int Nights { get; }

    /// <summary>Subtotal (NightlyRate × Nights).</summary>
    public Money Subtotal { get; }

    /// <summary>Tax amount.</summary>
    public Money TaxAmount { get; }

    /// <summary>Service fee charged by the platform.</summary>
    public Money ServiceFee { get; }

    /// <summary>Final total (Subtotal + Tax + ServiceFee).</summary>
    public Money Total { get; }

    private PriceBreakdown(
        Money nightlyRate,
        int nights,
        Money subtotal,
        Money taxAmount,
        Money serviceFee,
        Money total)
    {
        NightlyRate = nightlyRate;
        Nights = nights;
        Subtotal = subtotal;
        TaxAmount = taxAmount;
        ServiceFee = serviceFee;
        Total = total;
    }

    /// <summary>
    /// Calculate a price breakdown from the nightly rate and number of nights.
    /// </summary>
    /// <param name="nightlyRate">Rate per night.</param>
    /// <param name="nights">Number of nights.</param>
    /// <param name="taxRate">Tax rate as a decimal (e.g., 0.10 for 10%).</param>
    /// <param name="serviceFeeRate">Service fee rate (e.g., 0.05 for 5%).</param>
    public static PriceBreakdown Calculate(
        Money nightlyRate,
        int nights,
        decimal taxRate = 0.10m,
        decimal serviceFeeRate = 0.05m)
    {
        if (nights <= 0)
            throw new ArgumentException("Number of nights must be positive.", nameof(nights));

        var subtotal = nightlyRate.Multiply(nights);
        var taxAmount = subtotal.Multiply(taxRate);
        var serviceFee = subtotal.Multiply(serviceFeeRate);
        var total = subtotal.Add(taxAmount).Add(serviceFee);

        return new PriceBreakdown(nightlyRate, nights, subtotal, taxAmount, serviceFee, total);
    }

    // EF Core requires a parameterless constructor for owned entities
    private PriceBreakdown()
        : this(
            Money.Create(0, "USD"),
            0,
            Money.Create(0, "USD"),
            Money.Create(0, "USD"),
            Money.Create(0, "USD"),
            Money.Create(0, "USD"))
    { }
}
