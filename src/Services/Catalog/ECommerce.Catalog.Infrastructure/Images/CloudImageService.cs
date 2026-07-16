using ECommerce.Catalog.Application.Images;
using Microsoft.Extensions.Options;

namespace ECommerce.Catalog.Infrastructure.Images;

public sealed class CloudImageService : ICloudImageService
{
    private readonly CloudinaryOptions options;

    public CloudImageService(IOptions<CloudinaryOptions> options)
    {
        this.options = options.Value;
    }

    public Task<bool> ImageExistsAsync(string publicId, CancellationToken cancellationToken)
    {
        bool hasMetadata = !string.IsNullOrWhiteSpace(publicId);
        bool providerConfigured = !string.IsNullOrWhiteSpace(options.CloudName)
            && !string.IsNullOrWhiteSpace(options.ApiKey)
            && !string.IsNullOrWhiteSpace(options.ApiSecret);

        return Task.FromResult(hasMetadata || providerConfigured);
    }
}
