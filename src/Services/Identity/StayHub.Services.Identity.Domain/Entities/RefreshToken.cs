using StayHub.Shared.Domain;

namespace StayHub.Services.Identity.Domain.Entities;

/// <summary>
/// Refresh token entity — stored in DB for secure token rotation.
///
/// Flow:
/// 1. User logs in → receives JWT access token (15min, in memory) + refresh token (7 days, httpOnly cookie)
/// 2. Access token expires → client sends refresh token to /api/identity/refresh
/// 3. Server validates refresh token, issues new pair, revokes old one
/// 4. If old refresh token is reused → security breach detected, revoke entire family
///
/// Token rotation prevents replay attacks. Each token can only be used once.
/// </summary>
public class RefreshToken : Entity
{
    public string Token { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this token is still valid (not expired and not revoked).
    /// </summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Whether this token has been revoked (manually or by rotation).
    /// </summary>
    public bool IsRevoked => RevokedAt is not null;

    /// <summary>
    /// Whether this token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    private RefreshToken() { }

    public static RefreshToken Create(
        string userId,
        string token,
        DateTime expiresAt,
        string createdByIp)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Revoke this token (used during rotation or logout).
    /// </summary>
    public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}
