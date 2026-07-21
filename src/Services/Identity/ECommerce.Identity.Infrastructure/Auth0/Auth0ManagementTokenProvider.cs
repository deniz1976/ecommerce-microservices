using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace ECommerce.Identity.Infrastructure.Auth0;

public sealed class Auth0ManagementTokenProvider : IAuth0ManagementTokenProvider, IDisposable
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly Auth0ManagementOptions options;
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? accessToken;
    private DateTimeOffset accessTokenExpiresAt;

    public Auth0ManagementTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<Auth0ManagementOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (accessToken is not null && accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return accessToken;
        }

        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (accessToken is not null && accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return accessToken;
            }

            HttpClient client = httpClientFactory.CreateClient(Auth0RoleSynchronizer.HttpClientName);
            using HttpResponseMessage response = await client.PostAsJsonAsync("oauth/token", new
            {
                grant_type = "client_credentials",
                client_id = options.ClientId,
                client_secret = options.ClientSecret,
                audience = $"https://{options.Domain}/api/v2/"
            }, cancellationToken);

            response.EnsureSuccessStatusCode();
            Auth0TokenResponse token = await response.Content.ReadFromJsonAsync<Auth0TokenResponse>(cancellationToken)
                ?? throw new HttpRequestException("Auth0 returned an empty token response.");

            if (string.IsNullOrWhiteSpace(token.AccessToken) || token.ExpiresIn <= 0)
            {
                throw new HttpRequestException("Auth0 returned an invalid Management API token response.");
            }

            accessToken = token.AccessToken;
            accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
            return accessToken;
        }
        finally
        {
            tokenLock.Release();
        }
    }

    public void Dispose()
    {
        tokenLock.Dispose();
    }
}
