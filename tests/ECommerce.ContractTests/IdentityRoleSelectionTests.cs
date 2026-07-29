using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application;
using ECommerce.Identity.Application.Commands.SelectExternalUserRole;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace ECommerce.ContractTests;

public sealed class IdentityRoleSelectionTests
{
    [Fact]
    public async Task Auth0FailureLeavesLocalRoleUnchanged()
    {
        StubGetOrCreateExternalUserHandler getOrCreateHandler = new(
            Result<UserResponse>.Success(
                new UserResponse(
                    Guid.NewGuid(),
                    "user@example.test",
                    "Test User",
                    [UserRoleNames.Customer],
                    true)));
        TrackingSelfServiceRoleWriter roleWriter = new();
        SelectExternalUserRoleCommandHandler handler = new(
            getOrCreateHandler,
            new RejectingRoleSynchronizer(),
            roleWriter,
            new TrackingRoleReconciliationQueue(),
            NullLogger<SelectExternalUserRoleCommandHandler>.Instance);

        Result<UserResponse> result = await handler.HandleAsync(
            new SelectExternalUserRoleCommand(
                new ExternalUserProfile("Auth0", "auth0|user-1", "user@example.test", "Test User"),
                UserRoleNames.Seller),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrorCodes.ExternalRoleSynchronizationFailed, result.Error?.Code);
        Assert.False(roleWriter.ReplaceCalled);
    }

    [Fact]
    public async Task LocalPersistenceFailureRestoresPreviousExternalRole()
    {
        StubGetOrCreateExternalUserHandler getOrCreateHandler = CreateCustomerHandler();
        TrackingRoleSynchronizer roleSynchronizer = new(
            ExternalRoleSynchronizationResult.Succeeded,
            ExternalRoleSynchronizationResult.Succeeded);
        TrackingSelfServiceRoleWriter roleWriter = new()
        {
            ExceptionToThrow = new InvalidOperationException("Synthetic local persistence failure.")
        };
        SelectExternalUserRoleCommandHandler handler = new(
            getOrCreateHandler,
            roleSynchronizer,
            roleWriter,
            new TrackingRoleReconciliationQueue(),
            NullLogger<SelectExternalUserRoleCommandHandler>.Instance);

        Result<UserResponse> result = await handler.HandleAsync(
            new SelectExternalUserRoleCommand(
                new ExternalUserProfile("Auth0", "auth0|user-2", "user@example.test", "Test User"),
                UserRoleNames.Seller),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrorCodes.ExternalRoleSynchronizationFailed, result.Error?.Code);
        Assert.True(roleWriter.ReplaceCalled);
        Assert.Equal(
            [(UserRoleNames.Seller, UserRoleNames.Customer), (UserRoleNames.Customer, UserRoleNames.Seller)],
            roleSynchronizer.Transitions);
    }

    [Fact]
    public async Task MissingLocalUserAfterExternalSyncRestoresPreviousRole()
    {
        StubGetOrCreateExternalUserHandler getOrCreateHandler = CreateCustomerHandler();
        TrackingRoleSynchronizer roleSynchronizer = new(
            ExternalRoleSynchronizationResult.Succeeded,
            ExternalRoleSynchronizationResult.Succeeded);
        TrackingSelfServiceRoleWriter roleWriter = new();
        SelectExternalUserRoleCommandHandler handler = new(
            getOrCreateHandler,
            roleSynchronizer,
            roleWriter,
            new TrackingRoleReconciliationQueue(),
            NullLogger<SelectExternalUserRoleCommandHandler>.Instance);

        Result<UserResponse> result = await handler.HandleAsync(
            new SelectExternalUserRoleCommand(
                new ExternalUserProfile("Auth0", "auth0|user-3", "user@example.test", "Test User"),
                UserRoleNames.Seller),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityErrorCodes.ExternalRoleSynchronizationFailed, result.Error?.Code);
        Assert.Equal(
            [(UserRoleNames.Seller, UserRoleNames.Customer), (UserRoleNames.Customer, UserRoleNames.Seller)],
            roleSynchronizer.Transitions);
    }

    [Fact]
    public async Task UnrestoredExternalFailureQueuesPreviousRoleForReconciliation()
    {
        StubGetOrCreateExternalUserHandler getOrCreateHandler = CreateCustomerHandler();
        TrackingRoleSynchronizer roleSynchronizer = new(
            ExternalRoleSynchronizationResult.ReconciliationRequired);
        TrackingSelfServiceRoleWriter roleWriter = new();
        TrackingRoleReconciliationQueue reconciliationQueue = new();
        SelectExternalUserRoleCommandHandler handler = new(
            getOrCreateHandler,
            roleSynchronizer,
            roleWriter,
            reconciliationQueue,
            NullLogger<SelectExternalUserRoleCommandHandler>.Instance);

        Result<UserResponse> result = await handler.HandleAsync(
            new SelectExternalUserRoleCommand(
                new ExternalUserProfile("Auth0", "auth0|user-4", "user@example.test", "Test User"),
                UserRoleNames.Seller),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(roleWriter.ReplaceCalled);
        Assert.Equal(
            [(UserRoleNames.Customer, UserRoleNames.Seller)],
            reconciliationQueue.Items);
    }

    private static StubGetOrCreateExternalUserHandler CreateCustomerHandler()
    {
        return new StubGetOrCreateExternalUserHandler(
            Result<UserResponse>.Success(
                new UserResponse(
                    Guid.NewGuid(),
                    "user@example.test",
                    "Test User",
                    [UserRoleNames.Customer],
                    true)));
    }
}
