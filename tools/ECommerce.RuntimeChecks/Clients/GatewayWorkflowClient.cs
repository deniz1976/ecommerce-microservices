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

    public Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        return PostAsync<OrderResponse>("/gateway/orders", request, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(path, body, cancellationToken);
        return await ReadSuccessResponseAsync<T>(path, response, cancellationToken);
    }

    private async Task PutAsync(string path, object body, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync(path, body, cancellationToken);
        await EnsureSuccessAsync(path, response, cancellationToken);
    }

    private static async Task<T> ReadSuccessResponseAsync<T>(string path, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(path, response, cancellationToken);

        T? payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException($"{path} returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(string path, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException($"{path} returned {(int)response.StatusCode}: {content}");
    }
}
