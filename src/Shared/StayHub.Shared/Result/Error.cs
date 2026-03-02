namespace StayHub.Shared.Result;

/// <summary>
/// Represents a typed error with a code and message.
/// Error codes follow the pattern: "{Domain}.{Operation}" (e.g., "Hotel.NotFound").
/// </summary>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error — used for successful results.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// A null/empty value was provided where one was required.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A required value was not provided.");

    /// <summary>
    /// Generic validation error.
    /// </summary>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>
    /// Entity not found.
    /// </summary>
    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with ID '{id}' was not found.");

    /// <summary>
    /// Conflict — operation cannot proceed due to current state.
    /// </summary>
    public static Error Conflict(string code, string message) => new(code, message);

    /// <summary>
    /// Unauthorized access.
    /// </summary>
    public static Error Unauthorized(string message = "You are not authorized to perform this action.") =>
        new("Error.Unauthorized", message);

    /// <summary>
    /// Forbidden — authenticated but lacks permission.
    /// </summary>
    public static Error Forbidden(string message = "You do not have permission to perform this action.") =>
        new("Error.Forbidden", message);
}
