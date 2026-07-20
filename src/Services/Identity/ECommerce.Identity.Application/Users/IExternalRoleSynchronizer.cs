namespace ECommerce.Identity.Application.Users;

public interface IExternalRoleSynchronizer
{
    Task<bool> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        CancellationToken cancellationToken);
}
