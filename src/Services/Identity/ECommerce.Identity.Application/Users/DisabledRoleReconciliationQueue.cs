namespace ECommerce.Identity.Application.Users;

internal sealed class DisabledRoleReconciliationQueue : IRoleReconciliationQueue
{
    public Task EnqueueAsync(
        string externalSubject,
        string desiredRole,
        string? currentExternalRole,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
