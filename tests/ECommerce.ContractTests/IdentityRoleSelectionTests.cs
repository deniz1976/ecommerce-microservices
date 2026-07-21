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

}
