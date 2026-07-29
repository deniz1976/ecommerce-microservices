namespace ECommerce.BuildingBlocks.Security;

public interface IAuthenticatedUserResolver
{
    Task<Guid?> ResolveUserIdAsync(
        CancellationToken cancellationToken,
        string? accessToken = null);
}
