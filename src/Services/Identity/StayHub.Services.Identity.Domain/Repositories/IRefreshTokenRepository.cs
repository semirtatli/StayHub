using StayHub.Services.Identity.Domain.Entities;

namespace StayHub.Services.Identity.Domain.Repositories;

/// <summary>
/// Repository for refresh token persistence.
/// Implemented in the Infrastructure layer with EF Core.
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
