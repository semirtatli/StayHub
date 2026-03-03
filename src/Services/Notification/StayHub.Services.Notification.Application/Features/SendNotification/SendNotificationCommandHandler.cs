using Microsoft.Extensions.Logging;
using StayHub.Services.Notification.Application.Abstractions;
using StayHub.Services.Notification.Application.DTOs;
using StayHub.Services.Notification.Domain.Entities;
using StayHub.Services.Notification.Domain.Repositories;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Notification.Application.Features.SendNotification;

/// <summary>
/// Handles creating and sending a notification.
///
/// Flow:
/// 1. Render the email template with provided data.
/// 2. Create a NotificationEntity in Pending status.
/// 3. Attempt to send via IEmailSender.
/// 4. Mark as Sent or record failure.
/// 5. Return the notification DTO.
///
/// The entity is persisted by TransactionBehavior (handler does NOT call SaveChangesAsync).
/// </summary>
internal sealed class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    public SendNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task<Result<NotificationDto>> Handle(
        SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // 1. Render template
        string renderedBody;
        try
        {
            renderedBody = await _templateRenderer.RenderAsync(
                request.TemplateName, request.TemplateData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateName}", request.TemplateName);
            return Result.Failure<NotificationDto>(NotificationErrors.Notification.TemplateNotFound);
        }

        // 2. Derive subject from template data or use a default
        var subject = request.TemplateData.TryGetValue("Subject", out var subjectValue)
            ? subjectValue
            : $"StayHub — {request.Type}";

        // 3. Create notification entity
        var notification = NotificationEntity.Create(
            request.UserId,
            request.Channel,
            request.Type,
            request.Recipient,
            subject,
            renderedBody,
            request.CorrelationId);

        _notificationRepository.Add(notification);

        // 4. Attempt delivery
        try
        {
            var sent = await _emailSender.SendAsync(
                request.Recipient, subject, renderedBody, cancellationToken);

            if (sent)
            {
                notification.MarkAsSent();
                _logger.LogInformation(
                    "Notification {NotificationId} sent to {Recipient} ({Type})",
                    notification.Id, request.Recipient, request.Type);
            }
            else
            {
                notification.MarkAsFailed("Email sender returned failure.");
                _logger.LogWarning(
                    "Notification {NotificationId} delivery failed for {Recipient}",
                    notification.Id, request.Recipient);
            }
        }
        catch (Exception ex)
        {
            notification.MarkAsFailed(ex.Message);
            _logger.LogError(ex,
                "Exception sending notification {NotificationId} to {Recipient}",
                notification.Id, request.Recipient);
        }

        return notification.ToDto();
    }
}
