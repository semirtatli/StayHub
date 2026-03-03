using StayHub.Services.Notification.Application.DTOs;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Application.Features.GetUserNotifications;

/// <summary>
/// Handles retrieving all notifications for a user.
/// </summary>
internal sealed class GetUserNotificationsQueryHandler
    : IQueryHandler<GetUserNotificationsQuery, IReadOnlyList<NotificationSummaryDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUserNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<IReadOnlyList<NotificationSummaryDto>>> Handle(
        GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        var dtos = notifications.Select(n => n.ToSummaryDto()).ToList();

        return Result.Success<IReadOnlyList<NotificationSummaryDto>>(dtos);
    }
}
