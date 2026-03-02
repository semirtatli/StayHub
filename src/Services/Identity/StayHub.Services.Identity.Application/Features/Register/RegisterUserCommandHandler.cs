using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.Register;

/// <summary>
/// Handles user registration.
///
/// Pipeline: ValidationBehavior (FluentValidation) → LoggingBehavior → this handler.
/// By the time we reach here, the command has already passed validation.
///
/// Delegates to IIdentityService for the actual Identity operations, which:
/// 1. Checks email uniqueness
/// 2. Creates ASP.NET Core Identity user with hashed password
/// 3. Assigns the requested role
/// 4. Publishes UserRegisteredEvent for downstream handlers (welcome email, analytics, etc.)
/// </summary>
public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing registration for {Email}", request.Email);

        var result = await _identityService.RegisterUserAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Registration failed for {Email}: {ErrorCode}", request.Email, result.Error.Code);
            return Result.Failure<RegisterUserResponse>(result.Error);
        }

        var response = new RegisterUserResponse(
            result.Value,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Role);

        _logger.LogInformation("Registration succeeded for {Email}, UserId: {UserId}", request.Email, result.Value);

        return Result.Success(response);
    }
}
