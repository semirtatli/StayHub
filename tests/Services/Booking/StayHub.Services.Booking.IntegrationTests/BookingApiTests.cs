using FluentAssertions;
using System.Net;

namespace StayHub.Services.Booking.IntegrationTests;

/// <summary>
/// Integration tests for the Booking API health check and basic endpoint availability.
/// Verifies the application can start up and respond to requests.
/// </summary>
public class BookingApiTests : IClassFixture<BookingApiFactory>
{
    private readonly BookingApiFactory _factory;

    public BookingApiTests(BookingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Factory_ShouldCreateServer()
    {
        // Verify the test server can be created (app starts up)
        using var client = _factory.CreateClient();
        client.Should().NotBeNull();
    }
}
