using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageMainService(
    IUnitOfWork unitOfWork,
    ProductImageAccessService accessService)
{
    public async Task<Result<ProductImageResponse>> SetMainAsync(
        Guid productId, Guid imageId, ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> result = await accessService.GetAuthorizedAsync(productId, access, cancellationToken);
        if (result.IsFailure) return Result<ProductImageResponse>.Failure(result.Error!);
        ProductImage? image = result.Value!.Images.FirstOrDefault(item => item.Id == imageId);
        if (image is null) return Result<ProductImageResponse>.Failure(
            new Error(CatalogErrorCodes.ProductImageNotFound, CatalogErrorCodes.ProductImageNotFound));
        result.Value.SetMainImage(imageId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductImageResponse>.Success(image.ToResponse());
    }
}
