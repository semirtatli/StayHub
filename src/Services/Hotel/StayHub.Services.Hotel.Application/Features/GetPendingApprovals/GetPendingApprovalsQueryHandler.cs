using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Application.DTOs;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Hotel.Application.Features.GetPendingApprovals;

/// <summary>
/// Handles fetching all hotels awaiting admin approval.
/// Returns summary DTOs ordered by creation date (oldest first for FIFO review).
/// </summary>
public sealed class GetPendingApprovalsQueryHandler
    : IQueryHandler<GetPendingApprovalsQuery, IReadOnlyList<HotelSummaryDto>>
{
    private readonly IHotelRepository _hotelRepository;
    private readonly ILogger<GetPendingApprovalsQueryHandler> _logger;

    public GetPendingApprovalsQueryHandler(
        IHotelRepository hotelRepository,
        ILogger<GetPendingApprovalsQueryHandler> logger)
    {
        _hotelRepository = hotelRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<HotelSummaryDto>>> Handle(
        GetPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var hotels = await _hotelRepository.GetByStatusAsync(
            HotelStatus.PendingApproval, cancellationToken);

        _logger.LogDebug("Found {Count} hotels pending approval", hotels.Count);

        var dtos = hotels.Select(HotelMappings.ToSummaryDto).ToList();

        return dtos;
    }
}
