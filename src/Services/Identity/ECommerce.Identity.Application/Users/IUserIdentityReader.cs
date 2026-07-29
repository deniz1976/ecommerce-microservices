namespace ECommerce.Identity.Application.Users;

public interface IUserIdentityReader
{
    Task<Guid?> FindIdByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Guid?> FindIdByExternalIdentityAsync(
        string provider,
        string subject,
        CancellationToken cancellationToken);
}
