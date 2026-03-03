using FluentAssertions;
using StayHub.Services.Identity.Domain.Entities;

namespace StayHub.Services.Identity.UnitTests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = "user-123";
        var token = "random-token-value";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var ip = "127.0.0.1";

        // Act
        var refreshToken = RefreshToken.Create(userId, token, expiresAt, ip);

        // Assert
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.CreatedByIp.Should().Be(ip);
        refreshToken.Id.Should().NotBeEmpty();
        refreshToken.IsActive.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldBeTrue()
    {
        var token = RefreshToken.Create("user-1", "token", DateTime.UtcNow.AddHours(1), "127.0.0.1");

        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldBeFalse()
    {
        var token = RefreshToken.Create("user-1", "token", DateTime.UtcNow.AddHours(1), "127.0.0.1");

        token.Revoke("127.0.0.1");

        token.IsActive.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldBeFalse()
    {
        var token = RefreshToken.Create("user-1", "token", DateTime.UtcNow.AddMilliseconds(-1), "127.0.0.1");

        token.IsActive.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedProperties()
    {
        var token = RefreshToken.Create("user-1", "token", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        token.Revoke("192.168.1.1", "new-replacement-token");

        token.IsRevoked.Should().BeTrue();
        token.RevokedByIp.Should().Be("192.168.1.1");
        token.ReplacedByToken.Should().Be("new-replacement-token");
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public void Revoke_WithoutParameters_ShouldStillRevoke()
    {
        var token = RefreshToken.Create("user-1", "token", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
        token.RevokedByIp.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
    }
}
