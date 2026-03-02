using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;
using StayHub.Shared.Result;

namespace StayHub.Services.Identity.Application.Features.GetUser;

/// <summary>
/// Handles GetUserByIdQuery — delegates to IIdentityService for user lookup.
/// </summary>
public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _identityService.GetUserByIdAsync(request.UserId, cancellationToken);
    }
}
