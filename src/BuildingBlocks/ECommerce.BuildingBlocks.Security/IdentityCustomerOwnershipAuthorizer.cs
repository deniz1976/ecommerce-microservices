using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.BuildingBlocks.Security;

public sealed class IdentityCustomerOwnershipAuthorizer : ICustomerOwnershipAuthorizer
{
    private readonly HttpClient httpClient;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<IdentityCustomerOwnershipAuthorizer> logger;
    private Guid? resolvedCustomerId;
    private bool resolutionAttempted;

    public IdentityCustomerOwnershipAuthorizer(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<IdentityCustomerOwnershipAuthorizer> logger)
    {
        this.httpClient = httpClient;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public async Task<bool> CanAccessAsync(Guid customerId, CancellationToken cancellationToken)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        return await CanAccessAsync(
            customerId,
            httpContext?.User,
            CustomerAccessTokenReader.Read(httpContext),
            cancellationToken);
    }

    public async Task<bool> CanAccessAsync(
        Guid customerId,
        ClaimsPrincipal? principal,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (principal.IsInRole(ApplicationRoles.Admin) ||
            HasPermission(principal, ApplicationPermissions.ActAsCustomer))
        {
            return true;
        }

        Guid? currentCustomerId = await ResolveCurrentCustomerIdAsync(accessToken, cancellationToken);
        return currentCustomerId == customerId;
    }

    private async Task<Guid?> ResolveCurrentCustomerIdAsync(
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (resolutionAttempted)
        {
            return resolvedCustomerId;
        }

        resolutionAttempted = true;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        using HttpRequestMessage request = new(HttpMethod.Get, "api/v1/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Identity customer resolution failed with status code {StatusCode}",
                    (int)response.StatusCode);
                return null;
            }

            IdentityProfile? profile = await response.Content.ReadFromJsonAsync<IdentityProfile>(cancellationToken);
            resolvedCustomerId = profile?.Id;
            return resolvedCustomerId;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Identity customer resolution request failed");
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Identity customer resolution returned an invalid response");
            return null;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Identity customer resolution timed out");
            return null;
        }
    }

    private static bool HasPermission(ClaimsPrincipal principal, string permission)
    {
        return principal.Claims
            .Where(claim => claim.Type is "permissions" or "scope")
            .SelectMany(claim => claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Contains(permission, StringComparer.Ordinal);
    }

    private sealed record IdentityProfile(Guid Id);
}
