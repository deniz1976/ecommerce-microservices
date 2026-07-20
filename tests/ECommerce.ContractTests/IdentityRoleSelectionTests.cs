using ECommerce.Identity.Application;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;

namespace ECommerce.ContractTests;

public sealed class IdentityRoleSelectionTests
{
    [Fact]
    public async Task Auth0FailureLeavesLocalRoleUnchanged()
    {
        User user = new(Guid.NewGuid(), "user@example.test", "Test User", string.Empty, "Auth0", "auth0|user-1");
        TrackingUserRepository repository = new(user);
        UserService service = new(repository, new UnusedPasswordHashService(), new RejectingRoleSynchronizer());

        var result = await service.SelectExternalUserRoleAsync(
            new ExternalUserProfile("Auth0", "auth0|user-1", "user@example.test", "Test User"),
            new SelectUserRoleRequest(UserRoleNames.Seller),
            default);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrorCodes.ExternalRoleSynchronizationFailed, result.Error?.Code);
        Assert.False(repository.ReplaceRoleCalled);
    }

    private sealed class RejectingRoleSynchronizer : IExternalRoleSynchronizer
    {
        public Task<bool> SynchronizeSelfServiceRoleAsync(
            string externalSubject,
            string role,
            CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class UnusedPasswordHashService : IPasswordHashService
    {
        public string HashPassword(User user, string password) => throw new InvalidOperationException();
    }

    private sealed class TrackingUserRepository(User externalUser) : IUserRepository
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
}
