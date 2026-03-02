using Microsoft.AspNetCore.Identity;

namespace StayHub.Services.Identity.Infrastructure.Identity;

/// <summary>
/// Application user — extends ASP.NET Core Identity's IdentityUser with
/// StayHub-specific profile properties.
///
/// Why in Infrastructure and not Domain?
/// IdentityUser is a framework type from Microsoft.AspNetCore.Identity.
/// Putting it in Domain would violate the dependency rule (Domain must not depend
/// on infrastructure frameworks). Instead, the Application layer uses IIdentityService
/// to interact with users through a clean abstraction.
///
/// The domain events and business logic live in the Application/Domain layers.
/// This is just the persistence model for Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}
