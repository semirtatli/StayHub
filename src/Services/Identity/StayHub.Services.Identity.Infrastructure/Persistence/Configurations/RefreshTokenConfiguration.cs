using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Identity.Domain.Entities;

namespace StayHub.Services.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for RefreshToken entity.
/// Token column is indexed for fast lookup during refresh flow.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(512);

        // Audit columns from Entity base
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedBy)
            .HasMaxLength(256);

        // Index: look up token during refresh flow (must be fast)
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        // Index: find all tokens for a user (used during logout/revoke-all)
        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // Index: cleanup expired tokens
        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
    }
}
