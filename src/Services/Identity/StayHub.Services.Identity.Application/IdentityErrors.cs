using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application;

/// <summary>
/// Static error definitions for the Identity bounded context.
/// Follows the pattern: "{Entity}.{ErrorType}" for consistent error codes.
/// </summary>
public static class IdentityErrors
{
    public static class User
    {
        public static readonly Error DuplicateEmail = new(
            "User.DuplicateEmail",
            "A user with this email address already exists.");

        public static readonly Error NotFound = new(
            "User.NotFound",
            "User was not found.");

        public static readonly Error InvalidCredentials = new(
            "User.InvalidCredentials",
            "Invalid email or password.");

        public static readonly Error AccountLocked = new(
            "User.AccountLocked",
            "Account is locked due to too many failed login attempts. Please try again later.");

        public static readonly Error AccountDisabled = new(
            "User.AccountDisabled",
            "This account has been disabled.");

        public static readonly Error EmailNotConfirmed = new(
            "User.EmailNotConfirmed",
            "Email address has not been confirmed.");

        public static readonly Error RegistrationFailed = new(
            "User.RegistrationFailed",
            "User registration failed. Please try again.");

        public static Error RegistrationFailedWithDetails(string details) => new(
            "User.RegistrationFailed",
            $"User registration failed: {details}");

        public static readonly Error PasswordChangeFailed = new(
            "User.PasswordChangeFailed",
            "Password change failed. Please verify your current password.");

        public static readonly Error RoleAssignmentFailed = new(
            "User.RoleAssignmentFailed",
            "Failed to assign the specified role.");

        public static readonly Error InvalidRole = new(
            "User.InvalidRole",
            "The specified role is not valid.");
    }

    public static class Token
    {
        public static readonly Error InvalidRefreshToken = new(
            "Token.InvalidRefreshToken",
            "The refresh token is invalid or has expired.");

        public static readonly Error RefreshTokenRevoked = new(
            "Token.RefreshTokenRevoked",
            "The refresh token has been revoked.");

        public static readonly Error RefreshTokenExpired = new(
            "Token.RefreshTokenExpired",
            "The refresh token has expired.");
    }

    public static class Email
    {
        public static readonly Error ConfirmationFailed = new(
            "Email.ConfirmationFailed",
            "Email confirmation failed. The token may have expired.");

        public static readonly Error InvalidToken = new(
            "Email.InvalidToken",
            "The email confirmation token is invalid.");

        public static readonly Error AlreadyConfirmed = new(
            "Email.AlreadyConfirmed",
            "Email address has already been confirmed.");

        public static readonly Error TokenGenerationFailed = new(
            "Email.TokenGenerationFailed",
            "Failed to generate email confirmation token.");
    }
}
