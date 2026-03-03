using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.GetDashboardKpis;

/// <summary>
/// Retrieves aggregated KPI summary for the admin dashboard.
/// Compares last 30 days vs previous 30 days for trend indicators.
/// </summary>
public sealed record GetDashboardKpisQuery : IQuery<DashboardKpiDto>;
