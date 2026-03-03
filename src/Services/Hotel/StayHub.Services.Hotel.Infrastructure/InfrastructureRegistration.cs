using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Hotel.Application.Abstractions;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Services.Hotel.Infrastructure.Persistence;
using StayHub.Services.Hotel.Infrastructure.Persistence.Repositories;
using StayHub.Services.Hotel.Infrastructure.Storage;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Hotel.Infrastructure;

/// <summary>
/// DI registration for Hotel Infrastructure layer.
/// Configures EF Core, repositories, and external service implementations.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddHotelInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<HotelDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("HotelDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(HotelDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Hotel DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<HotelDbContext>());

        // ── Repositories ──
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();

        // ── File storage ──
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
