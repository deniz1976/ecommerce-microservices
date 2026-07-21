using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class RejectingRoleSynchronizer : IExternalRoleSynchronizer
{
    public Task<bool> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        CancellationToken cancellationToken) => Task.FromResult(false);
}
