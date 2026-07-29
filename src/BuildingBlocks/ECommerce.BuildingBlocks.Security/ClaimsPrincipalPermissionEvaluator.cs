using System.Security.Claims;

namespace ECommerce.BuildingBlocks.Security;

internal static class ClaimsPrincipalPermissionEvaluator
{
    public static bool HasPermission(ClaimsPrincipal principal, string permission)
    {
        return principal.Claims
            .Where(claim => claim.Type is "permissions" or "scope")
            .SelectMany(claim => claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Contains(permission, StringComparer.Ordinal);
    }
}
