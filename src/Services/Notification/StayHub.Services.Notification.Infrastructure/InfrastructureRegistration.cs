using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Notification.Application.Abstractions;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Services.Notification.Infrastructure.BackgroundServices;
using StayHub.Services.Notification.Infrastructure.Email;
using StayHub.Services.Notification.Infrastructure.Persistence;
using StayHub.Services.Notification.Infrastructure.Persistence.Repositories;
using StayHub.Services.Notification.Infrastructure.Templates;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Outbox;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Notification.Infrastructure;

/// <summary>
/// DI registration for Notification Infrastructure layer.
/// Configures EF Core, repositories, email sender, template renderer, and background services.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<NotificationDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("NotificationDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Notification DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NotificationDbContext>());

        // ── Repositories ──
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // ── Outbox processor ──
        services.AddOutboxProcessor<NotificationDbContext>();

        // ── Email sender (dev: log only; prod: swap to SMTP/SendGrid) ──
        services.AddSingleton<IEmailSender, LogEmailSender>();

        // ── Template renderer (embedded resource-based) ──
        services.AddSingleton<ITemplateRenderer, EmbeddedResourceTemplateRenderer>();

        // ── Background services ──
        services.AddHostedService<NotificationRetryService>();

        return services;
    }
}
