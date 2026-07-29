using ECommerce.Catalog.Application.Images;

namespace ECommerce.ContractTests;

internal sealed class TestProductImageStorage : IProductImageStorage
{
    public int UploadCount { get; private set; }

    public List<string> DeletedPublicIds { get; } = [];

    public bool DeleteSucceeds { get; set; } = true;

    public Task<StoredProductImage?> UploadAsync(
        Guid productId,
        ProductImageUpload upload,
        CancellationToken cancellationToken)
    {
        UploadCount++;
        StoredProductImage image = new(
            $"ecommerce/products/{productId:N}/test",
            "http://res.cloudinary.com/demo/image/upload/test.png",
            "https://res.cloudinary.com/demo/image/upload/test.png",
            800,
            600,
            "png");
        return Task.FromResult<StoredProductImage?>(image);
    }

    public Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken)
    {
        DeletedPublicIds.Add(publicId);
        return Task.FromResult(DeleteSucceeds);
    }
}
