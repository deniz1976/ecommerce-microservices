using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Products;

public sealed class ProductStoreAccessValidator : IProductStoreAccessValidator
{
    private readonly IStoreRepository storeRepository;

    public ProductStoreAccessValidator(IStoreRepository storeRepository)
    {
        this.storeRepository = storeRepository;
    }

    public async Task<Result> ValidateAsync(
        Guid? storeId,
        ProductAccessContext access,
        bool requireStoreForSeller,
        CancellationToken cancellationToken)
    {
        if (!access.IsAdmin && access.UserId is null)
        {
            return Result.Failure(new Error(CatalogErrorCodes.IdentityResolutionFailed, CatalogErrorCodes.IdentityResolutionFailed));
        }

        if (storeId is null)
        {
            return access.IsAdmin || !requireStoreForSeller
                ? Result.Success()
                : Result.Failure(new Error(CatalogErrorCodes.StoreRequired, CatalogErrorCodes.StoreRequired));
        }

        Store? store = await storeRepository.GetByIdAsync(storeId.Value, cancellationToken);
        if (store is null)
        {
            return Result.Failure(new Error(CatalogErrorCodes.StoreNotFound, CatalogErrorCodes.StoreNotFound));
        }

        return access.IsAdmin || store.OwnerUserId == access.UserId
            ? Result.Success()
            : Result.Failure(new Error(CatalogErrorCodes.StoreAccessDenied, CatalogErrorCodes.StoreAccessDenied));
    }
}
