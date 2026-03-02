using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.ChangePassword;

/// <summary>
/// Handles ChangePasswordCommand by delegating to IIdentityService.
/// On success, all existing refresh tokens are revoked as a security measure.
/// </summary>
internal sealed class ChangePasswordCommandHandler(IIdentityService identityService)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return await identityService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}
