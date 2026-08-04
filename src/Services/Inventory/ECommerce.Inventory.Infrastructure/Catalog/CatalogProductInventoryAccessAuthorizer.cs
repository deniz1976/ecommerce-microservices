using System.Net;
using System.Net.Http.Headers;
using ECommerce.Inventory.Application.Inventory;
using Microsoft.Extensions.Logging;

namespace ECommerce.Inventory.Infrastructure.Catalog;

public sealed class CatalogProductInventoryAccessAuthorizer(
    HttpClient httpClient,
    ILogger<CatalogProductInventoryAccessAuthorizer> logger)
    : IProductInventoryAccessAuthorizer
{
    public async Task<ProductInventoryAccessResult> AuthorizeAsync(
        Guid productId,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ProductInventoryAccessResult.Denied;
        }

        using HttpRequestMessage request = new(
            HttpMethod.Get,
            $"/api/v1/products/manage/{productId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => ProductInventoryAccessResult.Granted,
                HttpStatusCode.Unauthorized or
                HttpStatusCode.Forbidden or
                HttpStatusCode.NotFound => ProductInventoryAccessResult.Denied,
                _ => LogUnavailable(productId, response.StatusCode)
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Catalog ownership check timed out for product {ProductId}.",
                productId);
            return ProductInventoryAccessResult.DependencyUnavailable;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Catalog ownership check failed for product {ProductId}.",
                productId);
            return ProductInventoryAccessResult.DependencyUnavailable;
        }
    }

    private ProductInventoryAccessResult LogUnavailable(
        Guid productId,
        HttpStatusCode statusCode)
    {
        logger.LogWarning(
            "Catalog ownership check returned status {StatusCode} for product {ProductId}.",
            (int)statusCode,
            productId);
        return ProductInventoryAccessResult.DependencyUnavailable;
    }
}
