using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application;

/// <summary>
/// Domain error codes for the Analytics Service.
/// </summary>
public static class AnalyticsErrors
{
    public static readonly Error NotFound = new(
        "Analytics.NotFound",
        "The requested analytics data was not found.");

    public static readonly Error InvalidDateRange = new(
        "Analytics.InvalidDateRange",
        "The start date must be before the end date.");

    public static readonly Error NoDataAvailable = new(
        "Analytics.NoDataAvailable",
        "No analytics data is available for the specified criteria.");

    public static readonly Error ExportFailed = new(
        "Analytics.ExportFailed",
        "Failed to generate the analytics export.");

    public static readonly Error InvalidMetricType = new(
        "Analytics.InvalidMetricType",
        "The specified metric type is not supported.");
}
