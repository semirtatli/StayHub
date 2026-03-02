using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Services;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Infrastructure;

/// <summary>
/// Extension methods for registering shared infrastructure services.
/// Each microservice calls this in its Program.cs:
///
///   builder.Services.AddSharedInfrastructure();
///
/// Registers:
/// - IDateTimeProvider (singleton — stateless)
/// - AuditableEntityInterceptor (scoped — uses ICurrentUserService per request)
/// - SoftDeleteInterceptor (scoped)
///
/// Note: ICurrentUserService is NOT registered here — each API project
/// registers its own HttpContext-based implementation.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        // Singleton: stateless, returns DateTime.UtcNow
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Scoped: depends on ICurrentUserService (per-request)
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        return services;
    }
}
