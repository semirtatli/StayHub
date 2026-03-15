using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StayHub.Shared.Infrastructure.Caching;

public static class CachingRegistration
{
    public static IServiceCollection AddStayHubCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "StayHub:";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
