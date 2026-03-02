namespace StayHub.Shared.Guards;

/// <summary>
/// Defensive programming guard clauses.
/// Fail fast on invalid arguments rather than allowing corrupt state.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws if the value is null.
    /// </summary>
    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, $"{parameterName} cannot be null.");

        return value;
    }

    /// <summary>
    /// Throws if the string is null, empty, or whitespace.
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null, empty, or whitespace.", parameterName);

        return value;
    }

    /// <summary>
    /// Throws if the string exceeds the maximum length.
    /// </summary>
    public static string AgainstMaxLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"{parameterName} cannot exceed {maxLength} characters.", parameterName);

        return value;
    }

    /// <summary>
    /// Throws if the value is not within the specified range (inclusive).
    /// </summary>
    public static T AgainstOutOfRange<T>(T value, T min, T max, string parameterName)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName,
                $"{parameterName} must be between {min} and {max}.");

        return value;
    }

    /// <summary>
    /// Throws if the value is negative.
    /// </summary>
    public static decimal AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} cannot be negative.");

        return value;
    }

    /// <summary>
    /// Throws if the value is zero or negative.
    /// </summary>
    public static decimal AgainstNegativeOrZero(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than zero.");

        return value;
    }

    /// <summary>
    /// Throws if the Guid is empty.
    /// </summary>
    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{parameterName} cannot be an empty GUID.", parameterName);

        return value;
    }

    /// <summary>
    /// Throws if the collection is null or empty.
    /// </summary>
    public static IReadOnlyCollection<T> AgainstNullOrEmpty<T>(
        IReadOnlyCollection<T>? value,
        string parameterName)
    {
        if (value is null || value.Count == 0)
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);

        return value;
    }

    /// <summary>
    /// Throws if the predicate is false.
    /// </summary>
    public static void Against(bool condition, string message)
    {
        if (condition)
            throw new ArgumentException(message);
    }

    /// <summary>
    /// Throws if the date is in the past.
    /// </summary>
    public static DateTime AgainstPastDate(DateTime value, string parameterName)
    {
        if (value.Date < DateTime.UtcNow.Date)
            throw new ArgumentException($"{parameterName} cannot be in the past.", parameterName);

        return value;
    }

    /// <summary>
    /// Throws if the end date is before or equal to the start date.
    /// </summary>
    public static void AgainstInvalidDateRange(DateTime startDate, DateTime endDate, string parameterName)
    {
        if (endDate <= startDate)
            throw new ArgumentException(
                $"End date must be after start date for {parameterName}.", parameterName);
    }
}
