namespace StayHub.Shared.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// Implemented in the API layer by reading HttpContext claims.
/// Used by audit interceptors and authorization logic.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The authenticated user's ID (from JWT claims). Null if anonymous.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// The authenticated user's email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Check if the current user has a specific role.
    /// </summary>
    bool IsInRole(string role);
}
