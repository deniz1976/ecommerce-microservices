namespace ECommerce.Catalog.Application.Images;

public interface IProductImageStorage
{
    Task<StoredProductImage?> UploadAsync(
        Guid productId,
        ProductImageUpload upload,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken);
}
