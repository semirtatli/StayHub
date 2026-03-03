using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayHub.Services.Analytics.Application.Features.ExportAnalytics;
using StayHub.Services.Analytics.Application.Features.GetBookingTrends;
using StayHub.Services.Analytics.Application.Features.GetDashboardKpis;
using StayHub.Services.Analytics.Application.Features.GetOccupancyAnalytics;
using StayHub.Services.Analytics.Application.Features.GetRevenueAnalytics;
using StayHub.Services.Analytics.Application.Features.GetTopHotels;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Api.Controllers;

/// <summary>
/// Admin analytics endpoints — revenue dashboards, occupancy rates,
/// booking trends, top hotels, KPI summaries, and CSV export.
/// All endpoints require Admin role.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public sealed class AnalyticsController : ApiController
{
    /// <summary>
    /// Revenue time-series for the specified date range.
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? hotelId = null)
    {
        var result = await Mediator.Send(
            new GetRevenueAnalyticsQuery(startDate, endDate, hotelId));

        return HandleResult(result);
    }

    /// <summary>
    /// Occupancy rate time-series for the specified date range.
    /// </summary>
    [HttpGet("occupancy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOccupancy(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? hotelId = null)
    {
        var result = await Mediator.Send(
            new GetOccupancyAnalyticsQuery(startDate, endDate, hotelId));

        return HandleResult(result);
    }

    /// <summary>
    /// Booking trends aggregated by the specified period.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTrends(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] TimePeriod period = TimePeriod.Daily,
        [FromQuery] Guid? hotelId = null)
    {
        var result = await Mediator.Send(
            new GetBookingTrendsQuery(startDate, endDate, period, hotelId));

        return HandleResult(result);
    }

    /// <summary>
    /// Top performing hotels ranked by the specified metric.
    /// Supported metrics: Revenue, Bookings, Rating, Reviews.
    /// </summary>
    [HttpGet("top-hotels")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopHotels(
        [FromQuery] string metricType = "Revenue",
        [FromQuery] int count = 10)
    {
        var result = await Mediator.Send(
            new GetTopHotelsQuery(metricType, count));

        return HandleResult(result);
    }

    /// <summary>
    /// Aggregated KPI summary for the admin dashboard.
    /// Includes period-over-period trend indicators.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await Mediator.Send(new GetDashboardKpisQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Export revenue analytics as a CSV file.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Export(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? hotelId = null)
    {
        var result = await Mediator.Send(
            new ExportAnalyticsQuery(startDate, endDate, hotelId));

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var export = result.Value;
        return File(export.FileContent, export.ContentType, export.FileName);
    }

    /// <summary>
    /// Health check / ping endpoint (no auth required).
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { service = "StayHub.Analytics", status = "healthy" });
}
