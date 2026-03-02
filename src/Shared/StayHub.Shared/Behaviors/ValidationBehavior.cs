using FluentValidation;
using MediatR;
using StayHub.Shared.Result;

namespace StayHub.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators
/// before the request reaches its handler.
///
/// How it works:
/// 1. MediatR dispatches a command/query
/// 2. This behavior intercepts it BEFORE the handler
/// 3. Finds all registered IValidator&lt;TRequest&gt; (via DI)
/// 4. Runs them all in parallel
/// 5. If any fail → returns ValidationResult with all errors (handler never runs)
/// 6. If all pass → calls next() to proceed to the handler
///
/// This eliminates manual validation in handlers — every command/query
/// gets validated automatically if a validator class exists.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result.Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        // Run all validators in parallel for performance
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(
                failure.PropertyName,
                failure.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
        {
            return CreateValidationResult<TResponse>(errors);
        }

        return await next();
    }

    /// <summary>
    /// Creates the appropriate ValidationResult type based on TResponse.
    /// Handles both Result (non-generic) and Result&lt;T&gt; (generic) responses.
    /// </summary>
    private static TResponse CreateValidationResult<T>(Error[] errors)
    {
        // If TResponse is Result (non-generic command)
        if (typeof(T) == typeof(Result.Result))
        {
            return (TResponse)(object)ValidationResult.WithErrors(errors);
        }

        // If TResponse is Result<T> (generic command/query)
        // We need to create ValidationResult<TValue>.WithErrors(errors)
        // where TValue is the type argument of Result<TValue>
        var resultType = typeof(T).GetGenericArguments()[0];
        var validationResultType = typeof(ValidationResult<>).MakeGenericType(resultType);

        var withErrorsMethod = validationResultType.GetMethod(
            nameof(ValidationResult.WithErrors),
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;

        return (TResponse)withErrorsMethod.Invoke(null, [errors])!;
    }
}
