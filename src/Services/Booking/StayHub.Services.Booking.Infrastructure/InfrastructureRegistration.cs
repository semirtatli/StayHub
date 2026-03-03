using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Services.Booking.Application.Abstractions;
using StayHub.Services.Booking.Domain.Repositories;
using StayHub.Services.Booking.Infrastructure.ExternalServices;
using StayHub.Services.Booking.Infrastructure.Persistence;
using StayHub.Services.Booking.Infrastructure.Persistence.Repositories;
using StayHub.Shared.Infrastructure;
using StayHub.Shared.Infrastructure.Interceptors;
using StayHub.Shared.Infrastructure.Outbox;
using StayHub.Shared.Interfaces;

namespace StayHub.Services.Booking.Infrastructure;

/// <summary>
/// DI registration for Booking Infrastructure layer.
/// Configures EF Core, repositories, and external service implementations.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddBookingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Shared infrastructure (DateTimeProvider, interceptors) ──
        services.AddSharedInfrastructure();

        // ── EF Core with SQL Server ──
        services.AddDbContext<BookingDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("BookingDb"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(auditInterceptor);
        });

        // Register IUnitOfWork pointing to the Booking DbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookingDbContext>());

        // ── Repositories ──
        services.AddScoped<IBookingRepository, BookingRepository>();

        // ── Outbox processor (polls OutboxMessages table, publishes to broker) ──
        services.AddOutboxProcessor<BookingDbContext>();

        // ── External service clients ──
        services.AddHttpClient<IHotelServiceClient, HotelServiceHttpClient>(client =>
        {
            var baseUrl = configuration["ExternalServices:HotelService:BaseUrl"]
                ?? "http://localhost:5102";

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
