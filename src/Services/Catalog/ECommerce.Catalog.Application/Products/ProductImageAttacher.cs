using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageAttacher : IProductImageAttacher
{
    private readonly ICloudImageService cloudImageService;

    public ProductImageAttacher(ICloudImageService cloudImageService)
    {
        this.cloudImageService = cloudImageService;
    }

    public async Task<Result> AttachAsync(
        Product product,
        IReadOnlyCollection<ProductImageInput> images,
        CancellationToken cancellationToken)
    {
        foreach (ProductImageInput image in images)
        {
            if (!await cloudImageService.ImageExistsAsync(image.PublicId, cancellationToken))
            {
                return Result.Failure(new Error(ErrorCodes.ValidationFailed, ErrorCodes.ValidationFailed));
            }

            product.AddImage(new ProductImage(
                Guid.NewGuid(),
                product.Id,
                image.PublicId,
                image.Url,
                image.SecureUrl,
                image.Width,
                image.Height,
                image.Format,
                image.SortOrder,
                image.IsMain));
        }

        return Result.Success();
    }
}
