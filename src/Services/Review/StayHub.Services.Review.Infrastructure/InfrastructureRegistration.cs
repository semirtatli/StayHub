using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Review.Domain.Repositories;
using StayHub.Services.Review.Infrastructure.Persistence;
using StayHub.Services.Review.Infrastructure.Persistence.Repositories;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Outbox;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Review.Infrastructure;

/// <summary>
/// DI registration for Review Infrastructure layer.
/// Configures EF Core, repositories, and outbox processor.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddReviewInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<ReviewDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("ReviewDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ReviewDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Review DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ReviewDbContext>());

        // ── Repositories ──
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // ── Outbox processor (polls OutboxMessages table, publishes to broker) ──
        services.AddOutboxProcessor<ReviewDbContext>();

        return services;
    }
}
