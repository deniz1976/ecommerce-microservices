namespace ECommerce.Identity.Application.Users;

public interface IRoleReconciliationQueue
{
    Task EnqueueAsync(
        string externalSubject,
        string desiredRole,
        string? currentExternalRole,
        CancellationToken cancellationToken);
}
