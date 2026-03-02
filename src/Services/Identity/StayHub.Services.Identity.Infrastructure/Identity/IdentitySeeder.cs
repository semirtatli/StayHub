using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Identity.Domain.Enums;

namespace StayHub.Services.Identity.Infrastructure.Identity;

/// <summary>
/// Seeds ASP.NET Core Identity roles on application startup.
/// Ensures Guest, HotelOwner, and Admin roles exist in the database
/// before any user registration can assign them.
///
/// Called from Program.cs after the app is built but before it starts listening.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
