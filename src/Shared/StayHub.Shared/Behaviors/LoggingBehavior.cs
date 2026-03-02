using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Shared.Result;

namespace StayHub.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs every request with timing information.
///
/// Logs:
/// - Request name, parameters (at Debug level), and timestamp on entry
/// - Response status (success/failure), duration on exit
/// - Warning if request takes longer than 500ms (potential performance issue)
/// - Error details if the result indicates failure
///
/// Uses structured logging (Serilog under the hood) so all fields are
/// searchable in Seq. The {@Request} syntax preserves the object structure.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result.Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private const int SlowRequestThresholdMs = 500;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Handling {RequestName}",
            requestName);

        _logger.LogDebug(
            "Handling {RequestName} with {@Request}",
            requestName,
            request);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request: {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                requestName,
                elapsedMs,
                SlowRequestThresholdMs);
        }

        if (response.IsFailure)
        {
            _logger.LogWarning(
                "Request {RequestName} failed with error {ErrorCode}: {ErrorMessage} ({ElapsedMs}ms)",
                requestName,
                response.Error.Code,
                response.Error.Message,
                elapsedMs);
        }
        else
        {
            _logger.LogInformation(
                "Handled {RequestName} successfully ({ElapsedMs}ms)",
                requestName,
                elapsedMs);
        }

        return response;
    }
}
