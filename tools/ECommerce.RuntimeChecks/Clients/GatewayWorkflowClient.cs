using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using ECommerce.RuntimeChecks.Models;

namespace ECommerce.RuntimeChecks.Clients;

internal sealed class GatewayWorkflowClient : IGatewayWorkflowClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient httpClient;

    public GatewayWorkflowClient(HttpClient httpClient, Uri gatewayBaseUri, string accessToken)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = gatewayBaseUri;
        this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en");
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public Task<UserResponse> RegisterUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        return PostAsync<UserResponse>("/gateway/users", request, cancellationToken);
    }

    public Task UpsertInventoryAsync(Guid productId, UpsertInventoryRequest request, CancellationToken cancellationToken)
    {
        return PutAsync($"/gateway/inventory/items/{productId}", request, cancellationToken);
    }

    public Task<RuntimeBasketResponse> AddBasketItemAsync(
        Guid customerId,
        RuntimeAddBasketItemRequest request,
        CancellationToken cancellationToken)
    {
        return PutAsync<RuntimeBasketResponse>(
            $"/gateway/baskets/{customerId}/items",
            request,
            cancellationToken);
    }

    public Task<RuntimeCheckoutBasketResponse> CheckoutBasketAsync(
        Guid customerId,
        RuntimeCheckoutBasketRequest request,
        CancellationToken cancellationToken)
    {
        return PostAsync<RuntimeCheckoutBasketResponse>(
            $"/gateway/baskets/{customerId}/checkout",
            request,
            cancellationToken);
    }

    public Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        return PostAsync<OrderResponse>("/gateway/orders", request, cancellationToken);
    }

    public async Task<OrderResponse> RequestOrderCancellationAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Put,
            $"/gateway/orders/{orderId}/cancellation");
        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadSuccessResponseAsync<OrderResponse>(response, cancellationToken);
    }

    public async Task<OrderResponse> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        string path = $"/gateway/orders/{orderId}";
        using HttpResponseMessage response = await httpClient.GetAsync(path, cancellationToken);
        return await ReadSuccessResponseAsync<OrderResponse>(response, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(path, body, cancellationToken);
        return await ReadSuccessResponseAsync<T>(response, cancellationToken);
    }

    private async Task PutAsync(string path, object body, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync(path, body, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<T> PutAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync(path, body, cancellationToken);
        return await ReadSuccessResponseAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadSuccessResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        T? payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException("Gateway returned an empty success response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string summary = await RuntimeErrorSummary.ReadAsync(
            response.Content,
            cancellationToken);
        throw new InvalidOperationException(
            $"Gateway returned HTTP {(int)response.StatusCode} ({summary}).");
    }
}
