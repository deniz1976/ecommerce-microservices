using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.BuildingBlocks.Contracts.Results;
using ECommerce.Ordering.Application.Orders;
using ECommerce.Ordering.Domain;

namespace ECommerce.Ordering.Application.Queries.SearchManagedOrders;

public sealed record SearchManagedOrdersQuery(
    Guid? CustomerId,
    int PageNumber,
    int PageSize,
    OrderStatus? Status,
    bool SortDescending)
    : IQuery<Result<PagedResult<OrderSummaryResponse>>>;
