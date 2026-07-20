using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Identity.Infrastructure.Auth0;

public sealed class Auth0RoleSynchronizer : IExternalRoleSynchronizer, IDisposable
{
    public const string HttpClientName = "Auth0Management";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly Auth0ManagementOptions options;
    private readonly ILogger<Auth0RoleSynchronizer> logger;
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? accessToken;
    private DateTimeOffset accessTokenExpiresAt;

    public Auth0RoleSynchronizer(
        IHttpClientFactory httpClientFactory,
        IOptions<Auth0ManagementOptions> options,
        ILogger<Auth0RoleSynchronizer> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<bool> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return true;
        }

        string desiredRoleId = GetRoleId(role);
        string roleToRemoveId = string.Equals(role, UserRoleNames.Customer, StringComparison.Ordinal)
            ? options.SellerRoleId
            : options.CustomerRoleId;

        try
        {
            string token = await GetAccessTokenAsync(cancellationToken);
            HttpClient client = httpClientFactory.CreateClient(HttpClientName);
            string userRolesPath = $"api/v2/users/{Uri.EscapeDataString(externalSubject)}/roles";

            using HttpRequestMessage removeRequest = CreateRoleRequest(HttpMethod.Delete, userRolesPath, roleToRemoveId, token);
            using HttpResponseMessage removeResponse = await client.SendAsync(removeRequest, cancellationToken);
            if (!removeResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Auth0 rejected removal of the previous self-service role with status {StatusCode}.",
                    (int)removeResponse.StatusCode);
                return false;
            }

            using HttpRequestMessage assignRequest = CreateRoleRequest(HttpMethod.Post, userRolesPath, desiredRoleId, token);
            using HttpResponseMessage assignResponse = await client.SendAsync(assignRequest, cancellationToken);
            if (!assignResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Auth0 rejected assignment of the selected self-service role with status {StatusCode}.",
                    (int)assignResponse.StatusCode);
                return false;
            }

            return true;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Auth0 role synchronization failed because the Management API was unavailable.");
            return false;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Auth0 role synchronization exceeded its configured timeout.");
            return false;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Auth0 returned an invalid Management API token response.");
            return false;
        }
    }

    public void Dispose()
    {
        tokenLock.Dispose();
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
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

            HttpClient client = httpClientFactory.CreateClient(HttpClientName);
            using HttpResponseMessage response = await client.PostAsJsonAsync("oauth/token", new
            {
                grant_type = "client_credentials",
                client_id = options.ClientId,
                client_secret = options.ClientSecret,
                audience = $"https://{options.Domain}/api/v2/"
            }, cancellationToken);

            response.EnsureSuccessStatusCode();
            TokenResponse token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
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

    private string GetRoleId(string role)
    {
        return role switch
        {
            UserRoleNames.Customer => options.CustomerRoleId,
            UserRoleNames.Seller => options.SellerRoleId,
            _ => throw new ArgumentException("Role is not available for Auth0 self-service synchronization.", nameof(role))
        };
    }

    private static HttpRequestMessage CreateRoleRequest(
        HttpMethod method,
        string path,
        string roleId,
        string token)
    {
        HttpRequestMessage request = new(method, path)
        {
            Content = JsonContent.Create(new { roles = new[] { roleId } })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
