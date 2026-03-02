namespace StayHub.Shared.Result;

/// <summary>
/// Marker interface for validation results — allows polymorphic access to errors
/// in base controllers without knowing the generic type parameter.
/// </summary>
public interface IValidationResult
{
    Error[] Errors { get; }
}

/// <summary>
/// Represents a validation result containing multiple errors.
/// Returned by the ValidationBehavior when FluentValidation fails.
/// </summary>
public sealed class ValidationResult : Result, IValidationResult
{
    public Error[] Errors { get; }

    private ValidationResult(Error[] errors)
        : base(false, errors.Length > 0 ? errors[0] : Error.Validation("Validation", "Validation failed."))
    {
        Errors = errors;
    }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Generic validation result with value type.
/// </summary>
public sealed class ValidationResult<T> : Result<T>, IValidationResult
{
    public Error[] Errors { get; }

    private ValidationResult(Error[] errors)
        : base(default, false, errors.Length > 0 ? errors[0] : Error.Validation("Validation", "Validation failed."))
    {
        Errors = errors;
    }

    public static ValidationResult<T> WithErrors(Error[] errors) => new(errors);
}
