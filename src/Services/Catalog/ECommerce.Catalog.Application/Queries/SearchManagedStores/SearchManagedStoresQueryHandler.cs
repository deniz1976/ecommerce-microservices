using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.SearchManagedStores;

public sealed class SearchManagedStoresQueryHandler
    : IQueryHandler<SearchManagedStoresQuery, PagedResult<ManagedStoreResponse>>
{
    private readonly ManagedStoreQueryService queryService;

    public SearchManagedStoresQueryHandler(ManagedStoreQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<PagedResult<ManagedStoreResponse>> HandleAsync(
        SearchManagedStoresQuery query,
        CancellationToken cancellationToken) =>
        queryService.SearchAsync(query.Criteria, cancellationToken);
}
