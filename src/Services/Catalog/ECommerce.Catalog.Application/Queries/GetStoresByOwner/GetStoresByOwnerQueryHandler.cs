using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.GetStoresByOwner;

public sealed class GetStoresByOwnerQueryHandler
    : IQueryHandler<GetStoresByOwnerQuery, Result<IReadOnlyCollection<StoreResponse>>>
{
    private readonly StoreService storeService;

    public GetStoresByOwnerQueryHandler(StoreService storeService)
    {
        this.storeService = storeService;
    }

    public Task<Result<IReadOnlyCollection<StoreResponse>>> HandleAsync(
        GetStoresByOwnerQuery query,
        CancellationToken cancellationToken)
    {
        return storeService.GetMineAsync(query.OwnerUserId, cancellationToken);
    }
}
