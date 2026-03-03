using StayHub.Services.Notification.Application.DTOs;
using StayHub.Services.Notification.Domain.Enums;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Notification.Application.Features.SendNotification;

/// <summary>
/// Command to create and send a notification.
/// Used internally by event consumers to dispatch notifications through the standard pipeline.
/// </summary>
public sealed record SendNotificationCommand(
    string? UserId,
    NotificationChannel Channel,
    NotificationType Type,
    string Recipient,
    string TemplateName,
    Dictionary<string, string> TemplateData,
    Guid? CorrelationId = null) : ICommand<NotificationDto>;
