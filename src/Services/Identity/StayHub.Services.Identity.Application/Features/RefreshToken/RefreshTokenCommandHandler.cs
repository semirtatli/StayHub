using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.RefreshToken;

/// <summary>
/// Handles refresh token rotation — validates the old token, issues a new pair.
/// </summary>
public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refresh token request from {IpAddress}", request.IpAddress);

        var result = await _identityService.RefreshTokenAsync(
            request.Token,
            request.IpAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Token refresh failed: {ErrorCode}", result.Error.Code);
        }

        return result;
    }
}
