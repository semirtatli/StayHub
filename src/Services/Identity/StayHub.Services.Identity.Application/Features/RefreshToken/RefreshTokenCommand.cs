using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a valid refresh token.
///
/// Token rotation flow:
/// 1. Client sends expired access token + valid refresh token
/// 2. Server validates refresh token (not expired, not revoked)
/// 3. Old refresh token is revoked, new pair is issued
/// 4. If reuse of revoked token detected → entire token family revoked (security breach)
/// </summary>
public sealed record RefreshTokenCommand(
    string Token,
    string IpAddress) : ICommand<AuthenticationResult>;
