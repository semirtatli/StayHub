using StayHub.Services.Notification.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Notification.Application.Features.GetUserNotifications;

/// <summary>
/// Query to retrieve all notifications for a specific user.
/// </summary>
public sealed record GetUserNotificationsQuery(string UserId) : IQuery<IReadOnlyList<NotificationSummaryDto>>;
