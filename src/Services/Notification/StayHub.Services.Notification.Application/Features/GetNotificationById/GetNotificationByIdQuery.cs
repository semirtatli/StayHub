using StayHub.Services.Notification.Application.DTOs;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Notification.Application.Features.GetNotificationById;

/// <summary>
/// Query to retrieve a notification by its ID.
/// </summary>
public sealed record GetNotificationByIdQuery(Guid NotificationId) : IQuery<NotificationDto>;
