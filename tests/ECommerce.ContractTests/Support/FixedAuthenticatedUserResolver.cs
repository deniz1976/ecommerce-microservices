using ECommerce.BuildingBlocks.Security;

namespace ECommerce.ContractTests;

internal sealed class FixedAuthenticatedUserResolver : IAuthenticatedUserResolver
{
    public Task<Guid?> ResolveUserIdAsync(CancellationToken cancellationToken) =>
        Task.FromResult<Guid?>(Guid.NewGuid());
}
