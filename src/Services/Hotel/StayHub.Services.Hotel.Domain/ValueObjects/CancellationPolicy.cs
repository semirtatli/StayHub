using StayHub.Services.Hotel.Domain.Enums;

namespace StayHub.Services.Hotel.Domain.ValueObjects;

/// <summary>
/// Cancellation policy configuration for a hotel.
///
/// Defines the rules for calculating refunds when a guest cancels:
/// - FreeCancellationDays: guest gets 100% refund if cancelling this many days before check-in
/// - PartialRefundPercentage: refund percentage if cancelling within the partial window
/// - PartialRefundDays: the window boundary for partial refunds (between free and no-refund)
///
/// A cancellation earlier than FreeCancellationDays → 100% refund.
/// Between PartialRefundDays and FreeCancellationDays → PartialRefundPercentage refund.
/// Less than PartialRefundDays → 0% refund.
///
/// Example "Moderate" policy:
///   FreeCancellationDays = 5, PartialRefundDays = 2, PartialRefundPercentage = 50
///   - Cancel 6+ days before → 100%
///   - Cancel 2-5 days before → 50%
///   - Cancel &lt;2 days before → 0%
/// </summary>
public sealed record CancellationPolicy
{
    /// <summary>Policy type for display/categorization.</summary>
    public CancellationPolicyType PolicyType { get; }

    /// <summary>Days before check-in for free (100%) cancellation.</summary>
    public int FreeCancellationDays { get; }

    /// <summary>Percentage refunded in the partial refund window (0-100).</summary>
    public int PartialRefundPercentage { get; }

    /// <summary>Days before check-in boundary for partial refund. Below this = no refund.</summary>
    public int PartialRefundDays { get; }

    private CancellationPolicy(
        CancellationPolicyType policyType,
        int freeCancellationDays,
        int partialRefundPercentage,
        int partialRefundDays)
    {
        PolicyType = policyType;
        FreeCancellationDays = freeCancellationDays;
        PartialRefundPercentage = partialRefundPercentage;
        PartialRefundDays = partialRefundDays;
    }

    /// <summary>
    /// Create a cancellation policy with validation.
    /// </summary>
    public static CancellationPolicy Create(
        CancellationPolicyType policyType,
        int freeCancellationDays,
        int partialRefundPercentage,
        int partialRefundDays)
    {
        if (freeCancellationDays < 0)
            throw new ArgumentException(
                "Free cancellation days cannot be negative.", nameof(freeCancellationDays));

        if (partialRefundPercentage is < 0 or > 100)
            throw new ArgumentException(
                "Partial refund percentage must be between 0 and 100.", nameof(partialRefundPercentage));

        if (partialRefundDays < 0)
            throw new ArgumentException(
                "Partial refund days cannot be negative.", nameof(partialRefundDays));

        if (partialRefundDays > freeCancellationDays)
            throw new ArgumentException(
                "Partial refund days cannot exceed free cancellation days.", nameof(partialRefundDays));

        return new CancellationPolicy(policyType, freeCancellationDays, partialRefundPercentage, partialRefundDays);
    }

    /// <summary>
    /// Create a policy from a predefined type with sensible defaults.
    /// </summary>
    public static CancellationPolicy FromType(CancellationPolicyType policyType) => policyType switch
    {
        CancellationPolicyType.Flexible => new CancellationPolicy(policyType, 1, 50, 0),
        CancellationPolicyType.Moderate => new CancellationPolicy(policyType, 5, 50, 2),
        CancellationPolicyType.Strict => new CancellationPolicy(policyType, 7, 50, 3),
        CancellationPolicyType.NonRefundable => new CancellationPolicy(policyType, 0, 0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(policyType))
    };

    /// <summary>
    /// Calculate the refund percentage based on how many days before check-in.
    /// </summary>
    /// <param name="daysBeforeCheckIn">Number of days between cancellation and check-in.</param>
    /// <returns>Refund percentage 0-100.</returns>
    public int CalculateRefundPercentage(int daysBeforeCheckIn)
    {
        if (PolicyType == CancellationPolicyType.NonRefundable)
            return 0;

        if (daysBeforeCheckIn >= FreeCancellationDays)
            return 100;

        if (daysBeforeCheckIn >= PartialRefundDays)
            return PartialRefundPercentage;

        return 0;
    }

    // EF Core parameterless constructor
    private CancellationPolicy()
        : this(CancellationPolicyType.Flexible, 1, 50, 0) { }
}
