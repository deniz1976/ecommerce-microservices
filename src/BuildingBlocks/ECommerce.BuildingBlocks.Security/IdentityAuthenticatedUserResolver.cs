using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.BuildingBlocks.Security;

public sealed class IdentityAuthenticatedUserResolver : IAuthenticatedUserResolver
{
    private readonly HttpClient httpClient;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<IdentityAuthenticatedUserResolver> logger;
    private Guid? resolvedUserId;
    private bool resolutionAttempted;

    public IdentityAuthenticatedUserResolver(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<IdentityAuthenticatedUserResolver> logger)
    {
        this.httpClient = httpClient;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public async Task<Guid?> ResolveUserIdAsync(
        CancellationToken cancellationToken,
        string? accessToken = null)
    {
        if (resolutionAttempted)
        {
            return resolvedUserId;
        }

        resolutionAttempted = true;
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (string.IsNullOrWhiteSpace(accessToken) &&
            httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        accessToken ??= CustomerAccessTokenReader.Read(httpContext);
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
                logger.LogWarning("Identity user resolution failed with status code {StatusCode}", (int)response.StatusCode);
                return null;
            }

            IdentityProfile? profile = await response.Content.ReadFromJsonAsync<IdentityProfile>(cancellationToken);
            resolvedUserId = profile?.Id;
            return resolvedUserId;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Identity user resolution request failed");
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Identity user resolution returned an invalid response");
            return null;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Identity user resolution timed out");
            return null;
        }
    }
}
