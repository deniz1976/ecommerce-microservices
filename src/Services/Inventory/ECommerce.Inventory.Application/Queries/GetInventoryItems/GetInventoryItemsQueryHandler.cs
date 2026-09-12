using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.GetInventoryItems;

public sealed class GetInventoryItemsQueryHandler
    : IQueryHandler<GetInventoryItemsQuery, IReadOnlyCollection<InventoryItemResponse>>
{
    private readonly InventoryQueryService queryService;

    public GetInventoryItemsQueryHandler(InventoryQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<IReadOnlyCollection<InventoryItemResponse>> HandleAsync(
        GetInventoryItemsQuery query,
        CancellationToken cancellationToken)
    {
        return queryService.GetManyAsync(query.ProductIds, cancellationToken);
    }
}
