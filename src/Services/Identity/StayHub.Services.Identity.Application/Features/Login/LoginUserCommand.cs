using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.Login;

/// <summary>
/// Command to authenticate a user with email and password.
/// Returns JWT access token + refresh token on success.
///
/// Flow: Controller → Validation → Handler → IIdentityService.AuthenticateAsync
///       → JWT generation, refresh token stored in DB, domain event published.
/// </summary>
public sealed record LoginUserCommand(
    string Email,
    string Password,
    string IpAddress) : ICommand<AuthenticationResult>;
