using System.Security.Claims;

namespace ECommerce.BuildingBlocks.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Email { get; }

    ClaimsPrincipal? Principal { get; }

    bool IsInRole(string role);
}
