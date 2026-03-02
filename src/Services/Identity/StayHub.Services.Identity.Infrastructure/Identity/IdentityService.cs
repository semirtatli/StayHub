using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Domain.Enums;
using StayHub.Services.Identity.Domain.Events;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Infrastructure.Identity;

/// <summary>
/// Implements IIdentityService using ASP.NET Core Identity (UserManager, SignInManager).
/// This is the bridge between the Application layer's clean abstractions and
/// the infrastructure-level Identity framework.
///
/// Domain events are published via MediatR after successful operations
/// to decouple side effects (e.g., sending welcome emails) from the core flow.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMediator _mediator;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IMediator mediator,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate email
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return Result.Failure<string>(IdentityErrors.User.DuplicateEmail);
        }

        // Validate role
        if (!AppRoles.All.Contains(role))
        {
            return Result.Failure<string>(IdentityErrors.User.InvalidRole);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("User registration failed for {Email}: {Errors}", email, errors);
            return Result.Failure<string>(IdentityErrors.User.RegistrationFailedWithDetails(errors));
        }

        // Assign role
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Role assignment failed for {Email} to role {Role}", email, role);
            // User created but role failed — still return success but log warning
            // In production, this would be a compensating transaction
        }

        _logger.LogInformation("User {UserId} registered with email {Email} and role {Role}", user.Id, email, role);

        // Publish domain event
        await _mediator.Publish(new UserRegisteredEvent(user.Id, email, firstName, lastName, role), cancellationToken);

        return Result.Success(user.Id);
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticationResult>> AuthenticateAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        // Will be implemented in commit 12 (Authentication & JWT)
        await Task.CompletedTask;
        throw new NotImplementedException("AuthenticateAsync will be implemented in the authentication commit.");
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("RefreshTokenAsync will be implemented in the authentication commit.");
    }

    /// <inheritdoc />
    public async Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("RevokeRefreshTokenAsync will be implemented in the authentication commit.");
    }

    /// <inheritdoc />
    public async Task<Result> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("ConfirmEmailAsync will be implemented in a later commit.");
    }

    /// <inheritdoc />
    public async Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("ChangePasswordAsync will be implemented in a later commit.");
    }

    /// <inheritdoc />
    public async Task<Result> AssignRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("AssignRoleAsync will be implemented in a later commit.");
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure<UserDto>(IdentityErrors.User.NotFound);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? AppRoles.Guest;

        return Result.Success(new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            role,
            user.EmailConfirmed,
            user.CreatedAt));
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user is null;
    }
}
