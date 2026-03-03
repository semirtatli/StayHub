using System.Globalization;
using StayHub.Services.Analytics.Application.Abstractions;
using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Analytics.Application.Features.ExportAnalytics;

public sealed class ExportAnalyticsQueryHandler
    : IQueryHandler<ExportAnalyticsQuery, AnalyticsExportDto>
{
    private readonly IAnalyticsQueryStore _queryStore;

    public ExportAnalyticsQueryHandler(IAnalyticsQueryStore queryStore)
    {
        _queryStore = queryStore;
    }

    public async Task<Result<AnalyticsExportDto>> Handle(
        ExportAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<AnalyticsExportDto>(AnalyticsErrors.InvalidDateRange);
        }

        var csvBytes = await _queryStore.ExportRevenueDataAsync(
            request.StartDate, request.EndDate, request.HotelId, cancellationToken);

        if (csvBytes.Length == 0)
        {
            return Result.Failure<AnalyticsExportDto>(AnalyticsErrors.NoDataAvailable);
        }

        var fileName = string.Create(
            CultureInfo.InvariantCulture,
            $"revenue_{request.StartDate:yyyy-MM-dd}_{request.EndDate:yyyy-MM-dd}.csv");

        var export = new AnalyticsExportDto(csvBytes, fileName, "text/csv");

        return export;
    }
}
