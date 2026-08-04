using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Stores;

public interface IManagedStoreReader
{
    Task<PagedResult<ManagedStoreResponse>> SearchAsync(
        ManagedStoreListCriteria criteria,
        CancellationToken cancellationToken);
}
