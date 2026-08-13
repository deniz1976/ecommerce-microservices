using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ManagedProductQueryService(
    IRepository<Product, Guid> repository,
    IProductSearchReader searchReader,
    IProductStoreAccessValidator storeAccessValidator)
{
    public async Task<Result<PagedResult<ProductResponse>>> SearchAsync(
        ProductListQuery query,
        ProductAccessContext access,
        string culture,
        CancellationToken cancellationToken)
    {
        Result storeAccess = await storeAccessValidator.ValidateAsync(
            query.StoreId,
            access,
            requireStoreForSeller: true,
            cancellationToken);
        if (storeAccess.IsFailure)
        {
            return Result<PagedResult<ProductResponse>>.Failure(storeAccess.Error!);
        }

        PagedResult<Product> products = await searchReader.SearchAsync(query, cancellationToken);
        return ProductSearchResultMapper.ToResult(products, culture);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(
        Guid id,
        ProductAccessContext access,
        string culture,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductResponse>.Failure(
                new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound));
        }

        if (!access.IsAdmin && product.StoreId is null)
        {
            return Result<ProductResponse>.Failure(new Error(
                CatalogErrorCodes.StoreAccessDenied,
                CatalogErrorCodes.StoreAccessDenied));
        }

        Result storeAccess = await storeAccessValidator.ValidateAsync(
            product.StoreId,
            access,
            requireStoreForSeller: true,
            cancellationToken);
        return storeAccess.IsFailure
            ? Result<ProductResponse>.Failure(storeAccess.Error!)
            : Result<ProductResponse>.Success(product.ToResponse(culture));
    }
}
