using ECommerce.Catalog.Application.Images;

namespace ECommerce.ContractTests;

internal sealed class AlwaysValidImageService : ICloudImageService
{
    public Task<bool> ImageExistsAsync(string publicId, CancellationToken cancellationToken) => Task.FromResult(true);
}
