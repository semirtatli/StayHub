using StayHub.Shared.Domain;

namespace StayHub.Services.Notification.Domain.Events;

/// <summary>Raised when a notification is successfully sent.</summary>
public sealed record NotificationSentEvent(
    Guid NotificationId,
    string Recipient,
    string Type) : DomainEvent;

/// <summary>Raised when sending a notification fails.</summary>
public sealed record NotificationFailedEvent(
    Guid NotificationId,
    string Recipient,
    string Type,
    string FailureReason) : DomainEvent;
