using System.Security.Claims;
using Hr360.Application.Interfaces;

namespace Hr360.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            return user?.FindFirstValue("oid")
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous";
        }
    }

    public string DisplayName =>
        accessor.HttpContext?.User.FindFirstValue("name")
        ?? accessor.HttpContext?.User.Identity?.Name
        ?? UserId;

    public bool IsInRole(string role) => accessor.HttpContext?.User.IsInRole(role) == true;
}
