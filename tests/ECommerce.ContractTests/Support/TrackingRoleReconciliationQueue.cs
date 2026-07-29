using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class TrackingRoleReconciliationQueue : IRoleReconciliationQueue
{
    public List<(string DesiredRole, string? CurrentRole)> Items { get; } = [];

    public Task EnqueueAsync(
        string externalSubject,
        string desiredRole,
        string? currentExternalRole,
        CancellationToken cancellationToken)
    {
        Items.Add((desiredRole, currentExternalRole));
        return Task.CompletedTask;
    }
}
