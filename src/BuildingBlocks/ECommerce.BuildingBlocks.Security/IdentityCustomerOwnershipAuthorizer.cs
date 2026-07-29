using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ECommerce.BuildingBlocks.Security;

public sealed class IdentityCustomerOwnershipAuthorizer : ICustomerOwnershipAuthorizer
{
    private readonly IAuthenticatedUserResolver authenticatedUserResolver;
    private readonly IHttpContextAccessor httpContextAccessor;

    public IdentityCustomerOwnershipAuthorizer(
        IAuthenticatedUserResolver authenticatedUserResolver,
        IHttpContextAccessor httpContextAccessor)
    {
        this.authenticatedUserResolver = authenticatedUserResolver;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> CanAccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        return await CanAccessAsync(
            customerId,
            httpContext?.User,
            CustomerAccessTokenReader.Read(httpContext),
            cancellationToken);
    }

    public async Task<bool> CanAccessAsync(
        Guid customerId,
        ClaimsPrincipal? principal,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (principal.IsInRole(ApplicationRoles.Admin) ||
            ClaimsPrincipalPermissionEvaluator.HasPermission(
                principal,
                ApplicationPermissions.ActAsCustomer))
        {
            return true;
        }

        Guid? currentCustomerId = await authenticatedUserResolver.ResolveUserIdAsync(
            cancellationToken,
            accessToken);
        return currentCustomerId == customerId;
    }
}
