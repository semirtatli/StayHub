using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StayHub.Services.Booking.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that replaces the SQL Server connection
/// with an in-memory database for fast integration tests.
/// For full DB integration, use Testcontainers with SQL Server.
/// </summary>
public class BookingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related service descriptors
            var dbDescriptors = services
                .Where(d => d.ServiceType.FullName != null &&
                           (d.ServiceType.FullName.Contains("DbContextOptions") ||
                            d.ServiceType.FullName.Contains("BookingDbContext")))
                .ToList();

            foreach (var descriptor in dbDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove hosted services (outbox processor, etc.) that depend on the DB
            var hostedServiceDescriptors = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService))
                .ToList();

            foreach (var descriptor in hostedServiceDescriptors)
            {
                services.Remove(descriptor);
            }
        });

        builder.UseEnvironment("Testing");
    }
}
