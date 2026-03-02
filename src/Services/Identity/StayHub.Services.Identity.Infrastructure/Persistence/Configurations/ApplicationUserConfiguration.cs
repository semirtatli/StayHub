using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayHub.Services.Identity.Infrastructure.Identity;

namespace StayHub.Services.Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ApplicationUser (extends Identity's AspNetUsers table).
/// Adds columns for StayHub-specific properties.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for common query: find user by email + active status
        builder.HasIndex(u => new { u.Email, u.IsActive })
            .HasDatabaseName("IX_AspNetUsers_Email_IsActive");
    }
}
