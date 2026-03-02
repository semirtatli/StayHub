using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Abstractions;

/// <summary>
/// Abstraction for ASP.NET Core Identity operations.
/// This sits in the Application layer so command handlers can work with Identity
/// without depending on the Infrastructure layer (Dependency Inversion).
///
/// Implemented by IdentityService in the Infrastructure layer using UserManager/SignInManager.
/// </summary>
public interface IIdentityService
{
    Task<Result<string>> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> AuthenticateAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<Result> AssignRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateProfileAsync(
        string userId,
        string firstName,
        string lastName,
        string? phoneNumber,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a successful authentication (login or token refresh).
/// </summary>
public sealed record AuthenticationResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    UserDto User);

/// <summary>
/// User data transfer object — safe projection for API responses.
/// Never includes password hash or security stamps.
/// </summary>
public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    string Role,
    bool EmailConfirmed,
    DateTime CreatedAt);
