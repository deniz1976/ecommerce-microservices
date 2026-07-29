using System.Net;
using System.Text;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Infrastructure.Images;
using Microsoft.Extensions.Options;

namespace ECommerce.ContractTests;

public sealed class CloudImageServiceTests
{
    [Fact]
    public async Task UploadAsync_uses_server_credentials_and_maps_provider_metadata()
    {
        RecordingHttpMessageHandler handler = new(
            _ => JsonResponse(
                """
                {
                  "public_id": "ecommerce/products/item",
                  "url": "http://res.cloudinary.com/demo/image/upload/item.webp",
                  "secure_url": "https://res.cloudinary.com/demo/image/upload/item.webp",
                  "width": 640,
                  "height": 480,
                  "format": "webp"
                }
                """));
        using HttpClient httpClient = new(handler);
        CloudImageService service = CreateService(httpClient);
        await using MemoryStream content = new([0x52, 0x49, 0x46, 0x46]);

        StoredProductImage? result = await service.UploadAsync(
            Guid.NewGuid(),
            new ProductImageUpload(content, "item.webp", "image/webp", content.Length),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("ecommerce/products/item", result.PublicId);
        Assert.Equal("Basic", handler.AuthenticationScheme);
        Assert.False(string.IsNullOrWhiteSpace(handler.AuthenticationParameter));
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.EndsWith("/image/upload", handler.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteAsync_calls_destroy_with_server_credentials()
    {
        RecordingHttpMessageHandler handler = new(_ => JsonResponse("""{"result":"ok"}"""));
        using HttpClient httpClient = new(handler);
        CloudImageService service = CreateService(httpClient);

        bool result = await service.DeleteAsync("ecommerce/products/item", CancellationToken.None);

        Assert.True(result);
        Assert.Equal("Basic", handler.AuthenticationScheme);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.EndsWith("/image/destroy", handler.RequestUri?.AbsoluteUri);
    }

    private static CloudImageService CreateService(HttpClient httpClient)
    {
        CloudinaryOptions options = new()
        {
            CloudName = "demo",
            ApiKey = "test-key",
            ApiSecret = "test-secret"
        };
        return new CloudImageService(httpClient, Options.Create(options));
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
