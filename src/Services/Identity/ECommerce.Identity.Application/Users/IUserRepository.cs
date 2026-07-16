namespace ECommerce.Identity.Application.Users;

public interface IUserRepository
{
    Task<Domain.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Domain.User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<Domain.User?> GetByExternalIdentityAsync(string provider, string subject, CancellationToken cancellationToken);

    Task<Domain.User?> ReplaceSelfServiceRoleAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken);

    void Add(Domain.User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
