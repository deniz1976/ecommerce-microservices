using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductImageAccessService(
    IRepository<Product, Guid> repository,
    IProductStoreAccessValidator storeAccessValidator)
{
    public async Task<Result<Product>> GetAuthorizedAsync(
        Guid productId,
        ProductAccessContext access,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result<Product>.Failure(
                new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        if (!access.IsAdmin && product.StoreId is null)
        {
            return Failure();
        }

        Result storeAccess = await storeAccessValidator.ValidateAsync(
            product.StoreId, access, requireStoreForSeller: true, cancellationToken);
        return storeAccess.IsFailure
            ? Result<Product>.Failure(storeAccess.Error!)
            : Result<Product>.Success(product);
    }

    private static Result<Product> Failure() => Result<Product>.Failure(
        new Error(CatalogErrorCodes.StoreAccessDenied, CatalogErrorCodes.StoreAccessDenied));
}
