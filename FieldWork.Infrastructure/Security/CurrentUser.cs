
using System.Security.Claims;
using FieldWork.Application.Security;
using Microsoft.AspNetCore.Http;

namespace FieldWork.Infrastructure.Security;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated
        ?? false;

    public Guid UserId =>
        GetGuidClaim(ClaimTypes.NameIdentifier);

    public Guid TenantId =>
        GetGuidClaim("tenantId");

    public string Username =>
        GetStringClaim("username");

    public string Role =>
        GetStringClaim(ClaimTypes.Role);

    private Guid GetGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(claimType)?
            .Value;

        if (!Guid.TryParse(value, out var result))
            throw new InvalidOperationException(
                $"Required claim '{claimType}' is missing or invalid.");

        return result;
    }

    private string GetStringClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?
            .User
            .FindFirst(claimType)?
            .Value
            ?? string.Empty;
    }
}

