using Microsoft.AspNetCore.Mvc;

namespace StayHub.Services.Booking.Api.Controllers;

/// <summary>
/// Booking management controller — placeholder for reservation CRUD operations.
///
/// Endpoints will be added in subsequent commits:
/// - Commit 24: Create reservation (POST /api/bookings)
/// - Commit 25: Status transitions (confirm, check-in, complete, cancel)
/// - Commit 28: Guest booking queries (GET /api/bookings/my, GET /api/bookings/{id})
/// </summary>
[Route("api/bookings")]
public sealed class BookingsController : ApiController
{
    /// <summary>
    /// Health check placeholder — confirms the Booking API is running.
    /// Will be replaced with actual booking endpoints in subsequent commits.
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { service = "StayHub.Booking", status = "healthy" });
    }
}
