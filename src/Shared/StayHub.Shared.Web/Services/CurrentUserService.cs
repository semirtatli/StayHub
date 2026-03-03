using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StayHub.Shared.Interfaces;

namespace StayHub.Shared.Web.Services;

/// <summary>
/// Reads the current user's identity from HttpContext claims.
/// Registered as scoped so each request gets its own instance.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
