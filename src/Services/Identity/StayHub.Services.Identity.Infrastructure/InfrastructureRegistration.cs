using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Application.IntegrationEvents;
using StayHub.Services.Identity.Domain.Repositories;
using StayHub.Services.Identity.Infrastructure.Email;
using StayHub.Services.Identity.Infrastructure.Identity;
using StayHub.Services.Identity.Infrastructure.Persistence;
using StayHub.Services.Identity.Infrastructure.Persistence.Repositories;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Identity.Infrastructure;

/// <summary>
/// DI registration for Identity Infrastructure layer.
/// Configures EF Core, ASP.NET Core Identity, and repositories.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("IdentityDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Identity DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());

        // ── ASP.NET Core Identity ──
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password policy
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;

                // Lockout policy
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Will be true in production
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        // ── Repositories ──
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ── Application service implementations ──
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // ── Integration event senders ──
        // Development: logs confirmation URL. Production: will publish to RabbitMQ.
        services.AddScoped<IEmailVerificationSender, LogEmailVerificationSender>();

        return services;
    }
}
