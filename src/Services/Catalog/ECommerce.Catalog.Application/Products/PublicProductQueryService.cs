using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class PublicProductQueryService(
    IRepository<Product, Guid> repository,
    IProductSearchReader searchReader)
{
    public async Task<Result<PagedResult<ProductResponse>>> SearchAsync(
        ProductListQuery query,
        string culture,
        CancellationToken cancellationToken)
    {
        PagedResult<Product> products = await searchReader.SearchAsync(
            query with { Status = ProductStatus.Active },
            cancellationToken);
        return ProductSearchResultMapper.ToResult(products, culture);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(
        Guid id,
        string culture,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null || product.Status != ProductStatus.Active
            ? Result<ProductResponse>.Failure(
                new Error(ErrorCodes.ProductNotFound, ErrorCodes.ProductNotFound))
            : Result<ProductResponse>.Success(product.ToResponse(culture));
    }
}
