using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Identity.Application.Users;

public interface IExternalUserProvisioningService
{
    Task<Result<UserResponse>> GetOrCreateExternalAsync(
        ExternalUserProfile profile,
        CancellationToken cancellationToken);
}
