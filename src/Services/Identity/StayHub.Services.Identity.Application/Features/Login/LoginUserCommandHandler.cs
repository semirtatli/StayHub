using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.Login;

/// <summary>
/// Handles user login — delegates to IIdentityService for credential verification,
/// JWT generation, and refresh token rotation.
/// </summary>
public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthenticationResult>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IIdentityService identityService,
        ILogger<LoginUserCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        var result = await _identityService.AuthenticateAsync(
            request.Email,
            request.Password,
            request.IpAddress,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Login failed for {Email}: {ErrorCode}", request.Email, result.Error.Code);
        }

        return result;
    }
}
