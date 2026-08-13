using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.GetStoreById;

public sealed class GetStoreByIdQueryHandler
    : IQueryHandler<GetStoreByIdQuery, Result<StoreResponse>>
{
    private readonly StoreQueryService storeService;

    public GetStoreByIdQueryHandler(StoreQueryService storeService)
    {
        this.storeService = storeService;
    }

    public Task<Result<StoreResponse>> HandleAsync(
        GetStoreByIdQuery query,
        CancellationToken cancellationToken)
    {
        return storeService.GetByIdAsync(query.Id, cancellationToken);
    }
}
