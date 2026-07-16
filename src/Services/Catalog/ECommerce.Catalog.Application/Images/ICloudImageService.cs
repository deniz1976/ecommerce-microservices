namespace ECommerce.Catalog.Application.Images;

public interface ICloudImageService
{
    Task<bool> ImageExistsAsync(string publicId, CancellationToken cancellationToken);
}
