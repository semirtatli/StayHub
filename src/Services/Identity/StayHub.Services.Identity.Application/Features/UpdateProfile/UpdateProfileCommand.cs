using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.UpdateProfile;

/// <summary>
/// Command to update the authenticated user's profile (name, phone number).
/// Returns the updated UserDto.
/// </summary>
public sealed record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : ICommand<UserDto>;
