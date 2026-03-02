using Microsoft.EntityFrameworkCore;
using StayHub.Services.Identity.Domain.Entities;
using StayHub.Services.Identity.Domain.Repositories;

namespace StayHub.Services.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IRefreshTokenRepository.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public RefreshTokenRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public void Add(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
    }

    public async Task RevokeAllByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }
}
