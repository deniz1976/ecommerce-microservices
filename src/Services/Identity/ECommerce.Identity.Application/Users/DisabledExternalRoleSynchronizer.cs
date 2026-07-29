namespace ECommerce.Identity.Application.Users;

internal sealed class DisabledExternalRoleSynchronizer : IExternalRoleSynchronizer
{
    public Task<ExternalRoleSynchronizationResult> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        string? previousRole,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(ExternalRoleSynchronizationResult.Succeeded);
    }
}
