using System.Security.Claims;

namespace ECommerce.BuildingBlocks.Security;

public interface ICustomerOwnershipAuthorizer
{
    Task<bool> CanAccessAsync(Guid customerId, CancellationToken cancellationToken);

    Task<bool> CanAccessAsync(
        Guid customerId,
        ClaimsPrincipal? principal,
        string? accessToken,
        CancellationToken cancellationToken);
}
