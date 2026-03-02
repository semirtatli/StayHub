using StayHub.Services.Identity.Application.Abstractions;
using StayHub.Shared.CQRS;

namespace StayHub.Services.Identity.Application.Features.GetUser;

/// <summary>
/// Query to get a user by their ID.
/// Used by profile endpoints and admin user management.
/// </summary>
public sealed record GetUserByIdQuery(string UserId) : IQuery<UserDto>;
