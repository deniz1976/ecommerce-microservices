using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.Logging;

namespace ECommerce.Basket.Infrastructure.Catalog;

public sealed class CatalogProductReader : IProductCatalogReader
{
    private const int ActiveProductStatus = 1;
    private readonly HttpClient httpClient;
    private readonly ILogger<CatalogProductReader> logger;

    public CatalogProductReader(HttpClient httpClient, ILogger<CatalogProductReader> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task<Result<CatalogProductSnapshot>> GetActiveProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(
                $"/api/v1/products/{productId}",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return ProductNotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return CatalogUnavailable();
            }

            CatalogProductSnapshot? product = await response.Content.ReadFromJsonAsync<CatalogProductSnapshot>(cancellationToken);

            return product is null || product.Status != ActiveProductStatus
                ? ProductNotFound()
                : Result<CatalogProductSnapshot>.Success(product);
        }
        catch (HttpRequestException exception)
        {
            LogCatalogFailure(productId, exception);
            return CatalogUnavailable();
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogCatalogFailure(productId, exception);
            return CatalogUnavailable();
        }
        catch (JsonException exception)
        {
            LogCatalogFailure(productId, exception);
            return CatalogUnavailable();
        }
    }

    private static Result<CatalogProductSnapshot> ProductNotFound() =>
        Result<CatalogProductSnapshot>.Failure(new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));

    private static Result<CatalogProductSnapshot> CatalogUnavailable() =>
        Result<CatalogProductSnapshot>.Failure(
            new Error(BasketErrorCodes.ProductCatalogUnavailable, BasketErrorCodes.ProductCatalogUnavailable));

    private void LogCatalogFailure(Guid productId, Exception exception)
    {
        logger.LogWarning(
            exception,
            "Catalog product lookup failed for product {ProductId}.",
            productId);
    }
}
