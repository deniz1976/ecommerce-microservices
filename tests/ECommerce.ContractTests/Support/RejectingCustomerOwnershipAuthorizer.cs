using System.Security.Claims;
using ECommerce.BuildingBlocks.Security;

namespace ECommerce.ContractTests.Support;

internal sealed class RejectingCustomerOwnershipAuthorizer : ICustomerOwnershipAuthorizer
{
    public int InvocationCount { get; private set; }

    public Task<bool> CanAccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        InvocationCount++;
        return Task.FromResult(false);
    }

    public Task<bool> CanAccessAsync(
        Guid customerId,
        ClaimsPrincipal? principal,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        InvocationCount++;
        return Task.FromResult(false);
    }
}
