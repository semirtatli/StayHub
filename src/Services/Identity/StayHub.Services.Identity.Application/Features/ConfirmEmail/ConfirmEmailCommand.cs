using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.ConfirmEmail;

/// <summary>
/// Command to confirm a user's email address using the confirmation token.
/// This is typically called when the user clicks the confirmation link in their email.
/// </summary>
public sealed record ConfirmEmailCommand(
    string UserId,
    string Token) : ICommand;
