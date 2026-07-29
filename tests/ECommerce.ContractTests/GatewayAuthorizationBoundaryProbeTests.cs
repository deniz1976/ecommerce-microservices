using System.Net;
using ECommerce.RuntimeChecks.Probes;

namespace ECommerce.ContractTests;

public sealed class GatewayAuthorizationBoundaryProbeTests
{
    [Fact]
    public async Task SellerStoreProbeRequiresForbiddenResponse()
    {
        RecordingHttpMessageHandler handler = new(
            _ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        using HttpClient httpClient = new(handler);
        GatewayAuthorizationBoundaryProbe probe = new(
            httpClient,
            new Uri("https://gateway.test"),
            "runtime-token");

        await probe.AssertSellerStoreAccessDeniedAsync(CancellationToken.None);

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/gateway/catalog/stores/mine", handler.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.AuthenticationScheme);
        Assert.Equal("runtime-token", handler.AuthenticationParameter);
    }

    [Fact]
    public async Task ProductImageProbeRequiresForbiddenResponse()
    {
        Guid productId = Guid.NewGuid();
        RecordingHttpMessageHandler handler = new(
            _ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        using HttpClient httpClient = new(handler);
        GatewayAuthorizationBoundaryProbe probe = new(
            httpClient,
            new Uri("https://gateway.test"),
            "runtime-token");

        await probe.AssertProductImageUploadDeniedAsync(
            productId,
            CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(
            $"/gateway/catalog/products/{productId}/images",
            handler.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ProbeFailsClosedWhenEndpointProcessesUnauthorizedRequest()
    {
        RecordingHttpMessageHandler handler = new(
            _ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        using HttpClient httpClient = new(handler);
        GatewayAuthorizationBoundaryProbe probe = new(
            httpClient,
            new Uri("https://gateway.test"),
            "runtime-token");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => probe.AssertSellerStoreAccessDeniedAsync(CancellationToken.None));

        Assert.Contains("must return 403", exception.Message, StringComparison.Ordinal);
        Assert.Contains("400", exception.Message, StringComparison.Ordinal);
    }
}
