using MediatR;
using Microsoft.Extensions.Logging;

namespace StayHub.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that catches unhandled exceptions from handlers.
///
/// This is the outermost behavior in the pipeline (registered first, runs last as a wrapper).
/// It ensures that:
/// 1. All unhandled exceptions are logged with full context (request name, parameters)
/// 2. Exceptions are re-thrown after logging (global exception middleware handles HTTP response)
///
/// This is NOT a swallowing handler — it logs and re-throws. The API layer's
/// global exception middleware (ExceptionHandlingMiddleware) converts exceptions
/// to proper HTTP responses (500, 409 for concurrency, etc.).
///
/// Pipeline order: UnhandledException → Logging → Validation → Transaction → Handler
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(
                ex,
                "Unhandled exception for request {RequestName} ({@Request})",
                requestName,
                request);

            throw;
        }
    }
}
