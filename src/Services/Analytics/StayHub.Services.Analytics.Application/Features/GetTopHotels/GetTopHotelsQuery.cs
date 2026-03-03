using StayHub.Services.Analytics.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Analytics.Application.Features.GetTopHotels;

/// <summary>
/// Retrieves top-performing hotels ranked by the specified metric.
/// Supported metrics: Revenue, Bookings, Rating, Reviews.
/// </summary>
public sealed record GetTopHotelsQuery(
    string MetricType = "Revenue",
    int Count = 10) : IQuery<IReadOnlyList<HotelPerformanceDto>>;
