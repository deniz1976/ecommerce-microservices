using System.Net;
using System.Net.Http.Headers;

namespace ECommerce.RuntimeChecks.Probes;

internal sealed class GatewayAuthorizationBoundaryProbe : IAuthorizationBoundaryProbe
{
    private readonly HttpClient httpClient;

    public GatewayAuthorizationBoundaryProbe(
        HttpClient httpClient,
        Uri gatewayBaseUri,
        string accessToken)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = gatewayBaseUri;
        this.httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task AssertSellerStoreAccessDeniedAsync(
        CancellationToken cancellationToken)
    {
        await AssertForbiddenAsync(
            new HttpRequestMessage(HttpMethod.Get, "/gateway/catalog/stores/mine"),
            "seller store read",
            cancellationToken);
    }

    public async Task AssertProductImageUploadDeniedAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"/gateway/catalog/products/{productId}/images");
        request.Content = new MultipartFormDataContent();

        await AssertForbiddenAsync(
            request,
            "seller product image upload",
            cancellationToken);
    }

    private async Task AssertForbiddenAsync(
        HttpRequestMessage request,
        string operation,
        CancellationToken cancellationToken)
    {
        using (request)
        using (HttpResponseMessage response = await httpClient.SendAsync(
                   request,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken))
        {
            if (response.StatusCode != HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    $"{operation} must return 403 for the least-privileged runtime M2M token, but returned {(int)response.StatusCode}.");
            }
        }
    }
}
