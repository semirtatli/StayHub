using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared;

namespace StayHub.Services.Review.Application;

/// <summary>
/// DI registration for the Review Application layer.
/// Registers MediatR handlers, FluentValidation validators, and pipeline behaviors.
/// </summary>
public static class ApplicationRegistration
{
    public static IServiceCollection AddReviewApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Registers MediatR + behaviors + validators from this assembly
        services.AddSharedKernel(assembly);

        return services;
    }
}
