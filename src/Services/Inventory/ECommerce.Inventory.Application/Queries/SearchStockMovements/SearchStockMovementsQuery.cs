using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Inventory.Application.Inventory;

namespace ECommerce.Inventory.Application.Queries.SearchStockMovements;

public sealed record SearchStockMovementsQuery(StockMovementListCriteria Criteria)
    : IQuery<PagedResult<StockMovementResponse>>;
