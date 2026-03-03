using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared.Interfaces;
using StayHub.Shared.Web.Services;

namespace StayHub.Shared.Web;

/// <summary>
/// Extension methods for registering shared web services.
/// Call this in each API project's Program.cs:
///   builder.Services.AddSharedWebServices();
/// </summary>
public static class SharedWebRegistration
{
    public static IServiceCollection AddSharedWebServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
