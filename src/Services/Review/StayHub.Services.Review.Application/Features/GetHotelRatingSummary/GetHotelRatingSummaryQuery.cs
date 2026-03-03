using StayHub.Services.Review.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Review.Application.Features.GetHotelRatingSummary;

/// <summary>
/// Query to get the aggregated rating summary for a hotel.
/// </summary>
public sealed record GetHotelRatingSummaryQuery(Guid HotelId) : IQuery<HotelRatingSummaryDto>;
