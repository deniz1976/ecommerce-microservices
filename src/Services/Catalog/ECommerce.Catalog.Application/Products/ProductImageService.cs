using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;
using Microsoft.Extensions.Logging;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageService
{
    private readonly IRepository<Product, Guid> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IProductStoreAccessValidator storeAccessValidator;
    private readonly ProductImageUploadValidator uploadValidator;
    private readonly IProductImageStorage imageStorage;
    private readonly IProductImageDeletionQueue imageDeletionQueue;
    private readonly ILogger<ProductImageService> logger;

    public ProductImageService(
        IRepository<Product, Guid> repository,
        IUnitOfWork unitOfWork,
        IProductStoreAccessValidator storeAccessValidator,
        ProductImageUploadValidator uploadValidator,
        IProductImageStorage imageStorage,
        IProductImageDeletionQueue imageDeletionQueue,
        ILogger<ProductImageService> logger)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.storeAccessValidator = storeAccessValidator;
        this.uploadValidator = uploadValidator;
        this.imageStorage = imageStorage;
        this.imageDeletionQueue = imageDeletionQueue;
        this.logger = logger;
    }

    public async Task<Result<ProductImageResponse>> UploadAsync(
        Guid productId,
        ProductImageUpload upload,
        ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> productResult = await GetAuthorizedProductAsync(productId, access, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result<ProductImageResponse>.Failure(productResult.Error!);
        }

        Product product = productResult.Value!;
        if (!product.CanAddImage)
        {
            return Failure(CatalogErrorCodes.ProductImageLimitExceeded);
        }

        Result validation = await uploadValidator.ValidateAsync(upload, cancellationToken);
        if (validation.IsFailure)
        {
            return Result<ProductImageResponse>.Failure(validation.Error!);
        }

        StoredProductImage? storedImage = await imageStorage.UploadAsync(productId, upload, cancellationToken);
        if (storedImage is null)
        {
            return Failure(CatalogErrorCodes.ImageStorageUnavailable);
        }

        ProductImage image = new(
            Guid.NewGuid(),
            product.Id,
            storedImage.PublicId,
            storedImage.Url,
            storedImage.SecureUrl,
            storedImage.Width,
            storedImage.Height,
            storedImage.Format,
            product.Images.Count,
            product.Images.Count == 0);

        ProductImageMutationResult mutationResult = product.AddImage(image);
        if (mutationResult != ProductImageMutationResult.Applied)
        {
            await imageStorage.DeleteAsync(storedImage.PublicId, cancellationToken);
            return Failure(mutationResult == ProductImageMutationResult.LimitExceeded
                ? CatalogErrorCodes.ProductImageLimitExceeded
                : CatalogErrorCodes.InvalidProductImage);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await imageStorage.DeleteAsync(storedImage.PublicId, cancellationToken);
            throw;
        }

        return Result<ProductImageResponse>.Success(image.ToResponse());
    }

    public async Task<Result<ProductImageResponse>> SetMainAsync(
        Guid productId,
        Guid imageId,
        ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> productResult = await GetAuthorizedProductAsync(productId, access, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result<ProductImageResponse>.Failure(productResult.Error!);
        }

        Product product = productResult.Value!;
        ProductImage? image = product.Images.FirstOrDefault(item => item.Id == imageId);
        if (image is null)
        {
            return Failure(CatalogErrorCodes.ProductImageNotFound);
        }

        product.SetMainImage(imageId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductImageResponse>.Success(image.ToResponse());
    }

    public async Task<Result<ProductImageResponse>> DeleteAsync(
        Guid productId,
        Guid imageId,
        ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> productResult = await GetAuthorizedProductAsync(productId, access, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result<ProductImageResponse>.Failure(productResult.Error!);
        }

        Product product = productResult.Value!;
        ProductImage? image = product.RemoveImage(imageId);
        if (image is null)
        {
            return Failure(CatalogErrorCodes.ProductImageNotFound);
        }

        await imageDeletionQueue.EnqueueAsync(image.PublicId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        bool providerDeleted = await imageStorage.DeleteAsync(image.PublicId, cancellationToken);
        if (providerDeleted)
        {
            try
            {
                await imageDeletionQueue.MarkCompletedAsync(image.PublicId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Product image provider deletion succeeded but cleanup completion persistence failed; durable retry remains.");
            }
        }
        else
        {
            logger.LogWarning(
                "Product image provider deletion was deferred to durable cleanup.");
        }

        return Result<ProductImageResponse>.Success(image.ToResponse());
    }

    private async Task<Result<Product>> GetAuthorizedProductAsync(
        Guid productId,
        ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result<Product>.Failure(new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        if (!access.IsAdmin && product.StoreId is null)
        {
            return Result<Product>.Failure(
                new Error(CatalogErrorCodes.StoreAccessDenied, CatalogErrorCodes.StoreAccessDenied));
        }

        Result storeAccess = await storeAccessValidator.ValidateAsync(
            product.StoreId,
            access,
            requireStoreForSeller: true,
            cancellationToken);

        return storeAccess.IsFailure
            ? Result<Product>.Failure(storeAccess.Error!)
            : Result<Product>.Success(product);
    }

    private static Result<ProductImageResponse> Failure(string code) =>
        Result<ProductImageResponse>.Failure(new Error(code, code));
}
