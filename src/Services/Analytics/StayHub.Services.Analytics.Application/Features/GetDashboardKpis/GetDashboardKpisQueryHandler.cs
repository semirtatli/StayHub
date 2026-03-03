using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.GetDashboardKpis;

public sealed class GetDashboardKpisQueryHandler
    : IQueryHandler<GetDashboardKpisQuery, DashboardKpiDto>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public GetDashboardKpisQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<DashboardKpiDto>> Handle(
        GetDashboardKpisQuery request,
        CancellationToken cancellationToken)
    {
        var kpis = await _queryStore.GetDashboardKpisAsync(cancellationToken);
        return kpis;
    }
}
