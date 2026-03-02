using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.Register;

/// <summary>
/// Command to register a new user.
/// Returns the new user's ID on success.
///
/// Flow: Controller → MediatR pipeline (Validation → Logging → Handler)
///       → IIdentityService.RegisterUserAsync → publishes UserRegisteredEvent.
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string Role) : ICommand<RegisterUserResponse>;

/// <summary>
/// Response DTO for successful registration.
/// </summary>
public sealed record RegisterUserResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role);
