namespace ECommerce.Identity.Infrastructure.Auth0;

public interface IAuth0ManagementTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}
