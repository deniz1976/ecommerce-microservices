using System.Net;
using System.Net.Http.Json;
using ECommerce.Basket.Application;
using ECommerce.Basket.Application.Baskets;
using ECommerce.Basket.Infrastructure.Catalog;
using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace ECommerce.ContractTests;

public sealed class CatalogProductReaderTests
{
    [Fact]
    public async Task GetActiveProductReturnsCanonicalCatalogData()
    {
        Guid productId = Guid.NewGuid();
        using HttpClient client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                id = productId,
                name = "Catalog product",
                price = 79.95m,
                currency = "TRY",
                status = 1
            })
        });
        CatalogProductReader reader = CreateReader(client);

        Result<CatalogProductSnapshot> result = await reader.GetActiveProductAsync(productId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Catalog product", result.Value!.Name);
        Assert.Equal(79.95m, result.Value.Price);
    }

    [Fact]
    public async Task GetActiveProductHidesInactiveProducts()
    {
        Guid productId = Guid.NewGuid();
        using HttpClient client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                id = productId,
                name = "Inactive product",
                price = 10m,
                currency = "USD",
                status = 2
            })
        });
        CatalogProductReader reader = CreateReader(client);

        Result<CatalogProductSnapshot> result = await reader.GetActiveProductAsync(productId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.ProductNotFound, result.Error!.Code);
    }

    [Fact]
    public async Task GetActiveProductMapsCatalogFailureToUnavailable()
    {
        using HttpClient client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        CatalogProductReader reader = CreateReader(client);

        Result<CatalogProductSnapshot> result = await reader.GetActiveProductAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(BasketErrorCodes.ProductCatalogUnavailable, result.Error!.Code);
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new StubHttpMessageHandler(responder))
        {
            BaseAddress = new Uri("http://catalog.test", UriKind.Absolute)
        };

    private static CatalogProductReader CreateReader(HttpClient client) =>
        new(client, NullLogger<CatalogProductReader>.Instance);
}
