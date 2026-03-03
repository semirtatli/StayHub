using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;

namespace StayHub.Shared.Web.Resilience;

/// <summary>
/// Extension methods for adding Polly resilience (retry + circuit breaker) to named/typed HttpClients.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds a standard retry policy (3 retries with exponential backoff)
    /// and a circuit breaker (breaks after 5 failures for 30 seconds).
    /// Attach to any <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static IHttpClientBuilder AddStandardResilience(this IHttpClientBuilder builder)
    {
        return builder
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx + 408 (RequestTimeout)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // 2s, 4s, 8s
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500))); // jitter
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
