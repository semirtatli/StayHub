namespace StayHub.Services.Hotel.Domain.Enums;

/// <summary>
/// Predefined cancellation policy types for hotel bookings.
///
/// Each type represents a common industry pattern:
/// - Flexible: free cancellation close to check-in
/// - Moderate: free cancellation with more lead time
/// - Strict: partial refund only with significant lead time
/// - NonRefundable: no refund at any point
/// </summary>
public enum CancellationPolicyType
{
    /// <summary>Free cancellation up to 1 day before check-in.</summary>
    Flexible = 0,

    /// <summary>Free cancellation up to 5 days before check-in.</summary>
    Moderate = 1,

    /// <summary>50% refund up to 7 days before check-in, no refund after.</summary>
    Strict = 2,

    /// <summary>No refund at any point after booking confirmation.</summary>
    NonRefundable = 3
}
