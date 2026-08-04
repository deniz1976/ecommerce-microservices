using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class StubGetOrCreateExternalUserHandler
    : IExternalUserProvisioningService
{
    private readonly Result<UserResponse> result;

    public StubGetOrCreateExternalUserHandler(Result<UserResponse> result)
    {
        this.result = result;
    }

    public Task<Result<UserResponse>> GetOrCreateExternalAsync(
        ExternalUserProfile profile,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(result);
    }
}
