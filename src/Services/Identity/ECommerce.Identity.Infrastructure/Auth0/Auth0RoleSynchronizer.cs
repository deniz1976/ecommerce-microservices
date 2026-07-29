using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Identity.Infrastructure.Auth0;

public sealed class Auth0RoleSynchronizer : IExternalRoleSynchronizer
{
    public const string HttpClientName = "Auth0Management";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly IAuth0ManagementTokenProvider tokenProvider;
    private readonly Auth0ManagementOptions options;
    private readonly ILogger<Auth0RoleSynchronizer> logger;

    public Auth0RoleSynchronizer(
        IHttpClientFactory httpClientFactory,
        IAuth0ManagementTokenProvider tokenProvider,
        IOptions<Auth0ManagementOptions> options,
        ILogger<Auth0RoleSynchronizer> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.tokenProvider = tokenProvider;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<ExternalRoleSynchronizationResult> SynchronizeSelfServiceRoleAsync(
        string externalSubject,
        string role,
        string? previousRole,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return ExternalRoleSynchronizationResult.Succeeded;
        }

        string desiredRoleId = GetRoleId(role);
        string roleToRemoveId = string.Equals(role, UserRoleNames.Customer, StringComparison.Ordinal)
            ? options.SellerRoleId
            : options.CustomerRoleId;

        try
        {
            string token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
            HttpClient client = httpClientFactory.CreateClient(HttpClientName);
            string userRolesPath = $"api/v2/users/{Uri.EscapeDataString(externalSubject)}/roles";

            using HttpRequestMessage removeRequest = CreateRoleRequest(HttpMethod.Delete, userRolesPath, roleToRemoveId, token);
            using HttpResponseMessage removeResponse = await client.SendAsync(removeRequest, cancellationToken);
            if (!removeResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Auth0 rejected removal of the previous self-service role with status {StatusCode}.",
                    (int)removeResponse.StatusCode);
                return ExternalRoleSynchronizationResult.FailedRestored;
            }

            using HttpRequestMessage assignRequest = CreateRoleRequest(HttpMethod.Post, userRolesPath, desiredRoleId, token);
            using HttpResponseMessage assignResponse = await client.SendAsync(assignRequest, cancellationToken);
            if (!assignResponse.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Auth0 rejected assignment of the selected self-service role with status {StatusCode}.",
                    (int)assignResponse.StatusCode);
                bool restored = await RestorePreviousRoleAsync(
                    client,
                    userRolesPath,
                    previousRole,
                    role,
                    token,
                    cancellationToken);
                return restored
                    ? ExternalRoleSynchronizationResult.FailedRestored
                    : ExternalRoleSynchronizationResult.ReconciliationRequired;
            }

            return ExternalRoleSynchronizationResult.Succeeded;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Auth0 role synchronization failed because the Management API was unavailable.");
            return ExternalRoleSynchronizationResult.FailedRestored;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Auth0 role synchronization exceeded its configured timeout.");
            return ExternalRoleSynchronizationResult.FailedRestored;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Auth0 returned an invalid Management API token response.");
            return ExternalRoleSynchronizationResult.FailedRestored;
        }
    }

    private async Task<bool> RestorePreviousRoleAsync(
        HttpClient client,
        string userRolesPath,
        string? previousRole,
        string desiredRole,
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(previousRole) ||
            string.Equals(previousRole, desiredRole, StringComparison.Ordinal))
        {
            return true;
        }

        string previousRoleId = GetRoleId(previousRole);
        using HttpRequestMessage restoreRequest = CreateRoleRequest(
            HttpMethod.Post,
            userRolesPath,
            previousRoleId,
            token);
        using HttpResponseMessage restoreResponse = await client.SendAsync(
            restoreRequest,
            cancellationToken);
        if (!restoreResponse.IsSuccessStatusCode)
        {
            logger.LogCritical(
                "Auth0 rejected restoration of the previous self-service role with status {StatusCode}. Durable reconciliation is required.",
                (int)restoreResponse.StatusCode);
            return false;
        }

        return true;
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
}
