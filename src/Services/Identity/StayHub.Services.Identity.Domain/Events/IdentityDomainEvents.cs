using StayHub.Shared.Domain;

namespace StayHub.Services.Identity.Domain.Events;

/// <summary>
/// Raised when a new user registers.
/// Handlers: send welcome email, create default profile, track analytics.
/// </summary>
public sealed record UserRegisteredEvent(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role) : DomainEvent;

/// <summary>
/// Raised when a user successfully logs in.
/// Handlers: update last login timestamp, log for security audit.
/// </summary>
public sealed record UserLoggedInEvent(
    string UserId,
    string Email,
    string IpAddress) : DomainEvent;

/// <summary>
/// Raised when a user's email is confirmed.
/// Handlers: unlock full account features.
/// </summary>
public sealed record EmailConfirmedEvent(
    string UserId,
    string Email) : DomainEvent;

/// <summary>
/// Raised when a user's role is changed.
/// Handlers: notify user, update cached permissions.
/// </summary>
public sealed record UserRoleChangedEvent(
    string UserId,
    string OldRole,
    string NewRole,
    string ChangedByUserId) : DomainEvent;

/// <summary>
/// Raised when a user changes their password.
/// Handlers: revoke all refresh tokens, send security notification email.
/// </summary>
public sealed record PasswordChangedEvent(
    string UserId,
    string Email) : DomainEvent;
