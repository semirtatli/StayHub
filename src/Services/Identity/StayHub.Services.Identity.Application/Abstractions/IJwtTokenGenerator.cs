namespace StayHub.Services.Identity.Application.Abstractions;

/// <summary>
/// Abstraction for JWT token generation.
/// Implemented in Infrastructure — keeps crypto/signing logic out of Application layer.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generate a signed JWT access token with user claims.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="role">The user's role.</param>
    /// <returns>Signed JWT string and its expiration time.</returns>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(string userId, string email, string role);

    /// <summary>
    /// Generate a cryptographically random refresh token string.
    /// </summary>
    string GenerateRefreshToken();
}
