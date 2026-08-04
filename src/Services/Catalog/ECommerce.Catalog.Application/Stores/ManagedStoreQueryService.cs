using ECommerce.BuildingBlocks.Contracts.Results;

namespace ECommerce.Catalog.Application.Stores;

public sealed class ManagedStoreQueryService
{
    private readonly IManagedStoreReader storeReader;

    public ManagedStoreQueryService(IManagedStoreReader storeReader)
    {
        this.storeReader = storeReader;
    }

    public Task<PagedResult<ManagedStoreResponse>> SearchAsync(
        ManagedStoreListCriteria criteria,
        CancellationToken cancellationToken) =>
        storeReader.SearchAsync(criteria, cancellationToken);
}
