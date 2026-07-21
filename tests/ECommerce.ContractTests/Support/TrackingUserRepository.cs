using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;

namespace ECommerce.ContractTests;

internal sealed class TrackingUserRepository(User externalUser) : IUserRepository
{
    public bool ReplaceRoleCalled { get; private set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<User?>(null);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult<User?>(null);

    public Task<User?> GetByExternalIdentityAsync(
        string provider,
        string subject,
        CancellationToken cancellationToken) => Task.FromResult<User?>(externalUser);

    public Task<User?> ReplaceSelfServiceRoleAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken)
    {
        ReplaceRoleCalled = true;
        return Task.FromResult<User?>(externalUser);
    }

    public void Add(User user) => throw new InvalidOperationException();

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
