using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Images;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageUploadService(
    IUnitOfWork unitOfWork,
    ProductImageAccessService accessService,
    ProductImageUploadValidator uploadValidator,
    IProductImageStorage imageStorage)
{
    public async Task<Result<ProductImageResponse>> UploadAsync(
        Guid productId, ProductImageUpload upload, ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Result<Product> productResult = await accessService.GetAuthorizedAsync(
            productId, access, cancellationToken);
        if (productResult.IsFailure) return Result<ProductImageResponse>.Failure(productResult.Error!);
        Product product = productResult.Value!;
        if (!product.CanAddImage) return Failure(CatalogErrorCodes.ProductImageLimitExceeded);
        Result validation = await uploadValidator.ValidateAsync(upload, cancellationToken);
        if (validation.IsFailure) return Result<ProductImageResponse>.Failure(validation.Error!);
        StoredProductImage? stored = await imageStorage.UploadAsync(productId, upload, cancellationToken);
        if (stored is null) return Failure(CatalogErrorCodes.ImageStorageUnavailable);
        ProductImage image = new(Guid.NewGuid(), product.Id, stored.PublicId, stored.Url,
            stored.SecureUrl, stored.Width, stored.Height, stored.Format,
            product.Images.Count, product.Images.Count == 0);
        ProductImageMutationResult mutation = product.AddImage(image);
        if (mutation != ProductImageMutationResult.Applied)
        {
            await imageStorage.DeleteAsync(stored.PublicId, cancellationToken);
            return Failure(mutation == ProductImageMutationResult.LimitExceeded
                ? CatalogErrorCodes.ProductImageLimitExceeded : CatalogErrorCodes.InvalidProductImage);
        }
        try { await unitOfWork.SaveChangesAsync(cancellationToken); }
        catch { await imageStorage.DeleteAsync(stored.PublicId, cancellationToken); throw; }
        return Result<ProductImageResponse>.Success(image.ToResponse());
    }

    private static Result<ProductImageResponse> Failure(string code) =>
        Result<ProductImageResponse>.Failure(new Error(code, code));
}
