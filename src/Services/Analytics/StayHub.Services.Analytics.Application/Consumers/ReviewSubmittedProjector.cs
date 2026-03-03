using MediatR;
using Microsoft.Extensions.Logging;
using StayHub.Services.Analytics.Application.Features.RecordAnalyticsEvent;
using StayHub.Services.Analytics.Application.IntegrationEvents;
using StayHub.Services.Analytics.Domain.Enums;

namespace StayHub.Services.Analytics.Application.Consumers;

/// <summary>
/// Projects ReviewSubmitted events — updates hotel average rating and review counts.
/// </summary>
public sealed class ReviewSubmittedProjector : INotificationHandler<ReviewSubmittedIntegrationEvent>
{
    private readonly ISender _mediator;
    private readonly ILogger<ReviewSubmittedProjector> _logger;

    public ReviewSubmittedProjector(ISender mediator, ILogger<ReviewSubmittedProjector> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        ReviewSubmittedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Projecting ReviewSubmitted: ReviewId={ReviewId}, HotelId={HotelId}, Rating={Rating}",
            notification.ReviewId, notification.HotelId, notification.OverallRating);

        await _mediator.Send(new RecordAnalyticsEventCommand(
            EventType: AnalyticsEventType.ReviewSubmitted,
            HotelId: notification.HotelId,
            UserId: notification.UserId,
            Rating: notification.OverallRating), cancellationToken);
    }
}
