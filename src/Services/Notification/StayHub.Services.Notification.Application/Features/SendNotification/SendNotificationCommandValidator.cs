using FluentValidation;

namespace StayHub.Services.Notification.Application.Features.SendNotification;

/// <summary>
/// Validates the SendNotificationCommand.
/// </summary>
public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Recipient is required.")
            .MaximumLength(256).WithMessage("Recipient must not exceed 256 characters.");

        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("Template name is required.")
            .MaximumLength(100).WithMessage("Template name must not exceed 100 characters.");

        RuleFor(x => x.TemplateData)
            .NotNull().WithMessage("Template data is required.");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Invalid notification channel.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid notification type.");
    }
}
