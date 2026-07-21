using System.Security.Claims;
using ECommerce.BuildingBlocks.Security;

namespace ECommerce.ContractTests;

internal sealed class AllowAllCustomerOwnershipAuthorizer : ICustomerOwnershipAuthorizer
{
    public Task<bool> CanAccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CanAccessAsync(
        Guid customerId,
        ClaimsPrincipal? principal,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
