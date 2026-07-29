using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.GetStoreById;

public sealed class GetStoreByIdQueryHandler
    : IQueryHandler<GetStoreByIdQuery, Result<StoreResponse>>
{
    private readonly StoreService storeService;

    public GetStoreByIdQueryHandler(StoreService storeService)
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
