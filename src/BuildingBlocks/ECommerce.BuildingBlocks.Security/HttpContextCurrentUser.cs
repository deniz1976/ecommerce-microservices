using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BuildingBlocks.Security;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("sub");

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email");

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}
