using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Shared;

namespace StayHub.Services.Payment.Application;

/// <summary>
/// DI registration for the Payment Application layer.
/// Registers MediatR handlers, FluentValidation validators, and pipeline behaviors.
/// </summary>
public static class ApplicationRegistration
{
    public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Registers MediatR + behaviors + validators from this assembly
        services.AddSharedKernel(assembly);

        return services;
    }
}
