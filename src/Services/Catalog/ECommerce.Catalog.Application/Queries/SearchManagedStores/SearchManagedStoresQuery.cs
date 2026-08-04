using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Catalog.Application.Stores;

namespace ECommerce.Catalog.Application.Queries.SearchManagedStores;

public sealed record SearchManagedStoresQuery(ManagedStoreListCriteria Criteria)
    : IQuery<PagedResult<ManagedStoreResponse>>;
