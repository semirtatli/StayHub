using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.ConfirmEmail;

/// <summary>
/// Handles ConfirmEmailCommand by delegating to IIdentityService.ConfirmEmailAsync.
/// The Identity infrastructure validates the token against ASP.NET Core Identity's
/// token provider and marks the email as confirmed.
/// Publishes EmailConfirmedEvent on success.
/// </summary>
internal sealed class ConfirmEmailCommandHandler(IIdentityService identityService)
    : ICommandHandler<ConfirmEmailCommand>
{
    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        return await identityService.ConfirmEmailAsync(
            request.UserId,
            request.Token,
            cancellationToken);
    }
}
