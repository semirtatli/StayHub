using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.RevokeToken;

/// <summary>
/// Handles refresh token revocation — used for logout.
/// Delegates to IIdentityService which marks the token as revoked in the database.
/// </summary>
public sealed class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IIdentityService identityService,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RevokeTokenCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token revocation request from {IpAddress}", request.IpAddress);

        var result = await _identityService.RevokeRefreshTokenAsync(
            request.Token,
            request.IpAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Token revocation failed: {ErrorCode}", result.Error.Code);
        }

        return result;
    }
}
