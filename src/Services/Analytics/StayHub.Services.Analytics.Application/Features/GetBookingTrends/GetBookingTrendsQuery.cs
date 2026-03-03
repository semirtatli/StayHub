using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Services.Analytics.Domain.Enums;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.GetBookingTrends;

/// <summary>
/// Retrieves booking trend data aggregated by the specified time period.
/// </summary>
public sealed record GetBookingTrendsQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimePeriod Period = TimePeriod.Daily,
    Guid? HotelId = null) : IQuery<IReadOnlyList<BookingTrendDto>>;
