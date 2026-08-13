using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;
using Microsoft.Extensions.Logging;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageDeletionService(
    IUnitOfWork unitOfWork,
    ProductImageAccessService accessService,
    IProductImageStorage imageStorage,
    IProductImageDeletionQueue imageDeletionQueue,
    ILogger<ProductImageDeletionService> logger)
{
    public async Task<Result<ProductImageResponse>> DeleteAsync(
        Guid productId, Guid imageId, ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> result = await accessService.GetAuthorizedAsync(productId, access, cancellationToken);
        if (result.IsFailure) return Result<ProductImageResponse>.Failure(result.Error!);
        ProductImage? image = result.Value!.RemoveImage(imageId);
        if (image is null) return Result<ProductImageResponse>.Failure(
            new Error(CatalogErrorCodes.ProductImageNotFound, CatalogErrorCodes.ProductImageNotFound));

        await imageDeletionQueue.EnqueueAsync(image.PublicId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        bool deleted = await imageStorage.DeleteAsync(image.PublicId, cancellationToken);
        if (deleted)
        {
            try { await imageDeletionQueue.MarkCompletedAsync(image.PublicId, cancellationToken); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception)
            {
                logger.LogWarning("Product image provider deletion succeeded but cleanup completion persistence failed; durable retry remains.");
            }
        }
        else
        {
            logger.LogWarning("Product image provider deletion was deferred to durable cleanup.");
        }
        return Result<ProductImageResponse>.Success(image.ToResponse());
    }
}
