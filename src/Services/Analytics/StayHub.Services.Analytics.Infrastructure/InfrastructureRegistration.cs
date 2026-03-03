using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Domain.Repositories;
using StayHub.Services.Analytics.Infrastructure.Persistence;
using StayHub.Services.Analytics.Infrastructure.Persistence.QueryStores;
using StayHub.Services.Analytics.Infrastructure.Persistence.Repositories;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Outbox;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Analytics.Infrastructure;

/// <summary>
/// DI registration for Analytics Infrastructure layer.
/// Configures EF Core, repositories, query store, and outbox processor.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddAnalyticsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<AnalyticsDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("AnalyticsDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AnalyticsDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Analytics DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AnalyticsDbContext>());

        // ── Repositories (write-side) ──
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

        // ── Query Store (read-side, CQRS) ──
        services.AddScoped<IAnalyticsQueryStore, AnalyticsQueryStore>();

        // ── Outbox processor (polls OutboxMessages table, publishes to broker) ──
        services.AddOutboxProcessor<AnalyticsDbContext>();

        return services;
    }
}
