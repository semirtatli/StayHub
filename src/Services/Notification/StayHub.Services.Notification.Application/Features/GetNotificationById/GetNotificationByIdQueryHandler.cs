using StayHub.Services.Notification.Application.DTOs;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Application.Features.GetNotificationById;

/// <summary>
/// Handles retrieving a notification by ID.
/// </summary>
internal sealed class GetNotificationByIdQueryHandler : IQueryHandler<GetNotificationByIdQuery, NotificationDto>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationByIdQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<NotificationDto>> Handle(
        GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(
            request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure<NotificationDto>(NotificationErrors.Notification.NotFound);

        return notification.ToDto();
    }
}
