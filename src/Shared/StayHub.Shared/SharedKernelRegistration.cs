using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared.Behaviors;
using System.Reflection;

namespace StayHub.Shared;

/// <summary>
/// Extension methods for registering StayHub.Shared services in the DI container.
/// Each microservice calls this in its Program.cs to wire up MediatR + behaviors + validators.
///
/// Usage:
///   builder.Services.AddSharedKernel(typeof(CreateBookingCommand).Assembly);
///
/// This registers:
/// - MediatR with all handlers from the calling assembly
/// - Pipeline behaviors in the correct order (outermost → innermost):
///   1. UnhandledExceptionBehavior (catches + logs unexpected exceptions)
///   2. LoggingBehavior (logs request/response with timing)
///   3. ValidationBehavior (runs FluentValidation before handler)
///   4. TransactionBehavior (commits IUnitOfWork after successful commands)
/// - All FluentValidation validators from the calling assembly
/// </summary>
public static class SharedKernelRegistration
{
    /// <summary>
    /// Registers MediatR, pipeline behaviors, and FluentValidation validators.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">
    /// The assemblies containing handlers, validators, and domain types.
    /// Typically the Application layer assembly of each microservice.
    /// </param>
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Register MediatR and discover all handlers from provided assemblies
        services.AddMediatR(config =>
        {
            foreach (var assembly in assemblies)
            {
                config.RegisterServicesFromAssembly(assembly);
            }

            // Pipeline behaviors execute in registration order (first registered = outermost).
            // Order matters:
            // 1. Exception handling wraps everything
            // 2. Logging captures timing for the full pipeline
            // 3. Validation short-circuits before handler if invalid
            // 4. Transaction commits after successful handler execution
            config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Register all FluentValidation validators from provided assemblies
        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        }

        return services;
    }
}
