using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Payment.Application.Abstractions;
using StayHub.Services.Payment.Domain.Repositories;
using StayHub.Services.Payment.Infrastructure.ExternalServices;
using StayHub.Services.Payment.Infrastructure.Persistence;
using StayHub.Services.Payment.Infrastructure.Persistence.Repositories;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Outbox;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Payment.Infrastructure;

/// <summary>
/// DI registration for Payment Infrastructure layer.
/// Configures EF Core, repositories, Stripe provider, and outbox processor.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddPaymentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<PaymentDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("PaymentDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Payment DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());

        // ── Repositories ──
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // ── Outbox processor (polls OutboxMessages table, publishes to broker) ──
        services.AddOutboxProcessor<PaymentDbContext>();

        // ── Stripe payment provider ──
        services.AddSingleton<IPaymentProvider, StripePaymentProvider>();

        return services;
    }
}
