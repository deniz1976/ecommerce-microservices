namespace ECommerce.Identity.Application.Users;

internal sealed class DisabledExternalRoleSynchronizer : IExternalRoleSynchronizer
{
    public Task<bool> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
