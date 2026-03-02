using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.RevokeToken;

/// <summary>
/// Command to revoke a refresh token — used for logout or security invalidation.
/// After revocation, the token can no longer be used to obtain new access tokens.
/// </summary>
public sealed record RevokeTokenCommand(
    string Token,
    string IpAddress) : ICommand;
