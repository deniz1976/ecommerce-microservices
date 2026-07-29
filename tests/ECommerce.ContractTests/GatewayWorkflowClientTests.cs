using System.Net;
using System.Net.Http.Json;
using ECommerce.RuntimeChecks.Clients;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.ContractTests;

public sealed class GatewayWorkflowClientTests
{
    [Fact]
    public async Task AddBasketItemUsesAuthenticatedGatewayRoute()
    {
        Guid customerId = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                customerId,
                currency = "USD",
                totalAmount = 12.75m,
                updatedAt = DateTimeOffset.UtcNow,
                items = new[]
                {
                    new
                    {
                        productId,
                        productName = "Runtime Checkout Product",
                        quantity = 1,
                        unitPrice = 12.75m,
                        totalPrice = 12.75m,
                        currency = "USD"
                    }
                }
            })
        });
        using HttpClient httpClient = new(handler);
        GatewayWorkflowClient client = new(httpClient, new Uri("https://gateway.test"), "runtime-token");

        RuntimeBasketResponse basket = await client.AddBasketItemAsync(
            customerId,
            new RuntimeAddBasketItemRequest(productId, 1),
            CancellationToken.None);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal($"/gateway/baskets/{customerId}/items", handler.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.AuthenticationScheme);
        Assert.Equal("runtime-token", handler.AuthenticationParameter);
        Assert.Equal(productId, Assert.Single(basket.Items).ProductId);
    }

    [Fact]
    public async Task CheckoutBasketReadsStableSnapshotResponse()
    {
        Guid customerId = Guid.NewGuid();
        Guid checkoutId = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                snapshotId = checkoutId,
                customerId,
                totalAmount = 25.50m,
                currency = "USD",
                createdAt
            })
        });
        using HttpClient httpClient = new(handler);
        GatewayWorkflowClient client = new(httpClient, new Uri("https://gateway.test"), "runtime-token");
        RuntimeCheckoutBasketRequest request = new(
            checkoutId,
            "Runtime Customer",
            "Runtime Avenue 1",
            "Istanbul",
            "TR",
            "34000");

        RuntimeCheckoutBasketResponse checkout = await client.CheckoutBasketAsync(
            customerId,
            request,
            CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal($"/gateway/baskets/{customerId}/checkout", handler.RequestUri!.AbsolutePath);
        Assert.Equal(checkoutId, checkout.SnapshotId);
        Assert.Equal(createdAt, checkout.CreatedAt);
    }

    [Fact]
    public async Task FailureResponseDoesNotExposeRawBody()
    {
        RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(
            HttpStatusCode.InternalServerError)
        {
            Content = JsonContent.Create(new
            {
                code = "PAYMENT_FAILED",
                traceId = "trace-safe-123",
                details = new
                {
                    providerSecret = "must-not-appear"
                }
            })
        });
        using HttpClient httpClient = new(handler);
        GatewayWorkflowClient client = new(
            httpClient,
            new Uri("https://gateway.test"),
            "runtime-token");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetOrderAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("HTTP 500", exception.Message, StringComparison.Ordinal);
        Assert.Contains("code=PAYMENT_FAILED", exception.Message, StringComparison.Ordinal);
        Assert.Contains("traceId=trace-safe-123", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("must-not-appear", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("providerSecret", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("runtime-token", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MalformedFailureResponseDoesNotExposeRawBody()
    {
        RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(
            HttpStatusCode.BadGateway)
        {
            Content = new StringContent("provider-secret-raw-body")
        });
        using HttpClient httpClient = new(handler);
        GatewayWorkflowClient client = new(
            httpClient,
            new Uri("https://gateway.test"),
            "runtime-token");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetOrderAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("HTTP 502", exception.Message, StringComparison.Ordinal);
        Assert.Contains("code=unavailable", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("provider-secret-raw-body", exception.Message, StringComparison.Ordinal);
    }
}
