using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace StayHub.Shared.Web.Resilience;

/// <summary>
/// Delegating handler that propagates the correlation ID from the current HTTP context
/// to outgoing inter-service HTTP calls.
/// </summary>
public sealed class CorrelationIdDelegatingHandler(
    IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext?.Items["CorrelationId"] is string correlationId)
        {
            request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
        }

        // Propagate JWT token to downstream services
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
