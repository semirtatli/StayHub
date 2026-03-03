using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.ExportAnalytics;

/// <summary>
/// Exports revenue analytics as a CSV file for the specified date range.
/// Returns the file bytes and metadata for the API response.
/// </summary>
public sealed record ExportAnalyticsQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? HotelId = null) : IQuery<AnalyticsExportDto>;
