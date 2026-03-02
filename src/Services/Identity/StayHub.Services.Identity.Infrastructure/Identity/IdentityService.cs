using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Services.Identity.Domain.Entities;
using StayHub.Services.Identity.Domain.Enums;
using StayHub.Services.Identity.Domain.Events;
using StayHub.Services.Identity.Domain.Repositories;
using StayHub.Shared.Interfaces;
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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IMediator mediator,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
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
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.User.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.User.AccountDisabled);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Account locked for {Email}", email);
            return Result.Failure<AuthenticationResult>(IdentityErrors.User.AccountLocked);
        }

        if (!signInResult.Succeeded)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.User.InvalidCredentials);
        }

        // Get user role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? AppRoles.Guest;

        // Generate JWT access token
        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email!, role);

        // Generate and persist refresh token
        var refreshTokenDays = int.Parse(
            _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7",
            System.Globalization.CultureInfo.InvariantCulture);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            _jwtTokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(refreshTokenDays),
            ipAddress);

        _refreshTokenRepository.Add(refreshToken);

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in from {IpAddress}", user.Id, ipAddress);

        // Publish domain event
        await _mediator.Publish(new UserLoggedInEvent(user.Id, email, ipAddress), cancellationToken);

        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, role, user.EmailConfirmed, user.CreatedAt);

        return Result.Success(new AuthenticationResult(accessToken, refreshToken.Token, expiresAt, userDto));
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (existingToken is null)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.Token.InvalidRefreshToken);
        }

        // Detect reuse of revoked token — potential token theft
        if (existingToken.IsRevoked)
        {
            // Revoke entire token family for this user (security measure)
            _logger.LogWarning("Reuse of revoked refresh token detected for UserId {UserId}. Revoking all tokens.", existingToken.UserId);
            await _refreshTokenRepository.RevokeAllByUserIdAsync(existingToken.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthenticationResult>(IdentityErrors.Token.RefreshTokenRevoked);
        }

        if (existingToken.IsExpired)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.Token.RefreshTokenExpired);
        }

        // Find the user
        var user = await _userManager.FindByIdAsync(existingToken.UserId);
        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthenticationResult>(IdentityErrors.User.NotFound);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? AppRoles.Guest;

        // Generate new token pair
        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email!, role);

        var refreshTokenDays = int.Parse(
            _configuration["Jwt:RefreshTokenExpirationDays"] ?? "7",
            System.Globalization.CultureInfo.InvariantCulture);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            _jwtTokenGenerator.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(refreshTokenDays),
            ipAddress);

        // Rotate: revoke old, persist new
        existingToken.Revoke(ipAddress, newRefreshToken.Token);
        _refreshTokenRepository.Update(existingToken);
        _refreshTokenRepository.Add(newRefreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refreshed for UserId {UserId} from {IpAddress}", user.Id, ipAddress);

        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, role, user.EmailConfirmed, user.CreatedAt);

        return Result.Success(new AuthenticationResult(accessToken, newRefreshToken.Token, expiresAt, userDto));
    }

    /// <inheritdoc />
    public async Task<Result> RevokeRefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (existingToken is null)
        {
            return Result.Failure(IdentityErrors.Token.InvalidRefreshToken);
        }

        if (!existingToken.IsActive)
        {
            return Result.Failure(IdentityErrors.Token.InvalidRefreshToken);
        }

        existingToken.Revoke(ipAddress);
        _refreshTokenRepository.Update(existingToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for UserId {UserId} from {IpAddress}", existingToken.UserId, ipAddress);

        return Result.Success();
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
