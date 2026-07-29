using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ECommerce.Catalog.Application.Images;
using Microsoft.Extensions.Options;

namespace ECommerce.Catalog.Infrastructure.Images;

public sealed class CloudImageService : IProductImageStorage
{
    private readonly HttpClient httpClient;
    private readonly CloudinaryOptions options;

    public CloudImageService(HttpClient httpClient, IOptions<CloudinaryOptions> options)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
    }

    public async Task<StoredProductImage?> UploadAsync(
        Guid productId,
        ProductImageUpload upload,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            return null;
        }

        string endpoint = BuildEndpoint("upload");
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);
        request.Headers.Authorization = CreateAuthorization();

        using MultipartFormDataContent form = new();
        using StreamContent fileContent = new(upload.Content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(upload.ContentType);
        form.Add(fileContent, "file", Path.GetFileName(upload.FileName));
        form.Add(new StringContent($"ecommerce/products/{productId:N}"), "folder");
        form.Add(new StringContent("false"), "use_filename");
        form.Add(new StringContent("true"), "unique_filename");
        form.Add(new StringContent("false"), "overwrite");
        request.Content = form;

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            CloudinaryUploadResponse? payload = await response.Content.ReadFromJsonAsync<CloudinaryUploadResponse>(
                cancellationToken: cancellationToken);
            return IsValid(payload)
                ? new StoredProductImage(
                    payload!.PublicId,
                    payload.Url,
                    payload.SecureUrl,
                    payload.Width,
                    payload.Height,
                    payload.Format)
                : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        using HttpRequestMessage request = new(HttpMethod.Post, BuildEndpoint("destroy"));
        request.Headers.Authorization = CreateAuthorization();
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("public_id", publicId),
            new KeyValuePair<string, string>("invalidate", "true")
        ]);

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            CloudinaryDestroyResponse? payload = await response.Content.ReadFromJsonAsync<CloudinaryDestroyResponse>(
                cancellationToken: cancellationToken);
            return payload?.Result is "ok" or "not found";
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private string BuildEndpoint(string action) =>
        $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(options.CloudName!)}/image/{action}";

    private AuthenticationHeaderValue CreateAuthorization()
    {
        string credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{options.ApiKey}:{options.ApiSecret}"));
        return new AuthenticationHeaderValue("Basic", credentials);
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(options.CloudName) &&
        !string.IsNullOrWhiteSpace(options.ApiKey) &&
        !string.IsNullOrWhiteSpace(options.ApiSecret);

    private static bool IsValid(CloudinaryUploadResponse? payload) =>
        payload is not null &&
        !string.IsNullOrWhiteSpace(payload.PublicId) &&
        !string.IsNullOrWhiteSpace(payload.Url) &&
        Uri.TryCreate(payload.SecureUrl, UriKind.Absolute, out Uri? secureUri) &&
        secureUri.Scheme == Uri.UriSchemeHttps &&
        payload.Width > 0 &&
        payload.Height > 0 &&
        !string.IsNullOrWhiteSpace(payload.Format);
}
