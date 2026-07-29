using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class TrackingRoleSynchronizer : IExternalRoleSynchronizer
{
    private readonly Queue<ExternalRoleSynchronizationResult> results;

    public TrackingRoleSynchronizer(params ExternalRoleSynchronizationResult[] results)
    {
        this.results = new Queue<ExternalRoleSynchronizationResult>(results);
    }

    public List<(string Role, string? PreviousRole)> Transitions { get; } = [];

    public Task<ExternalRoleSynchronizationResult> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        string? previousRole,
        CancellationToken cancellationToken)
    {
        Transitions.Add((role, previousRole));
        return Task.FromResult(
            results.Count == 0
                ? ExternalRoleSynchronizationResult.Succeeded
                : results.Dequeue());
    }
}
