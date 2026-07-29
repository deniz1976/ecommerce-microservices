using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class RejectingRoleSynchronizer : IExternalRoleSynchronizer
{
    public Task<ExternalRoleSynchronizationResult> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        string? previousRole,
        CancellationToken cancellationToken) =>
        Task.FromResult(ExternalRoleSynchronizationResult.FailedRestored);
}
