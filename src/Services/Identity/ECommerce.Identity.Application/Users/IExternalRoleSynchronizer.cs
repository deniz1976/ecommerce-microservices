namespace ECommerce.Identity.Application.Users;

public interface IExternalRoleSynchronizer
{
    Task<ExternalRoleSynchronizationResult> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        string? previousRole,
        CancellationToken cancellationToken);
}
