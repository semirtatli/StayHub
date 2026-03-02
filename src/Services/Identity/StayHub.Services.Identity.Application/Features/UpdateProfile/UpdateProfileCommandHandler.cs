using Microsoft.Extensions.Logging;
using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.UpdateProfile;

/// <summary>
/// Handles profile updates — delegates to IIdentityService.
/// </summary>
public sealed class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserDto>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IIdentityService identityService,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Profile update for UserId {UserId}", request.UserId);

        return await _identityService.UpdateProfileAsync(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            cancellationToken);
    }
}
