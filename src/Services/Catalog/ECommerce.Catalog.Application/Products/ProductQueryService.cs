using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductQueryService
{
    private readonly IRepository<Product, Guid> repository;
    private readonly IProductSearchReader searchReader;
    private readonly IProductStoreAccessValidator storeAccessValidator;

    public ProductQueryService(
        IRepository<Product, Guid> repository,
        IProductSearchReader searchReader,
        IProductStoreAccessValidator storeAccessValidator)
    {
        this.repository = repository;
        this.searchReader = searchReader;
        this.storeAccessValidator = storeAccessValidator;
    }

    public Task<Result<PagedResult<ProductResponse>>> SearchPublicAsync(
        ProductListQuery query,
        string culture,
        CancellationToken cancellationToken)
    {
        return SearchCoreAsync(
            query with { Status = ProductStatus.Active },
            culture,
            cancellationToken);
    }

    public async Task<Result<PagedResult<ProductResponse>>> SearchManagedAsync(
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

        return await SearchCoreAsync(query, culture, cancellationToken);
    }

    public async Task<Result<ProductResponse>> GetPublicByIdAsync(
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

    public async Task<Result<ProductResponse>> GetManagedByIdAsync(
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
            return Result<ProductResponse>.Failure(
                new Error(
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

    private async Task<Result<PagedResult<ProductResponse>>> SearchCoreAsync(
        ProductListQuery query,
        string culture,
        CancellationToken cancellationToken)
    {
        PagedResult<Product> products = await searchReader.SearchAsync(query, cancellationToken);
        PagedResult<ProductResponse> response = new(
            products.Items.Select(product => product.ToResponse(culture)).ToArray(),
            products.PageNumber,
            products.PageSize,
            products.TotalCount);

        return Result<PagedResult<ProductResponse>>.Success(response);
    }
}
