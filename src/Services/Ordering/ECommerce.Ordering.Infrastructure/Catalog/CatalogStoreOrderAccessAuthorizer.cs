using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Ordering.Application.Orders;
using Microsoft.Extensions.Logging;

namespace ECommerce.Ordering.Infrastructure.Catalog;

public sealed class CatalogStoreOrderAccessAuthorizer(
    HttpClient httpClient,
    ILogger<CatalogStoreOrderAccessAuthorizer> logger)
    : IStoreOrderAccessAuthorizer
{
    public async Task<StoreOrderAccessResult> AuthorizeAsync(
        Guid storeId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (storeId == Guid.Empty || string.IsNullOrWhiteSpace(accessToken))
        {
            return StoreOrderAccessResult.Denied;
        }

        using HttpRequestMessage request = new(HttpMethod.Get, "/api/v1/stores/mine");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                CatalogOwnedStoreResponse[] stores = await response.Content
                    .ReadFromJsonAsync<CatalogOwnedStoreResponse[]>(cancellationToken) ?? [];
                return stores.Any(store => store.Id == storeId)
                    ? StoreOrderAccessResult.Granted
                    : StoreOrderAccessResult.Denied;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return StoreOrderAccessResult.Denied;
            }

            logger.LogWarning(
                "Catalog store ownership check returned status {StatusCode} for store {StoreId}.",
                (int)response.StatusCode,
                storeId);
            return StoreOrderAccessResult.DependencyUnavailable;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Catalog store ownership check timed out for store {StoreId}.", storeId);
            return StoreOrderAccessResult.DependencyUnavailable;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Catalog store ownership check failed for store {StoreId}.",
                storeId);
            return StoreOrderAccessResult.DependencyUnavailable;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "Catalog store ownership response was invalid for store {StoreId}.",
                storeId);
            return StoreOrderAccessResult.DependencyUnavailable;
        }
    }
}
