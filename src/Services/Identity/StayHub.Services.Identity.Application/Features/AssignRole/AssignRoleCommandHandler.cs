using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.AssignRole;

/// <summary>
/// Handles role assignment — delegates to IIdentityService.
/// Authorization is enforced at the controller level (Admin policy).
/// </summary>
public sealed class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AssignRoleCommandHandler> _logger;

    public AssignRoleCommandHandler(
        IIdentityService identityService,
        ILogger<AssignRoleCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignRoleCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Role assignment: {Role} → User {UserId} by {AssignedBy}",
            request.Role, request.UserId, request.AssignedByUserId);

        var result = await _identityService.AssignRoleAsync(
            request.UserId,
            request.Role,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Role assignment failed for User {UserId}: {ErrorCode}",
                request.UserId, result.Error.Code);
        }

        return result;
    }
}
