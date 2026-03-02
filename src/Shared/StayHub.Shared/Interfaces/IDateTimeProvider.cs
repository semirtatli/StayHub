namespace StayHub.Shared.Interfaces;

/// <summary>
/// Abstraction for date/time operations, enabling testability.
/// Always use this instead of DateTime.UtcNow directly.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Returns the current UTC date/time.
    /// </summary>
    DateTime UtcNow { get; }
}
