using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared.Behaviors;

namespace StayHub.Services.Notification.Application;

/// <summary>
/// Registers Notification Application layer services: MediatR, FluentValidation, pipeline behaviors.
/// </summary>
public static class ApplicationRegistration
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationRegistration).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline behaviors — order matters
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
