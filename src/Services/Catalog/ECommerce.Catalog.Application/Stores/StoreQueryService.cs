using ECommerce.BuildingBlocks.Contracts.Errors;
using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Domain;

namespace ECommerce.Catalog.Application.Stores;

public sealed class StoreQueryService(
    IRepository<Store, Guid> repository,
    IStoreReader storeReader)
{
    public async Task<Result<StoreResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Store? store = await repository.GetByIdAsync(id, cancellationToken);
        return store is null
            ? Result<StoreResponse>.Failure(new Error(
                CatalogErrorCodes.StoreNotFound,
                CatalogErrorCodes.StoreNotFound))
            : Result<StoreResponse>.Success(StoreResponseMapper.ToResponse(store));
    }

    public async Task<Result<IReadOnlyCollection<StoreResponse>>> GetByOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Store> stores = await storeReader.GetByOwnerAsync(
            ownerUserId,
            cancellationToken);
        return Result<IReadOnlyCollection<StoreResponse>>.Success(
            stores.Select(StoreResponseMapper.ToResponse).ToArray());
    }
}
